using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe class ArenaAllocator : IDisposable
{
	private byte* _buffer; // a pointer to a byte

	private int _offset; // represents how far into our block of memory we have allocated
	private readonly int _capacity;

	public void Reset() => _offset = 0; // Reset the offset to 0

	/// <summary>
	///     Initializes a new instance of the ArenaAllocator with a specified size in bytes.
	/// </summary>
	/// <param name="sizeInBytes"></param>
	public ArenaAllocator(int sizeInBytes)
	{
		_buffer = (byte*)UnsafeUtility.Malloc(sizeInBytes, 4, Allocator.Persistent); // Allocate memory with 4-byte alignment
		_capacity = sizeInBytes; // Store the capacity of the arena
		_offset = 0; // Initialize the offset to 0
	}

	/// <summary>
	///     Reserves space for one or more values of type T inside the memory arena's buffer.
	/// </summary>
	/// <param name="count"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public T* Alloc<T>(int count = 1) where T : unmanaged
	{
		var size = UnsafeUtility.SizeOf<T>() * count; // Calculate the size needed for T
		if (_offset + size > _capacity)
		{
			throw new InvalidOperationException("ArenaAllocator out of memory");
		}

		var ptr = (T*)(_buffer + _offset); // Get the pointer to the allocated memory
		_offset += size; // Update the offset

		return ptr; // Return the pointer to the allocated memory
	}


	public void Dispose()
	{
		if (_buffer != null)
		{
			UnsafeUtility.Free(_buffer, Allocator.Persistent); // Free the allocated memory
			_buffer = null; // Set the buffer to null to avoid dangling pointers
		}
	}
}