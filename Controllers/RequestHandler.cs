using NMR_projekt.Controllers;
using System.Net.Sockets;
using System.Text;

namespace NMR_projekt.Controllers
{
    public class RequestHandler
    {
        public void HandleAllRequests(string method, string receivedRequest, Socket clientSocket, byte[] buffer)
        {
            string urlRoute, urlParams, responseRoute;
            urlRoute = receivedRequest.Trim().Split(" ")[1];

            if (urlRoute.Contains('?'))
            {
                urlParams = GetUrlParams(receivedRequest);
                urlRoute = GetUrlRoute(receivedRequest);
            }
            else
            {
                urlParams = "";
            }

            responseRoute = GetFullRoute(urlRoute);
            var contentType = GetContentType(responseRoute);

            if (method == "GET")
            {
                byte[] sentResponse;

                if (responseRoute.Contains("Images"))
                {
                    sentResponse = GetHandler.GetImage(responseRoute);
                }
                else
                {
                    string response = GetHandler.GetData(urlRoute, urlParams, responseRoute);
                    sentResponse = Encoding.UTF8.GetBytes(response);
                }
                string responseHeader = GetHeader(contentType, sentResponse);
                clientSocket.Send(Encoding.UTF8.GetBytes(responseHeader));
                clientSocket.Send(sentResponse);
            }

            if (method == "POST")
            {
                PostHandler.ProcessDroneAdding(receivedRequest, buffer, urlRoute);
                clientSocket.Send(Encoding.UTF8.GetBytes(GetRedirectHeader()));
                return;
            }
        }

        public string GetUrlRoute(string receivedRequest)
        {
            string wholeRoute = receivedRequest.Trim().Split(" ")[1];
            return wholeRoute.Split("?")[0];
        }

        public string GetUrlParams(string receivedRequest)
        {
            string wholeRoute = receivedRequest.Trim().Split(" ")[1];
            return wholeRoute.Split("?")[1];
        }

        public string GetFullRoute(string urlRoute)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            if (urlRoute.Contains("/Images"))
            {
                return Path.Combine(projectRoot, urlRoute.Trim('/'));
            }
            else if (urlRoute.Contains("addNewDrone") || urlRoute.Contains("addDrone") || urlRoute.Contains("index") || urlRoute == "/" || urlRoute.Contains("masterDetail"))
            {
                if(urlRoute == "/")
                {
                    urlRoute = "index";
                }
                string viewsDirectory = Path.Combine(projectRoot, "Views");
                string addedUrl = urlRoute;
                if (!addedUrl.EndsWith(".html"))
                {
                    addedUrl += ".html";
                }
                return Path.Combine(viewsDirectory, addedUrl.Trim('/'));
            }

            return Path.Combine(projectRoot, urlRoute.Trim('/'));
        }

        public static string GetContentType(string filePath)
        {
            if (filePath.EndsWith(".html"))
            {
                return "text/html";
            }
            else if (filePath.EndsWith(".css"))
            {
                return "text/css";
            }
            else if (filePath.EndsWith(".jpg") || filePath.EndsWith(".jpeg"))
            {
                return "image/jpeg";
            }
            else if (filePath.EndsWith(".png"))
            {
                return "image/png";
            }
            else if (filePath.EndsWith(".gif"))
            {
                return "image/gif";
            }
            else if (filePath.EndsWith(".svg"))
            {
                return "image/svg+xml";
            }
            else if (filePath.EndsWith(".ico"))
            {
                return "image/x-icon";
            }
            else if (filePath.EndsWith(".webp"))
            {
                return "image/webp";
            }
            else
            {
                return "application/octet-stream";
            }
        }

        public static string GetHeader(string contentType, byte[] sentResponse)
        {
            string responseHeader;
            if (sentResponse.Length > 0)
            {
                responseHeader = "HTTP/1.1 200 OK\r\n" +
                                 $"Content-Type: {contentType}\r\n" +
                                 $"Content-Length: {sentResponse.Length}\r\n" +
                                 "Connection: close\r\n\r\n";
            }
            else
            {
                contentType = "text/html";
                sentResponse = Encoding.UTF8.GetBytes("<h1>404 NOT FOUND</h1>");
                responseHeader = "HTTP/1.1 404 Not Found\r\n" +
                                 $"Content-Type: {contentType}\r\n" +
                                 $"Content-Length: {sentResponse.Length}\r\n" +
                                 "Connection: close\r\n\r\n";
            }
            return responseHeader;
        }

        public static string GetRedirectHeader()
        {
            string response =
                "HTTP/1.1 302 Found\r\n" +
                "Location: /\r\n" +
                "Content-Length: 0\r\n" +
                "Content-Type: text/html\r\n" +
                "Connection: close\r\n\r\n";
            return response;
        }
    }
}