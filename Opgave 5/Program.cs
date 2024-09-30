using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5001);
        server.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient socket = await server.AcceptTcpClientAsync();
            Task.Run(() => HandleClient(socket));
        }
    }

    private static async Task HandleClient(TcpClient socket)
    {
        NetworkStream ns = socket.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
        string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        var request = JsonSerializer.Deserialize<Request>(requestJson);
        var response = new Response();

        if (request != null && request.IsValid())
        {
            switch (request.Method.ToLower())
            {
                case "random":
                    Random random = new Random();
                    response.Result = new int[]
                    {
                        random.Next(request.Tal1, request.Tal2 + 1),
                        random.Next(request.Tal1, request.Tal2 + 1)
                    };
                    break;
                case "add":
                    response.Result = request.Tal1 + request.Tal2;
                    break;
                case "subtract":
                    response.Result = request.Tal1 - request.Tal2;
                    break;
                default:
                    response.Error = "Unknown method";
                    break;
            }
        }
        else
        {
            response.Error = "Invalid request format";
        }

        string responseJson = JsonSerializer.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
        await ns.WriteAsync(responseBytes, 0, responseBytes.Length);
        socket.Close();
    }
}

public class Request
{
    public string Method { get; set; }
    public int Tal1 { get; set; }
    public int Tal2 { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Method) && Tal1 >= 0 && Tal2 >= 0;
    }
}

public class Response
{
    public object Result { get; set; }
    public string Error { get; set; }
}
