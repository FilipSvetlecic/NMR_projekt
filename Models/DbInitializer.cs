using DroneCatalog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;


namespace NMR_projekt.Models
{
    public class DbInitializer
    {
        private static AppDbContext context;

        public static AppDbContext GetContext()
        {
            if (context == null)
            {
                context = new AppDbContext();
                context.Database.EnsureCreated();
                SeedDatabase(context);
            }
            return context;
        }

        private static void SeedDatabase(AppDbContext db)
        {
            
            
            if (db.Users.Any())
            {
                return;
            }


            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@droneshop.com",
                FullName = "Administrator",
                Phone = "+385 91 234 5678"
            };
            db.Users.Add(adminUser);
            db.SaveChanges();

            var drones = new List<Drone>
            {
                new Drone
                {
                    DroneName = "DJI Mavic 3 Pro",
                    Price = 2199.00M,
                    Description = "Professional drone with triple camera system and advanced obstacle avoidance.",
                    ColorOptions = "Black,Gray",
                    AvailableShops = "Zagreb - Ulica Kralja Tomislava IV,Split - Marmontova 15,Rijeka - Korzo 22",
                    ImagePath = "/Images/c2394437-3715-48d9-9ca9-5fab0ea8c746.jpg",
                    Manufacturer = "DJI",
                    FlightTime = 43,
                    MaxRange = 15000,
                    MaxSpeed = 75.6M,
                    Weight = 895,
                    CameraResolution = "4K/120fps",
                    UserId = adminUser.UserId
                },
                new Drone
                {
                    DroneName = "DJI Mini 3",
                    Price = 669.00M,
                    Description = "Compact and lightweight drone perfect for beginners and travel.",
                    ColorOptions = "Black,Orange,Blue",
                    AvailableShops = "Zagreb - Ulica Kralja Tomislava IV,Osijek - Europska avenija 24",
                    ImagePath = "/Images/1e648302-3e18-4d79-8a5e-a6c46b96f422.jpg",
                    Manufacturer = "DJI",
                    FlightTime = 38,
                    MaxRange = 10000,
                    MaxSpeed = 57.6M,
                    Weight = 249,
                    CameraResolution = "4K/30fps",
                    UserId = adminUser.UserId
                },
                new Drone
                {
                    DroneName = "Autel EVO Lite+",
                    Price = 1049.00M,
                    Description = "Versatile drone with excellent image quality and intelligent flight modes.",
                    ColorOptions = "Orange,White",
                    AvailableShops = "Split - Marmontova 15,Zadar - Široka ulica 8",
                    ImagePath = "/Images/01ecdecd-44e5-42b3-8de8-b18965157768.jpg",
                    Manufacturer = "Autel Robotics",
                    FlightTime = 40,
                    MaxRange = 12000,
                    MaxSpeed = 60.0M,
                    Weight = 835,
                    CameraResolution = "6K/30fps",
                    UserId = adminUser.UserId
                }
            };

            db.Drones.AddRange(drones);
            db.SaveChanges();

        }
    }
}
