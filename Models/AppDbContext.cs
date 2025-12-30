using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NMR_projekt.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DroneCatalog.Models
{
    public class AppDbContext : DbContext
    {
        private static readonly SqliteConnection connection =
            new SqliteConnection("Data Source=InMemoryDroneDB;Mode=Memory;Cache=Shared");

        public DbSet<Drone> Drones { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext()
        {
            connection.Open();
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Drone>()
                .HasOne(d => d.User)
                .WithMany(u => u.Drones)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    
    
}