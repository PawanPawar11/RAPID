using RAPID.PubSub;
using RAPID.Server;
using RAPID.Storage;

namespace RAPID.Commands;

public class CommandContext
{
    public Database Db { get; }
    public PubSubManager PubSub { get; }
    public ClientSession Session { get; }

    public CommandContext(Database db, PubSubManager pubSub, ClientSession session)
    {
        Db = db;
        PubSub = pubSub;
        Session = session;
    }
}
