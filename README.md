# RAPID - High-Performance C# Redis Clone

**RAPID** is a lightweight, high-performance, multithreaded Redis clone built from scratch in C# (.NET 10). It implements the Redis Serialization Protocol (RESP), supports multiple data types (Strings, Lists, Hashes), features hybrid key expiration (TTL), atomic lock-free numeric operations, JSON snapshot persistence, real-time Pub/Sub message broadcasting, and graceful server shutdown.

---

## 🚀 Features & Milestones

- **Multithreaded TCP Server:** Listens on port `6379` by default, handling concurrent client connections asynchronously via task worker pools.
- **RESP Protocol Support:** Formats replies according to standard Redis Serialization Protocol specs (`+OK\r\n`, `$len\r\nval\r\n`, `:integer\r\n`, `*array\r\n`, `-ERR ...\r\n`).
- **Rich Data Structures:**
  - **Strings & Numerics:** `SET`, `GET`, `INCR`, `DECR`, `INCRBY`, `DECRBY`.
  - **Lists:** `LPUSH`, `RPUSH`, `LPOP`, `RPOP`, `LLEN`.
  - **Hashes:** `HSET`, `HGET`, `HDEL`, `HEXISTS`, `HGETALL`.
- **Hybrid Expiration System (TTL):**
  - **Passive / Lazy:** Cleans up expired keys on-demand during access (`GET`, `EXISTS`, `TTL`).
  - **Active / Background:** Periodic timer task sweeps expired keys every 1 second without blocking clients.
- **Data Persistence (RDB-style):**
  - **`SAVE`**: Synchronously saves snapshot of database state to `dump.json`.
  - **`BGSAVE`**: Asynchronously saves snapshot in a background worker thread.
  - **Atomic File Writing:** Writes to `dump.json.tmp` before moving to `dump.json` to prevent file corruption.
  - **Auto-Restoration:** Restores data state on server startup while skipping expired keys.
- **Pub/Sub Messaging:**
  - `SUBSCRIBE`, `UNSUBSCRIBE`, `PUBLISH` commands.
  - Real-time message broadcasting to active subscribers.
  - Automatic channel cleanup and subscription removal on client disconnect.
- **Thread Safety & High Concurrency:**
  - Lock-free optimistic Compare-And-Swap (CAS) loops for numeric operations.
  - Fine-grained per-instance locking for List and Hash collections.
- **Graceful Shutdown:**
  - Catches `Ctrl+C` / process exit signals (`SIGINT`/`SIGTERM`), stops new listener connections, closes client sockets cleanly, and saves a final snapshot.

---

## 📁 Project Architecture

The codebase is organized into decoupled layers:

```text
RAPID/
├── Architecture/
│   └── ConcurrencyStrategy.md          # Detailed Locking & Concurrency Documentation
├── Commands/
│   ├── ICommand.cs                      # Unified Command Interface
│   ├── CommandContext.cs                # Context DTO passed to commands (Db, PubSub, Session)
│   ├── CommandDispatcher.cs             # Command Registry & Router
│   ├── StringCommands/                  # GET, SET, INCR, DECR, INCRBY, DECRBY
│   ├── ListCommands/                    # LPUSH, RPUSH, LPOP, RPOP, LLEN
│   ├── HashCommands/                    # HSET, HGET, HDEL, HEXISTS, HGETALL
│   ├── KeyCommands/                     # DEL, EXISTS, EXPIRE, TTL
│   ├── PubSubCommands/                  # SUBSCRIBE, UNSUBSCRIBE, PUBLISH
│   └── ServerCommands/                  # PING, SAVE, BGSAVE
├── Persistence/
│   ├── KeySnapshotDto.cs                # Serializable DTOs for snapshotting
│   └── PersistenceManager.cs            # Atomic JSON I/O & BGSAVE state manager
├── PubSub/
│   └── PubSubManager.cs                 # Real-time channel & subscriber registry
├── Server/
│   ├── TcpServer.cs                     # TCP Listener & active client socket lifecycle
│   ├── ClientHandler.cs                 # Socket read/write processing per connection
│   └── ClientSession.cs                 # Client connection context & Pub/Sub stream handle
├── Storage/
│   ├── Database.cs                      # Core ConcurrentDictionary storage engine
│   ├── Database.String.cs               # String & Get operation implementation
│   ├── Database.Numeric.cs              # Lock-free CAS IncrBy implementation
│   ├── Database.List.cs                 # Atomic List operation implementation
│   ├── Database.Hash.cs                 # Atomic Hash operation implementation
│   ├── Database.Expiration.cs           # EXPIRE, TTL, and cleanup logic
│   ├── Database.Persistence.cs          # Snapshot creation & reloading
│   ├── ExpirationManager.cs             # Background periodic key cleaner
│   └── Models/                          # Data Models & Result records
└── Program.cs                           # Clean bootstrap entry point
```

---

## 🛠️ Supported Commands Reference

### String & Numeric Commands
| Command | Syntax | Description | Example Response |
| :--- | :--- | :--- | :--- |
| `SET` | `SET key value` | Set key to hold string value | `+OK\r\n` |
| `GET` | `GET key` | Get string value of key | `$5\r\nHello\r\n` or `$-1\r\n` |
| `INCR` | `INCR key` | Increment key's integer by 1 | `:1\r\n` |
| `DECR` | `DECR key` | Decrement key's integer by 1 | `:0\r\n` |
| `INCRBY` | `INCRBY key increment` | Increment key's integer by value | `:10\r\n` |
| `DECRBY` | `DECRBY key decrement` | Decrement key's integer by value | `:5\r\n` |

### Key & Expiration Commands
| Command | Syntax | Description | Example Response |
| :--- | :--- | :--- | :--- |
| `DEL` | `DEL key [key2 ...]` | Delete one or more keys | `:1\r\n` |
| `EXISTS` | `EXISTS key [key2 ...]` | Check if key(s) exist | `:1\r\n` |
| `EXPIRE` | `EXPIRE key seconds` | Set TTL in seconds on a key | `:1\r\n` (1 if set, 0 if missing) |
| `TTL` | `TTL key` | Get remaining TTL in seconds | `:10\r\n` (`-1` no TTL, `-2` missing) |

### List Commands
| Command | Syntax | Description | Example Response |
| :--- | :--- | :--- | :--- |
| `LPUSH` | `LPUSH key val [val2 ...]` | Prepend value(s) to head of list | `:2\r\n` |
| `RPUSH` | `RPUSH key val [val2 ...]` | Append value(s) to tail of list | `:3\r\n` |
| `LPOP` | `LPOP key` | Remove & return first element | `$5\r\nvalue\r\n` |
| `RPOP` | `RPOP key` | Remove & return last element | `$5\r\nvalue\r\n` |
| `LLEN` | `LLEN key` | Get total length of list | `:2\r\n` |

### Hash Commands
| Command | Syntax | Description | Example Response |
| :--- | :--- | :--- | :--- |
| `HSET` | `HSET key field value` | Set hash field to value | `:1\r\n` (1 new, 0 update) |
| `HGET` | `HGET key field` | Get value of hash field | `$5\r\nvalue\r\n` |
| `HDEL` | `HDEL key field` | Delete field from hash | `:1\r\n` |
| `HEXISTS` | `HEXISTS key field` | Check if field exists in hash | `:1\r\n` |
| `HGETALL` | `HGETALL key` | Get all field-value pairs | `*4\r\n$1\r\nk\r\n$1\r\nv\r\n...` |

### Persistence & Utility Commands
| Command | Syntax | Description | Example Response |
| :--- | :--- | :--- | :--- |
| `PING` | `PING` | Test server connection | `+PONG\r\n` |
| `SAVE` | `SAVE` | Synchronously save snapshot to disk | `+OK\r\n` |
| `BGSAVE` | `BGSAVE` | Asynchronously save snapshot in background | `+Background saving started\r\n` |

### Pub/Sub Commands
| Command | Syntax | Description | Example Response |
| :--- | :--- | :--- | :--- |
| `SUBSCRIBE` | `SUBSCRIBE channel [chan2 ...]` | Subscribe client to channel(s) | `*3\r\n$9\r\nsubscribe\r\n...` |
| `UNSUBSCRIBE` | `UNSUBSCRIBE [channel ...]` | Unsubscribe from channel(s) | `*3\r\n$11\r\nunsubscribe\r\n...` |
| `PUBLISH` | `PUBLISH channel message` | Broadcast message to subscribers | `:2\r\n` (receivers count) |

---

## ⚡ Getting Started

### Prerequisites
- **.NET SDK 10.0** (or .NET 8.0+) installed on your machine.
- `redis-cli` or any TCP telnet/netcat client (e.g. `nc 127.0.0.1 6379`).

### Building and Running
```bash
# Clone the repository
git clone https://github.com/PawanPawar11/RAPID.git
cd RAPID

# Build the project
dotnet build

# Run the server
dotnet run
```

### Testing with `redis-cli` or `netcat`
Open a separate terminal window and connect to RAPID:
```bash
# Using netcat / telnet
nc 127.0.0.1 6379

# Try commands:
PING
+PONG

SET name Pawan
+OK

GET name
$5
Pawan

EXPIRE name 10
:1

TTL name
:9

LPUSH mylist item1 item2
:2

HSET user:1 name Alice
:1

HGETALL user:1
*2
$4
name
$5
Alice
```

---

## 🧠 Concurrency & Lock Design

For a detailed deep dive into RAPID's concurrency design, read [ConcurrencyStrategy.md](Architecture/ConcurrencyStrategy.md).

- **Global Store:** Uses `ConcurrentDictionary<string, RedisValue>` for lock-free lookup and mutations across keys.
- **Atomic Numerics:** Uses optimistic Compare-And-Swap (CAS) retry loops for 100% lock-free `INCR`/`DECR` operations.
- **Collections (`List`/`Hash`):** Uses instance-level locking (`lock(list)` / `lock(hash)`) ensuring operations on key `A` never block operations on key `B`.
- **Zero Deadlocks:** Zero nested locks, no I/O inside lock scopes.

---

## 📄 License
Distributed under the MIT License.
