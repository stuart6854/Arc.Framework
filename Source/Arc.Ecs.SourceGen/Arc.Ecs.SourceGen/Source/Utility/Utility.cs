using System.Runtime.InteropServices;
using System.Text;

namespace Arc.Ecs.SourceGen;

public static class Utility
{
    private const ulong FNVOffsetBasis = 0xcbf29ce484222325;
    private const ulong FNVPrime = 0x100000001b3;

    public static ulong ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return FNVOffsetBasis;

        var bytes = Encoding.UTF8.GetBytes(input);

        ulong hash = FNVOffsetBasis;
        var data = MemoryMarshal.AsBytes(bytes);
        foreach (byte b in data)
        {
            hash ^= b;        // XOR the byte
            hash *= FNVPrime; // Multiply by the prime
        }

        return hash;
    }
    
    public static string Join(int count, Func<int, string> func, string separator) => string.Join(separator, Enumerable.Range(0, count).Select(func));
}