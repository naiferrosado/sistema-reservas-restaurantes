using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.Models;
using SistemaReservas.Services;
using SistemaReservas.ViewModels;

namespace SistemaReservas.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILoginRateLimiter _rateLimiter;
        private readonly ILogger<AccountController> _logger;

        private static readonly PasswordHasher<string> _dummyHasher = new();
        private static readonly string _dummyHash = _dummyHasher.HashPassword("security_timing_protection@internal.local", "DummyTimingSecret123!");

        public AccountController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILoginRateLimiter rateLimiter,
            ILogger<AccountController> logger)
        {
            _context = context;
            _emailService = emailService;
            _rateLimiter = rateLimiter;
            _logger = logger;
        }

        // GET /Account/Login?returnUrl=...
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        // POST /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            // 1. CAPA DE SEGURIDAD: Rate Limiting por IP (Protege la Base de Datos contra saturación/DoS)
            // Se valida en memoria RAM antes de realizar cualquier consulta SQL
            var (isIpBlocked, retryAfter) = _rateLimiter.CheckIpThrottled(ipAddress);
            if (isIpBlocked && retryAfter.HasValue)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling(retryAfter.Value.TotalMinutes));
                ModelState.AddModelError("", $"Demasiados intentos fallidos detectados desde esta red. Por seguridad del servidor y protección de la base de datos, el acceso se encuentra suspendido temporalmente por {minutes} minuto(s).");
                return View(model);
            }

            var cleanEmail = model.Email.Trim().ToLowerInvariant();
            var emailThrottleKey = $"email:{cleanEmail}";

            // Verificar también rate limit por cuenta (protege cuentas administrativas contra ataques distribuidos)
            var (isEmailBlocked, emailRetryAfter) = _rateLimiter.CheckIpThrottled(emailThrottleKey);
            if (isEmailBlocked && emailRetryAfter.HasValue)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling(emailRetryAfter.Value.TotalMinutes));
                ModelState.AddModelError("", $"Demasiados intentos fallidos para esta cuenta. Por seguridad, el acceso se encuentra suspendido temporalmente por {minutes} minuto(s).");
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

            bool isValid = false;
            var hasher = new PasswordHasher<string>();

            // 2. CAPA DE SEGURIDAD: Usuario no existente -> Anti-Enumeración y Anti-Timing Attack
            if (user == null)
            {
                // Ejecución criptográfica dummy para que el tiempo de respuesta sea idéntico al de un usuario existente
                _dummyHasher.VerifyHashedPassword("security_timing_protection@internal.local", _dummyHash, model.Password);

                _rateLimiter.RecordFailedAttempt(emailThrottleKey);
                var (isNowBlocked, failedCount, remainingAttempts, lockDuration) = _rateLimiter.RecordFailedAttempt(ipAddress);
                if (isNowBlocked)
                {
                    var mins = lockDuration.HasValue ? (int)lockDuration.Value.TotalMinutes : 10;
                    ModelState.AddModelError("", $"Has superado el límite de intentos fallidos. Tu acceso ha sido bloqueado temporalmente por {mins} minutos.");
                }
                else
                {
                    // Mensaje idéntico al de usuario existente para evitar enumeración de cuentas
                    ModelState.AddModelError("", $"Credenciales no válidas. Revisa tu correo y contraseña. Te quedan {remainingAttempts} intento(s) antes del bloqueo por seguridad.");
                }
                return View(model);
            }

            // 3. CAPA DE SEGURIDAD: Cuenta inactiva o bloqueada
            if (!user.IsActive)
            {
                _dummyHasher.VerifyHashedPassword("security_timing_protection@internal.local", _dummyHash, model.Password);
                if (user.FailedLoginAttempts >= 5)
                {
                    ModelState.AddModelError("", "Esta cuenta ha sido bloqueada por seguridad tras exceder el número máximo de intentos fallidos. Comunícate con un administrador.");
                }
                else
                {
                    ModelState.AddModelError("", "Esta cuenta se encuentra inactiva. Comunícate con el administrador del sistema.");
                }
                return View(model);
            }

            // 4. Verificación de contraseña para usuario activo
            var verifyResult = hasher.VerifyHashedPassword(cleanEmail, user.PasswordHash, model.Password);
            if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                isValid = true;
                _rateLimiter.ResetAttempts(ipAddress);
                _rateLimiter.ResetAttempts(emailThrottleKey);

                bool needsSave = false;
                if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.PasswordHash = hasher.HashPassword(cleanEmail, model.Password);
                    needsSave = true;
                }
                if (user.FailedLoginAttempts > 0)
                {
                    user.FailedLoginAttempts = 0;
                    needsSave = true;
                }
                if (needsSave)
                {
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Contraseña incorrecta
                var (isNowBlocked, failedCount, ipRemaining, lockDuration) = _rateLimiter.RecordFailedAttempt(ipAddress);
                var (isEmailNowBlocked, _, emailRemaining, emailLockDuration) = _rateLimiter.RecordFailedAttempt(emailThrottleKey);
                bool isAdministrative = user.Role == "Administrador" || user.Role == "Gerente / Encargado";

                if (isNowBlocked || isEmailNowBlocked)
                {
                    var effectiveDuration = isNowBlocked ? lockDuration : emailLockDuration;
                    var mins = effectiveDuration.HasValue ? (int)effectiveDuration.Value.TotalMinutes : 10;
                    ModelState.AddModelError("", $"Has superado el límite de intentos fallidos. Tu acceso ha sido bloqueado temporalmente por {mins} minutos.");
                    return View(model);
                }

                if (!isAdministrative)
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsActive = false;
                        await _context.SaveChangesAsync();
                        _logger.LogWarning("Cuenta operativa bloqueada por 5 intentos fallidos consecutivos: {Email}", user.Email);
                        ModelState.AddModelError("", "Tu cuenta ha sido desactivada y bloqueada por seguridad tras 5 intentos fallidos consecutivos. Contacta a un administrador para reactivarla.");
                        return View(model);
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                        int accountRemaining = 5 - user.FailedLoginAttempts;
                        int effectiveRemaining = Math.Min(accountRemaining, Math.Min(ipRemaining, emailRemaining));
                        ModelState.AddModelError("", $"Credenciales no válidas. Revisa tu correo y contraseña. Te quedan {effectiveRemaining} intento(s) antes del bloqueo por seguridad.");
                        return View(model);
                    }
                }
                else
                {
                    int effectiveRemaining = Math.Min(ipRemaining, emailRemaining);
                    ModelState.AddModelError("", $"Credenciales no válidas. Revisa tu correo y contraseña. Te quedan {effectiveRemaining} intento(s) antes del bloqueo temporal por seguridad.");
                    return View(model);
                }
            }

            if (!isValid || user == null)
            {
                ModelState.AddModelError("", "Credenciales no válidas. Revisa tu correo y contraseña.");
                return View(model);
            }

            // Si la contraseña actual (legacy) no cumple con la nueva política (mín. 8 caracteres y complejidad),
            // redirigir al cambio obligatorio antes de otorgar acceso al panel.
            var (meetsPasswordPolicy, _) = PasswordPolicy.Validate(model.Password);
            if (!meetsPasswordPolicy)
            {
                var upgradeBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
                var upgradeToken = Convert.ToHexString(upgradeBytes).ToLowerInvariant();
                user.ResetToken = PasswordPolicy.HashToken(upgradeToken);
                user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                TempData["UpgradeNotice"] = "Por seguridad, tu contraseña actual no cumple con la nueva política (mínimo 8 caracteres, mayúscula, minúscula, número y símbolo). Actualízala ahora para ingresar.";
                return RedirectToAction(nameof(ResetPassword), new { token = upgradeToken, email = user.Email });
            }

            // Crear los Claims de autenticación
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe 
                    ? DateTimeOffset.UtcNow.AddDays(7) 
                    : DateTimeOffset.UtcNow.AddHours(8),
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Usuario autenticado: {Email} ({Role})", user.Email, user.Role);
            TempData["SuccessMessage"] = $"Bienvenido/a al panel administrativo, {user.FullName}.";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // =====================================================================
        // RECUPERACIÓN DE CONTRASEÑA
        // =====================================================================

        // GET /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new ForgotPasswordViewModel());
        }

        // POST /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var forgotKey = $"forgot:{ipAddress}";
            var (isThrottled, retryAfter) = _rateLimiter.CheckIpThrottled(forgotKey);
            if (isThrottled && retryAfter.HasValue)
            {
                var mins = Math.Max(1, (int)Math.Ceiling(retryAfter.Value.TotalMinutes));
                ModelState.AddModelError("", $"Has realizado demasiadas solicitudes de recuperación. Intenta nuevamente en {mins} minuto(s).");
                return View(model);
            }
            _rateLimiter.RecordFailedAttempt(forgotKey);

            var cleanEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

            if (user != null && user.IsActive)
            {
                // Generar token único criptográficamente seguro y almacenar solo su hash SHA-256 en BD
                var tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
                var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

                user.ResetToken = PasswordPolicy.HashToken(token);
                user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme)
                    ?? $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?token={token}&email={Uri.EscapeDataString(user.Email)}";

                try
                {
                    await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetLink, 30);
                    _logger.LogInformation("Enlace de restablecimiento generado y enviado a {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo de recuperación a {Email}", user.Email);
                }
            }

            // Mensaje de respuesta limpio y seguro (para evitar enumeración de correos)
            ViewBag.Submitted = true;
            ViewBag.SuccessMessage = "Si el correo electrónico está registrado en el sistema, recibirás un enlace para restablecer tu contraseña en los próximos minutos. Por favor revisa tu bandeja de entrada o spam.";

            return View(model);
        }

        // GET /Account/ResetPassword?token=...&email=...
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? token, string? email)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.InvalidToken = true;
                ViewBag.ErrorMessage = "El enlace de recuperación es inválido o no contiene un token válido.";
                return View(new ResetPasswordViewModel());
            }

            var tokenHash = PasswordPolicy.HashToken(token.Trim());
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == tokenHash && u.IsActive);

            if (user == null || user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt < DateTime.UtcNow ||
                (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.InvalidToken = true;
                ViewBag.ErrorMessage = "El enlace de recuperación ha expirado o ya ha sido utilizado. Por favor solicita uno nuevo.";
                return View(new ResetPasswordViewModel { Token = token, Email = email ?? string.Empty });
            }

            return View(new ResetPasswordViewModel
            {
                Token = token,
                Email = user.Email
            });
        }

        // POST /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tokenHash = PasswordPolicy.HashToken(model.Token.Trim());
            var cleanEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == tokenHash && u.Email.ToLower() == cleanEmail && u.IsActive);

            if (user == null || user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
            {
                ViewBag.InvalidToken = true;
                ViewBag.ErrorMessage = "El enlace de recuperación ha expirado o ya no es válido. Por favor solicita uno nuevo.";
                return View(model);
            }

            var hasher = new PasswordHasher<string>();
            user.PasswordHash = hasher.HashPassword(cleanEmail, model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiresAt = null;
            user.FailedLoginAttempts = 0;

            await _context.SaveChangesAsync();
            _rateLimiter.ResetAttempts($"email:{cleanEmail}");

            _logger.LogInformation("Contraseña actualizada exitosamente para el usuario {Email}", user.Email);
            TempData["SuccessMessage"] = "Tu contraseña ha sido restablecida exitosamente. Ya puedes iniciar sesión con tu nueva clave.";

            return RedirectToAction(nameof(Login));
        }

        // POST /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Has cerrado sesión correctamente.";
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "No tienes los permisos necesarios para acceder a este recurso.";
            return RedirectToAction(nameof(Login));
        }
    }
}
