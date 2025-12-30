using DroneCatalog.Models;
using NMR_projekt.Models;
using System.Text;

namespace NMR_projekt.Controllers
{
    public class PostHandler
    {
        public static void ProcessDroneAdding(string receivedRequest, byte[] buffer, string urlRoute)
        {
            string boundary = GetBoundary(receivedRequest);
            if (string.IsNullOrWhiteSpace(boundary))
            {
                Console.WriteLine("Boundary not found in request.");
                return;
            }
            int indexOfFirstDataElement = receivedRequest.IndexOf(boundary);
            string formData = receivedRequest.Substring(indexOfFirstDataElement);
            string[] parts = formData.Split(boundary);

            var fields = new Dictionary<string, string?>();

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part) || !part.Contains("Content-Disposition"))
                {
                    continue;
                }
                int nameIndex = part.IndexOf("name=\"", StringComparison.OrdinalIgnoreCase);
                if (nameIndex == -1)
                {
                    continue;
                }

                int nameStart = nameIndex + 6;
                int nameEnd = part.IndexOf("\"", nameStart);
                string fieldName = part.Substring(nameStart, nameEnd - nameStart);

                if (part.Contains("filename="))
                {
                    int filenameIndex = part.IndexOf("filename=\"", StringComparison.OrdinalIgnoreCase);
                    int filenameStart = filenameIndex + 10;
                    int filenameEnd = part.IndexOf("\"", filenameStart);
                    string filename = part.Substring(filenameStart, filenameEnd - filenameStart);

                    if (string.IsNullOrWhiteSpace(filename))
                    {
                        fields[fieldName] = null;
                        continue;
                    }
                    byte[] imageBytes = ExtractImageBytes(buffer, boundary, filename);

                    string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                    projectRoot = projectRoot.Replace("bin\\Debug", "");

                    //NOVI KOD !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //DODAJ NOVU SLIKU KOJA ĆE BITI POSTAVLJENA SVAKI PUTA KADA SE SLIKA SKROZ NE PRENESE
                    //SLIKA DA PRIJENOS NIJE BIO USPJEŠAN
                    if (imageBytes == Array.Empty<byte>())
                    {
                        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        string blankImagePathDB = Path.Combine("/Images", "blank_picture.jpg");
                        fields[fieldName] = blankImagePathDB.Replace("\\", "/");
                        continue;
                    }
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(filename);
                    string imagePath = Path.Combine(projectRoot, "Images", imageName);
                    Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                    File.WriteAllBytes(imagePath, imageBytes);

                    string imagePathDB = Path.Combine("/Images", imageName);

                    fields[fieldName] = imagePathDB.Replace("\\", "/");
                    continue;
                }

                int valueStart = part.IndexOf("\r\n\r\n");
                if (valueStart == -1)
                {
                    fields[fieldName] = null;
                    continue;
                }

                string value = part.Substring(valueStart + 4).TrimEnd('\r', '\n');
                fields[fieldName] = string.IsNullOrWhiteSpace(value) ? null : value;
            }


            // Print results
            Console.WriteLine("\n=== EXTRACTED DATA ===");
            foreach (var pair in fields)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value ?? "[null]"}");
            }
            Console.WriteLine("======================\n");

            if (urlRoute.Contains("addNewDrone"))
            {
                SaveDroneToDB(fields);
            }
        }

        public static byte[] ExtractImageBytes(byte[] buffer, string boundary, string filename)
        {

            byte[] filenameBytes = Encoding.ASCII.GetBytes($"filename=\"{filename}\"");
            int headerStart = IndexOf(buffer, filenameBytes);
            if (headerStart == -1)
            {
                return Array.Empty<byte>();
            }

            // Nađi početak sadržaja datoteke (iza \r\n\r\n)
            int contentStart = IndexOf(buffer, Encoding.ASCII.GetBytes("\r\n\r\n"), headerStart);
            if (contentStart == -1)
            {
                return Array.Empty<byte>();
            }
            contentStart += 4; // preskoči \r\n\r\n

            // Nađi idući boundary nakon početka slike
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n" + boundary);
            int contentEnd = IndexOf(buffer, boundaryBytes, contentStart);
            if (contentEnd == -1)
            {
                contentEnd = buffer.Length;
            }
            //VAŽNO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //SLIKA MOŽE DOĆI U VIŠE PAKETA ZBOG SEGMENTACIJE - OVAJ PROGRAM TO NE PODRŽAVA, PROGRAM ČITA SAMO JEDAN PAKET
            //POTREBNO NAPRAVITI DIO PROGRAMA DA SE APLIKACIJE U BROWERSU NE SRUŠI KADA SLIKA DOĐE U VIŠE PAKETA
            //SLIKA DOLAZI U VIŠE PAKETA KADA NA KRAJU PAKETA KOJI JE U BUFERU NIJE BOUNDARY
            //U TOME SLUČAJU, KORISNIKA PREUSMJERI NA POČETNU STRANICU TE NEMOJ NIŠTA ZAPISIVATI U BAZU PODATAKA ILI 
            //NEMOJ ZAPISATI SAMO SLIKU
            if (contentEnd == buffer.Length)
            {
                return Array.Empty<byte>();
            }


            int length = contentEnd - contentStart;
            byte[] imageBytes = new byte[length];
            Array.Copy(buffer, contentStart, imageBytes, 0, length);
            return imageBytes;
        }

        public static void SaveDroneToDB(Dictionary<string, string?> fields)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var drone = new Drone
                    {
                        DroneName = fields["name"],
                        Price = decimal.Parse(fields["price"]),
                        Description = fields["description"],
                        ColorOptions = fields["colors"],
                        AvailableShops = fields["shops"],
                        ImagePath = fields["image"],
                        Manufacturer = fields["manufacturer"],
                        FlightTime = int.Parse(fields["flightTime"]),
                        MaxRange = int.Parse(fields["maxRange"]),
                        MaxSpeed = decimal.Parse(fields["maxSpeed"]),
                        Weight = decimal.Parse(fields["weight"]),
                        CameraResolution = fields["camera"],
                        UserId = 1
                    };
                    db.Drones.Add(drone);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR saving drone to DB: {ex.Message}");
            }
        }

        public static int IndexOf(byte[] buffer, byte[] pattern, int start = 0)
        {
            if (start >= buffer.Length || pattern.Length == 0)
                return -1;

            for (int i = start; i <= buffer.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string GetBoundary(string receivedRequest)
        {
            string[] strings = receivedRequest.Split(';');
            foreach (string s in strings)
            {
                string trimmedS = s.Trim();
                if (trimmedS.Contains("boundary"))
                {
                    string boundary = trimmedS.Split('=')[1];
                    boundary = "--" + boundary;
                    return boundary.Split("\r\n")[0].Trim();
                }
            }
            return "";
        }
    }
}