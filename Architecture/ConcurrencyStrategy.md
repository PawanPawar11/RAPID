# RAPID Concurrency & Locking Strategy

## Overview

RAPID is built to support high-throughput concurrent client connections without sacrificing data consistency or thread safety.

---

## Synchronization Architecture

### 1. Global Dictionary Level (`ConcurrentDictionary<string, RedisValue>`)
- **Choice:** `ConcurrentDictionary<string, RedisValue>`
- **Rationale:** Provides lock-free O(1) concurrent lookups (`TryGetValue`), insertions (`TryAdd`), and deletions (`TryRemove`).
- **Benefit:** Operations on different keys run in parallel across worker threads without acquiring a global database lock.

### 2. Lock-Free Atomic Numeric Operations (`INCR`, `DECR`, `INCRBY`, `DECRBY`)
- **Choice:** Compare-And-Swap (CAS) optimistic concurrency loop.
- **Rationale:** Uses `TryGetValue` and `TryUpdate(key, newRedisValue, oldRedisValue)`. If another thread modifies the key concurrently, `TryUpdate` returns `false` and the operation retries automatically.
- **Benefit:** 100% thread-safe atomic increments under high contention without blocking any thread.

### 3. Fine-Grained Per-Instance Collection Locking (`List`, `Hash`)
- **Choice:** Instance-level `lock(redisVal.ListData)` and `lock(redisVal.HashData)`.
- **Rationale:** Encapsulates collection mutations (`LinkedList<string>`, `Dictionary<string, string>`) behind a lock dedicated strictly to that specific key's data instance.
- **Benefit:** Modifying `mylist1` never blocks another thread operating on `mylist2` or `myhash1`.

---

## Deadlock Prevention Guarantees

1. **Small Lock Scope:** Locks are held only for the microsecond duration of in-memory collection updates (e.g. `AddFirst`, `RemoveLast`).
2. **Zero Nested Locks:** A thread never attempts to acquire a second lock while holding a first lock.
3. **No I/O Under Locks:** Socket communication, disk writes, and console logging are executed completely outside lock boundaries.

---

## Graceful Server Shutdown

1. **Listener Stopping:** Stop accepting new incoming client socket connections.
2. **Client Cleanup:** Track active client sockets in a `ConcurrentDictionary<TcpClient, byte>` and close/dispose them gracefully during shutdown.
3. **Worker Cancellation:** Use `CancellationTokenSource` to gracefully stop background timers (such as `ExpirationManager`).
4. **Final Persistence:** Trigger snapshot save (`dump.json`) before process exit to guarantee data integrity.
