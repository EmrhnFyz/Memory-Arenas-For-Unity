# Memory Arenas for Unity

## Project Overview
This project demonstrates a minimal, high-performance memory arena (arena allocator) for Unity, 
designed to efficiently allocate and reset large numbers of temporary objects without triggering the garbage collector (GC). 
The system is showcased with a recursive, tree-shaped crafting simulation—an ideal use case for memory arenas due to its dynamic, graph-like data structures.

## Why Memory Arenas?
In Unity and other game engines, local variables are cheap because the CPU allocates them on the stack—a fast, temporary block of memory. 
However, when building recursive structures like trees or graphs, stack allocation is insufficient, and heap allocations become necessary. 
Every heap allocation in managed languages like C# can trigger the garbage collector, leading to performance spikes and memory fragmentation.
Game engines often solve this with memory arenas: fixed-size allocators that avoid fragmentation and GC entirely. 
Memory arenas allow you to allocate hundreds of temporary objects in a contiguous block of memory and reset everything with a single call, making them ideal for temporary, short-lived data.

## What This Project Does
- Implements a minimal, raw memory arena system using Unity's low-level unsafe utilities.
- Allocates hundreds of temporary objects (e.g., nodes in a recursive crafting tree) without any GC allocations.
- Resets all allocations instantly with a single call, making memory reuse trivial and efficient.
- Showcases the system with a recursive crafting simulation, modeling a graph/tree problem common in games.

## How It Works
### ArenaAllocator
The core of this project is the `ArenaAllocator`:
```csharp
public unsafe class ArenaAllocator : IDisposable
{
    private byte* _buffer;
    private int _offset;
    private readonly int _capacity;

    public ArenaAllocator(int sizeInBytes)
    {
        _buffer = (byte*)UnsafeUtility.Malloc(sizeInBytes, 4, Allocator.Persistent);
        _capacity = sizeInBytes;
        _offset = 0;
    }

    public T* Alloc<T>(int count = 1) where T : unmanaged
    {
        var size = UnsafeUtility.SizeOf<T>() * count;
        if (_offset + size > _capacity)
            throw new InvalidOperationException("ArenaAllocator out of memory");

        var ptr = (T*)(_buffer + _offset);
        _offset += size;
        return ptr;
    }

    public void Reset() => _offset = 0;

    public void Dispose()
    {
        if (_buffer != null)
        {
            UnsafeUtility.Free(_buffer, Allocator.Persistent);
            _buffer = null;
        }
    }
}
```
- Allocates a fixed-size block of memory up front.
- Provides `Alloc<T>(count)` to reserve space for one or more unmanaged objects.
- All allocations are linear and contiguous—no fragmentation, no per-object GC overhead.
- `Reset()` instantly makes all memory available for reuse.
- `Dispose()` frees the memory when done.

### Demo: Crafting Simulation
```csharp
// Allocate an arena for 10 CraftNode objects
var allocator = new ArenaAllocator(sizeof(CraftNode) * 10);

// Simulate crafting an Iron Sword (builds a tree of CraftNodes in the arena)
var root = craftSimulator.SimulateCraft(allocator, ItemType.IronSword, 1);

// Use the tree...

// Instantly reset all allocations for reuse
allocator.Reset();
```
- Models a recursive crafting system (e.g., crafting an Iron Sword from Iron Ingots, which are crafted from Iron Ore).
- Each node in the crafting tree is allocated from the arena, not the heap.
- The entire tree can be discarded and memory reused with a single `Reset()` call.
- Demonstrates how memory arenas excel at recursive, temporary data structures.

## When to Use Memory Arenas
- Temporary, short-lived objects (e.g., per-frame or per-operation data).
- Recursive or graph-like data structures.
- Situations where GC spikes or memory fragmentation are unacceptable.
