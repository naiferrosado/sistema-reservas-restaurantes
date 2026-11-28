using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace SistemaReservas.Services
{
    /// <summary>
    /// Política centralizada de complejidad de contraseñas y utilidades criptográficas.
    /// </summary>
    public static class PasswordPolicy
    {
        public const int MinLength = 8;
        public const int MaxLength = 100;

        public const string RequirementsSummary =
            "Debe tener al menos 8 caracteres e incluir una letra mayúscula, una minúscula, un número y un carácter especial.";

        /// <summary>
        /// Valida que la contraseña cumpla con la longitud mínima (8) y complejidad de caracteres.
        /// </summary>
        public static (bool IsValid, string ErrorMessage) Validate(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "La contraseña es obligatoria.");
            }

            if (password.Length < MinLength)
            {
                return (false, $"La contraseña debe tener al menos {MinLength} caracteres.");
            }

            if (password.Length > MaxLength)
            {
                return (false, $"La contraseña no puede exceder {MaxLength} caracteres.");
            }

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsControl(c))
                {
                    return (false, "La contraseña contiene caracteres de control no permitidos.");
                }

                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsWhiteSpace(c)) hasSpecial = true;
            }

            if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
            {
                var missing = new List<string>();
                if (!hasUpper) missing.Add("una letra mayúscula");
                if (!hasLower) missing.Add("una letra minúscula");
                if (!hasDigit) missing.Add("un número");
                if (!hasSpecial) missing.Add("un carácter especial (ej. !@#$%&*)");

                return (false, $"La contraseña debe incluir al menos: {string.Join(", ", missing)}.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Genera el hash SHA-256 en hexadecimal minúscula para almacenar tokens de forma segura en BD.
        /// </summary>
        public static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Atributo de validación para exigir contraseñas seguras (mínimo 8 caracteres y 4 tipos de caracteres).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string;
            var (isValid, errorMessage) = PasswordPolicy.Validate(password);
            return isValid
                ? ValidationResult.Success
                : new ValidationResult(errorMessage);
        }
    }
}
