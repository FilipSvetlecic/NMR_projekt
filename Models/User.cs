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
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = "";

        [Column("email")]
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = "";

        [Column("password")]
        [MaxLength(255)]
        public string Password { get; set; } = "";

        [Column("full_name")]
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Column("phone")]
        [MaxLength(50)]
        public string? Phone { get; set; }

        public List<Drone> Drones { get; set; } = new List<Drone>();
    }
}
