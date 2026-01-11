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

    public static ulong ComputeHash(string input)
    {
        ulong hash = FNVOffsetBasis;
        byte[] data = Encoding.UTF8.GetBytes(input);
        foreach (byte b in data)
        {
            hash ^= b;        // XOR the byte
            hash *= FNVPrime; // Multiply by the prime
        }

        return hash;
    }
}