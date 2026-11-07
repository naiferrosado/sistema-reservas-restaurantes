using System;

namespace SistemaReservas.Services
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo DominicanTimeZone;

        static DateTimeHelper()
        {
            try
            {
                // En Windows la zona horaria UTC-4 de Santo Domingo es "SA Western Standard Time"
                DominicanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            }
            catch
            {
                try
                {
                    // En Linux/macOS
                    DominicanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");
                }
                catch
                {
                    DominicanTimeZone = TimeZoneInfo.CreateCustomTimeZone("DominicanTime", TimeSpan.FromHours(-4), "Dominican Time", "Dominican Time");
                }
            }
        }

        /// <summary>
        /// Obtiene la fecha y hora actual en la República Dominicana (UTC-4)
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DominicanTimeZone);

        /// <summary>
        /// Obtiene la fecha actual (a las 00:00:00) en República Dominicana
        /// </summary>
        public static DateTime Today => Now.Date;
    }
}

