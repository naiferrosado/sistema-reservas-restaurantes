using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.DTOs
{
    public class CreateTableDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public int TableNumber { get; set; }

        [Range(1, 24)]
        public int Capacity { get; set; }

        public string Location { get; set; } = "Salón Principal";
        public string Status { get; set; } = "Disponible";
        public int PosX { get; set; } = 1;
        public int PosY { get; set; } = 1;
    }

    public class UpdateTableStatusDto
    {
        [Required]
        public string Status { get; set; } = "Disponible"; // Disponible, Reservada, Ocupada, Mantenimiento
    }
}
