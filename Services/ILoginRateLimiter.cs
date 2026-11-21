using System;

namespace SistemaReservas.Services
{
    public interface ILoginRateLimiter
    {
        /// <summary>
        /// Comprueba si una dirección IP está actualmente bloqueada por demasiados intentos.
        /// </summary>
        (bool IsBlocked, TimeSpan? RetryAfter) CheckIpThrottled(string ipAddress);

        /// <summary>
        /// Registra un intento fallido proveniente de una IP.
        /// </summary>
        (bool IsNowBlocked, int FailedAttempts, int RemainingAttempts, TimeSpan? LockDuration) RecordFailedAttempt(string ipAddress);

        /// <summary>
        /// Reinicia el contador de intentos de la IP cuando hay un login exitoso.
        /// </summary>
        void ResetAttempts(string ipAddress);
    }
}
