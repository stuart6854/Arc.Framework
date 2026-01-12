using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Arc.Core;

/// <summary>
/// FNV-1a hash algorithm
/// FNV-1a is a non-cryptographic hash function.
/// FNV-1a is designed to be fast while maintaining a low collision rate.
/// FNV-1a has slightly better avalanche characteristics than FNV-1.
/// </summary>
/// <see cref="https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function"/>
public static class FNV1A
{
    private const ulong FNVOffsetBasis = 0xcbf29ce484222325;
    private const ulong FNVPrime = 0x100000001b3;

    public static ulong ComputeHash<T>(params List<T> values)
        where T : unmanaged
    {
        return ComputeHash(CollectionsMarshal.AsSpan(values));
    }

    public static ulong ComputeHash<T>(params Span<T> values)
        where T : unmanaged
    {
        ulong hash = FNVOffsetBasis;
        var data = MemoryMarshal.AsBytes(values);
        foreach (byte b in data)
        {
            hash ^= b;        // XOR the byte
            hash *= FNVPrime; // Multiply by the prime
        }

        return hash;
    }

    public static ulong ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return FNVOffsetBasis;

        var buffer = input.Length <= 128
            ? stackalloc byte[Encoding.UTF8.GetMaxByteCount(input.Length)]
            : new byte[Encoding.UTF8.GetByteCount(input)];

        var written = Encoding.UTF8.GetBytes(input, buffer);
        return ComputeHash(buffer[..written]);
    }
}