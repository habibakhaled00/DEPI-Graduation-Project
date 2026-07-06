using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.Models
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Address { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [StringLength(50)]
        public string? District { get; set; }
    }
}
