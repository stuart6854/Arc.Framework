using System.Numerics;

namespace Arc.Ecs;

public readonly struct ComponentMask() : IEquatable<ComponentMask>
{
    private readonly ulong[] _words = new ulong[WordCount];
    
    private static int WordCount => (ComponentRegistry.Components.Count + 63) >> 6;

    public bool IsEmpty
    {
        get
        {
            var w = _words;
            if (w is null || w.Length == 0)
                return true;

            foreach (var word in w)
                if (word != 0)
                    return false;
            return true;
        }
    }

    public int NumSetBits
    {
        get
        {
            var w = _words;
            if (w is null || w.Length == 0)
                return 0;

            var count = 0;
            foreach (var word in _words)
                count += BitOperations.PopCount(word);
            return count;
        }
    }

    public void SetBit(int index)
    {
        if ((uint)index > int.MaxValue) // also reject negatives
            throw new ArgumentOutOfRangeException(nameof(index));

        var wordIndex = index >> 6;
        var bit = index & 63;
        _words[wordIndex] |= 1ul << bit;
    }

    public void ClearBit(int index)
    {
        if ((uint)index > int.MaxValue) // also reject negatives
            throw new ArgumentOutOfRangeException(nameof(index));

        var wordIndex = index >> 6;
        var bit = index & 63;
        _words[wordIndex] &= ~(1ul << bit);
    }

    public bool HasBit(int index)
    {
        if ((uint)index > int.MaxValue) // also reject negatives
            return false;

        var wordIndex = index >> 6;
        var bit = index & 63;
        return (_words[wordIndex] & (1ul << bit)) != 0;
    }

    public int CountBitsUpto(int index)
    {
        var wordIndex = index >> 6;
        var bitOffset = index & 63;

        var count = 0;

        // 1) Full words before the target word
        var fullWords = Math.Min(wordIndex, _words.Length);
        for (var i = 0; i < fullWords; ++i)
            count += BitOperations.PopCount(_words[i]);

        // 2) Partial word (if in range)
        if (wordIndex < _words.Length)
        {
            var mask = (bitOffset == 63) ? ulong.MaxValue : (1ul << (bitOffset + 1)) - 1;
            count += BitOperations.PopCount(_words[wordIndex] & mask);
        }

        return count;
    }

    public bool ContainsAll(in ComponentMask other)
    {
        var a = _words;
        var b = other._words;

        if (b.Length == 0)
            return true; // Other is empty => always contained

        if (a.Length == 0)
            return other.IsEmpty;

        var min = Math.Min(a.Length, b.Length);

        // Common prefix
        for (var i = 0; i < min; ++i)
        {
            var bw = b[i];
            if ((a[i] & bw) != bw)
                return false;
        }

        // If other has extra words, they must all be zero to be contained
        for (var i = min; i < b.Length; ++i)
        {
            if (b[i] != 0)
                return false;
        }

        return true;
    }

    public bool ContainsAny(in ComponentMask other)
    {
        var a = _words;
        var b = other._words;

        if (a is null)
            return false;

        int min = Math.Min(a.Length, b.Length);
        for (var i = 0; i < min; ++i)
        {
            if ((a[i] & b[i]) != 0)
                return true;
        }

        return false;
    }

    public bool ContainsNone(in ComponentMask other) { return !ContainsAny(other); }

    public bool Equals(ComponentMask other) => _words.AsSpan().SequenceEqual(other._words);
    public override bool Equals(object? obj) => obj is ComponentMask other && Equals(other);

    public override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var word in _words)
            hc.Add(word);
        return hc.ToHashCode();
    }

    public static bool operator ==(ComponentMask left, ComponentMask right) => left.Equals(right);
    public static bool operator !=(ComponentMask left, ComponentMask right) => !left.Equals(right);
}