using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener listener = new TcpListener(IPAddress.Any, 6379);

ConcurrentDictionary<string, string> store = new ConcurrentDictionary<string, string>();

listener.Start();

Console.WriteLine("Redis server started on port 6379...");

while (true)
{
    // Wait for a new client
    TcpClient client = listener.AcceptTcpClient();

    Console.WriteLine("New client connected.");

    // Handle the client on a separate thread
    Task task = Task.Run(() =>
    {
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];

        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Console.WriteLine($"Received: {message}");

        store["lastMessage"] = message;

        string response = "OK";

        byte[] responseBytes = Encoding.UTF8.GetBytes(response);

        stream.Write(responseBytes, 0, responseBytes.Length);

        stream.Close();
        client.Close();

        Console.WriteLine(store["lastMessage"]);
        Console.WriteLine("Client disconnected.");
    });
}
