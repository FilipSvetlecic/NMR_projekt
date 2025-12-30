using NMR_projekt.Controllers;
using NMR_projekt.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;



namespace NMR_projekt
{
    public class Server
    {
        public static void Main()
        {
            var db = DbInitializer.GetContext();

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen();
            Console.WriteLine("Server is listening on localhost, on port 8080");
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }

        public static void HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[4096];
            string receivedRequest = "";
            int contentLength = 0;
            List<byte> allData = new List<byte>();
            bool headersReceived = false;
            int headerEndPosition = 0;
            try
            {
                int bytesRead;
                while((bytesRead = clientSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                {
                    for(int i = 0; i < bytesRead; i++)
                    {
                        allData.Add(buffer[i]);
                    }
                    if (!headersReceived)
                    {
                        string currentData = Encoding.UTF8.GetString(allData.ToArray());
                        int headerEnd = currentData.IndexOf("\r\n\r\n");

                        if (headerEnd != -1)
                        {
                            headersReceived = true;
                            receivedRequest = currentData.Substring(0, headerEnd + 4);

                            // Calculate where headers end in bytes
                            headerEndPosition = Encoding.UTF8.GetBytes(receivedRequest).Length;

                            // Extract Content-Length from headers
                            string[] lines = receivedRequest.Split('\n');
                            foreach (string line in lines)
                            {
                                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                                {
                                    string lengthStr = line.Substring(15).Trim();
                                    contentLength = int.Parse(lengthStr);
                                    Console.WriteLine($"Expecting {contentLength} bytes in body");
                                    break;
                                }
                            }
                        }
                    }

                    // Check if we have all the data
                    if (headersReceived)
                    {
                        int bodyReceived = allData.Count - headerEndPosition;

                        if (contentLength > 0)
                        {
                            Console.WriteLine($"Progress: {bodyReceived}/{contentLength} bytes received");
                        }

                        // If we have all the body data, stop reading
                        if (contentLength > 0 && bodyReceived >= contentLength)
                        {
                            Console.WriteLine("All data received!");
                            break;
                        }

                        // If no Content-Length (GET request), break after first packet
                        if (contentLength == 0)
                        {
                            break;
                        }
                    }

                    // Safety check: if we've received too much data, something is wrong
                    if (allData.Count > 50 * 1024 * 1024) // 50MB max
                    {
                        Console.WriteLine("ERROR: Data too large, aborting");
                        break;
                    }
                }

                // Convert all data to array
                byte[] completeData = allData.ToArray();
                receivedRequest = Encoding.UTF8.GetString(completeData, 0, Math.Min(4096, completeData.Length));

                // Parse method
                string method = receivedRequest.Trim().Split(" ")[0];
                string urlRoute = receivedRequest.Trim().Split(" ")[1];

                Console.WriteLine($"Method: {method} | Route: {urlRoute} | Total Size: {completeData.Length} bytes");

                // Pass to RequestHandler
                var handler = new RequestHandler();
                handler.HandleAllRequests(method, receivedRequest, clientSocket, completeData);

            }
            
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                clientSocket.Close();
            }

            Console.WriteLine(receivedRequest);
        }
    }
}