using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RAPID.Server;

public class ClientSession
{
    public string Id { get; }
    public NetworkStream Stream { get; }
    public HashSet<string> SubscribedChannels { get; } = new(StringComparer.OrdinalIgnoreCase);

    public ClientSession(string id, NetworkStream stream)
    {
        Id = id;
        Stream = stream;
    }

    public void SendRawResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return;
        byte[] bytes = Encoding.UTF8.GetBytes(response);
        lock (Stream)
        {
            Stream.Write(bytes, 0, bytes.Length);
        }
    }
}
