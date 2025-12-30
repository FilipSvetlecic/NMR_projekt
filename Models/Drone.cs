using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMR_projekt.Models;

namespace NMR_projekt.Models
{
    [Table("Drones")]
    public class Drone
    {
        [Key]
        [Column("drone_id")]
        public int DroneId { get; set; }

        [Column("drone_name")]
        [Required]
        [MaxLength(200)]
        public string DroneName { get; set; } = "";

        [Column("price")]
        [Required]
        public decimal Price { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("color_options")]
        [MaxLength(500)]
        public string? ColorOptions { get; set; }

        [Column("available_shops")]
        public string? AvailableShops { get; set; }

        [Column("image_path")]
        [MaxLength(500)]
        public string? ImagePath { get; set; }

        [Column("manufacturer")]
        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [Column("flight_time")]
        public int? FlightTime { get; set; }

        [Column("max_range")]
        public int? MaxRange { get; set; }

        [Column("max_speed")]
        public decimal? MaxSpeed { get; set; }

        [Column("weight")]
        public decimal? Weight { get; set; }

        [Column("camera_resolution")]
        [MaxLength(50)]
        public string? CameraResolution { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
