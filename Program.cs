using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener listener = new TcpListener(IPAddress.Any, 6379);

listener.Start();

TcpClient client = listener.AcceptTcpClient();

NetworkStream stream = client.GetStream();

byte[] buffer = new byte[1024];

int bytesRead = stream.Read(buffer, 0, buffer.Length);

string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

Console.WriteLine(message);

string response = "OK";

byte[] responseBytes = Encoding.UTF8.GetBytes(response);

stream.Write(responseBytes);

stream.Close();
client.Close();
listener.Stop();