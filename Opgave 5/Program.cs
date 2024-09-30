using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("JSON TCP Server:");

        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5001); // Ny port
        listener.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient socket = await listener.AcceptTcpClientAsync();
            IPEndPoint? clientEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
            if (clientEndPoint != null)
            {
                Console.WriteLine("Client connected: " + clientEndPoint.Address);
            }

            Task.Run(() => HandleClient(socket));
        }
    }

    static async Task HandleClient(TcpClient socket)
    {
        NetworkStream ns = socket.GetStream();
        StreamReader reader = new StreamReader(ns);
        StreamWriter writer = new StreamWriter(ns);

        writer.AutoFlush = true;

        while (socket.Connected)
        {
            string? message = await reader.ReadLineAsync();
            if (message != null)
            {
                Console.WriteLine("Received: " + message);
                string response = HandleMessage(message);
                await writer.WriteLineAsync(response);
            }
        }
    }

    static string HandleMessage(string message)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Request>(message);
            if (request == null || string.IsNullOrWhiteSpace(request.Method))
            {
                return JsonSerializer.Serialize(new Response { Status = "error", Message = "Invalid request format" });
            }

            switch (request.Method.ToLower())
            {
                case "random":
                    Random random = new Random();
                    int randomNumber1 = random.Next(request.Number1, request.Number2 + 1);
                    int randomNumber2 = random.Next(request.Number1, request.Number2 + 1);
                    return JsonSerializer.Serialize(new Response { Status = "success", Result = new[] { randomNumber1, randomNumber2 } });

                case "add":
                    int sum = request.Number1 + request.Number2;
                    return JsonSerializer.Serialize(new Response { Status = "success", Result = sum });

                case "subtract":
                    int difference = request.Number1 - request.Number2;
                    return JsonSerializer.Serialize(new Response { Status = "success", Result = difference });

                default:
                    return JsonSerializer.Serialize(new Response { Status = "error", Message = "Invalid method specified" });
            }
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new Response { Status = "error", Message = ex.Message });
        }
    }

    class Request
    {
        public string? Method { get; set; }
        public int Number1 { get; set; }
        public int Number2 { get; set; }
    }

    class Response
    {
        public string Status { get; set; }
        public object? Result { get; set; }
        public string? Message { get; set; }
    }
}

