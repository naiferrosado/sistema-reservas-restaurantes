using System;
using System.Collections.Concurrent;

namespace SistemaReservas.Services
{
    public class IpRateLimitInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime FirstFailedAt { get; set; }
        public DateTime? BlockedUntil { get; set; }
    }

    public class LoginRateLimiter : ILoginRateLimiter
    {
        private readonly ConcurrentDictionary<string, IpRateLimitInfo> _ipCache = new();

        // 5 intentos fallidos máximos en una ventana de 5 minutos
        public const int MaxAttempts = 5;
        public static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);

        public (bool IsBlocked, TimeSpan? RetryAfter) CheckIpThrottled(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return (false, null);

            var now = DateTime.UtcNow;

            if (_ipCache.TryGetValue(ipAddress, out var info))
            {
                if (info.BlockedUntil.HasValue)
                {
                    if (now < info.BlockedUntil.Value)
                    {
                        return (true, info.BlockedUntil.Value - now);
                    }
                    else
                    {
                        // Periodo de bloqueo cumplido, remover de la memoria
                        _ipCache.TryRemove(ipAddress, out _);
                        return (false, null);
                    }
                }

                // Si expiró la ventana de intentos sin haber sido bloqueado
                if (now - info.FirstFailedAt > AttemptWindow)
                {
                    _ipCache.TryRemove(ipAddress, out _);
                    return (false, null);
                }
            }

            return (false, null);
        }

        public (bool IsNowBlocked, int FailedAttempts, int RemainingAttempts, TimeSpan? LockDuration) RecordFailedAttempt(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return (false, 1, MaxAttempts - 1, null);

            var now = DateTime.UtcNow;

            var info = _ipCache.AddOrUpdate(ipAddress,
                _ => new IpRateLimitInfo
                {
                    FailedAttempts = 1,
                    FirstFailedAt = now
                },
                (_, existing) =>
                {
                    if (now - existing.FirstFailedAt > AttemptWindow)
                    {
                        existing.FailedAttempts = 1;
                        existing.FirstFailedAt = now;
                        existing.BlockedUntil = null;
                    }
                    else
                    {
                        existing.FailedAttempts++;
                    }
                    return existing;
                });

            if (info.FailedAttempts >= MaxAttempts)
            {
                info.BlockedUntil = now.Add(LockoutDuration);
                return (true, info.FailedAttempts, 0, LockoutDuration);
            }

            int remaining = Math.Max(0, MaxAttempts - info.FailedAttempts);
            return (false, info.FailedAttempts, remaining, null);
        }

        public void ResetAttempts(string ipAddress)
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                _ipCache.TryRemove(ipAddress, out _);
            }
        }
    }
}
