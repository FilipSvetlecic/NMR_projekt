using DroneCatalog.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using NMR_projekt.Models;
using System.Text;

namespace NMR_projekt.Controllers
{
    public static class GetHandler
    {
        public static string GetData(string urlRoute, string urlParams, string responseRoute)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            projectRoot = projectRoot.Replace("bin\\Debug", "");
            string viewsDirectory = Path.Combine(projectRoot, "Views");
            if (urlRoute.Contains("addNewDrone"))
            {
                return File.ReadAllText(responseRoute);
            }
            else if (urlRoute.Contains("masterDetail"))
            {
                var drone = GetOneDrone(int.Parse(urlParams.Split('=')[1]));
                string detailsHtml = File.ReadAllText(responseRoute);
                string fullDetailsHtml = GetFullDroneDetails(drone, detailsHtml, viewsDirectory);
                return fullDetailsHtml;
            }
            else if(urlRoute.Contains("/") || urlRoute.Contains("index"))
            {
                var drones = GetAllDrones();
                string indexHtml = File.ReadAllText(responseRoute);
                string fullIndexHtml = GetFullIndex(drones, indexHtml, viewsDirectory);
                return fullIndexHtml;
            }
            
            return "<h1>404 - Page Not Found</h1>";
        }

        public static string GetFullDroneDetails(Drone drone, string detailsHtml, string viewsDirectory)
        {
            detailsHtml = detailsHtml.Replace("{{droneId}}", drone.DroneId.ToString());
            detailsHtml = detailsHtml.Replace("{{imagePath}}", drone.ImagePath ?? "/Images/default_drone.jpg");
            detailsHtml = detailsHtml.Replace("{{droneName}}", drone.DroneName);
            detailsHtml = detailsHtml.Replace("{{manufacturer}}", drone.Manufacturer ?? "Unknown");
            detailsHtml = detailsHtml.Replace("{{flightTime}}", drone.FlightTime?.ToString() ?? "N/A");
            detailsHtml = detailsHtml.Replace("{{maxRange}}", drone.MaxRange?.ToString() ?? "N/A");
            detailsHtml = detailsHtml.Replace("{{camera}}", drone.CameraResolution ?? "N/A");
            detailsHtml = detailsHtml.Replace("{{price}}", drone.Price.ToString("0.00"));
            detailsHtml = detailsHtml.Replace("{{description}}", drone.Description ?? "No description available.");
            detailsHtml = detailsHtml.Replace("{{colorOptions}}", drone.ColorOptions ?? "N/A");
            detailsHtml = detailsHtml.Replace("{{shopsList}}", drone.AvailableShops ?? "N/A");
            detailsHtml = detailsHtml.Replace("{{maxSpeed}}", drone.MaxSpeed?.ToString() ?? "N/A");
            detailsHtml = detailsHtml.Replace("{{weight}}", drone.Weight?.ToString() ?? "N/A");
            return detailsHtml;
        }
         

        public static string GetFullIndex(List<Drone> drones, string indexHtml, string viewsDirectory)
        {
            string droneCardsHtml = "";

            foreach (var drone in drones)
            {
                string droneCard = File.ReadAllText(Path.Combine(viewsDirectory, "droneCard.html"));

                droneCard = droneCard.Replace("{{droneId}}", drone.DroneId.ToString());
                droneCard = droneCard.Replace("{{imagePath}}", drone.ImagePath ?? "/Images/default_drone.jpg");
                droneCard = droneCard.Replace("{{droneName}}", drone.DroneName);
                droneCard = droneCard.Replace("{{manufacturer}}", drone.Manufacturer ?? "Unknown");
                droneCard = droneCard.Replace("{{flightTime}}", drone.FlightTime?.ToString() ?? "N/A");
                droneCard = droneCard.Replace("{{maxRange}}", drone.MaxRange?.ToString() ?? "N/A");
                droneCard = droneCard.Replace("{{camera}}", drone.CameraResolution ?? "N/A");
                droneCard = droneCard.Replace("{{price}}", drone.Price.ToString("0.00"));

                droneCardsHtml += droneCard;
            }

            string fullIndexHtml = indexHtml.Replace("{{droneCards}}", droneCardsHtml);

            return fullIndexHtml;
        }

        public static List<Drone> GetAllDrones()
        {
            using (var db = new AppDbContext())
            {
                return db.Drones.ToList();
            }
        }

        public static Drone GetOneDrone(int droneId)
        {
            using (var db = new AppDbContext())
            {
                Drone drone = db.Drones.First(d => d.DroneId == droneId);
                return drone;
            }
        }

        public static byte[] GetImage(string responseRoute)
        {
            if (File.Exists(responseRoute))
            {
                return File.ReadAllBytes(responseRoute);
            }
            return Array.Empty<byte>();
        }
    }
}