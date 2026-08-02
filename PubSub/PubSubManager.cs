using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RAPID.Server;

namespace RAPID.PubSub;

public class PubSubManager
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<ClientSession, byte>> _channels =
        new(StringComparer.OrdinalIgnoreCase);

    public int Subscribe(string channel, ClientSession client)
    {
        var subscribers = _channels.GetOrAdd(channel, _ => new ConcurrentDictionary<ClientSession, byte>());
        subscribers.TryAdd(client, 0);

        lock (client.SubscribedChannels)
        {
            client.SubscribedChannels.Add(channel);
            return client.SubscribedChannels.Count;
        }
    }

    public int Unsubscribe(string channel, ClientSession client)
    {
        if (_channels.TryGetValue(channel, out var subscribers))
        {
            subscribers.TryRemove(client, out _);
            if (subscribers.IsEmpty)
            {
                _channels.TryRemove(channel, out _);
            }
        }

        lock (client.SubscribedChannels)
        {
            client.SubscribedChannels.Remove(channel);
            return client.SubscribedChannels.Count;
        }
    }

    public void UnsubscribeAll(ClientSession client, Action<string, int>? onUnsubscribed = null)
    {
        List<string> channelsToUnsub;
        lock (client.SubscribedChannels)
        {
            channelsToUnsub = new List<string>(client.SubscribedChannels);
        }

        foreach (var channel in channelsToUnsub)
        {
            int remaining = Unsubscribe(channel, client);
            onUnsubscribed?.Invoke(channel, remaining);
        }
    }

    public int Publish(string channel, string message)
    {
        if (!_channels.TryGetValue(channel, out var subscribers))
        {
            return 0;
        }

        var clientList = subscribers.Keys;
        int deliveredCount = 0;

        string formattedMessage = $"*3\r\n$7\r\nmessage\r\n${Encoding.UTF8.GetByteCount(channel)}\r\n{channel}\r\n${Encoding.UTF8.GetByteCount(message)}\r\n{message}\r\n";

        foreach (var client in clientList)
        {
            try
            {
                client.SendRawResponse(formattedMessage);
                deliveredCount++;
            }
            catch
            {
                Unsubscribe(channel, client);
            }
        }

        return deliveredCount;
    }
}
