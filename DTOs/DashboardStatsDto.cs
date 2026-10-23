namespace SistemaReservas.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalReservationsToday { get; set; }
        public int ExpectedGuestsToday { get; set; }
        public int ConfirmedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalTables { get; set; }
        public int OccupiedTables { get; set; }
        public int AvailableTables { get; set; }
        public double OccupancyRate { get; set; } // En porcentaje: ej. 65.5%
        public List<HourlyOccupancyDto> HourlyOccupancy { get; set; } = new();
    }

    public class HourlyOccupancyDto
    {
        public string TimeSlot { get; set; } = string.Empty;
        public int ReservationsCount { get; set; }
        public int GuestsCount { get; set; }
    }
}
