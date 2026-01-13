using System.Numerics;

namespace Arc.Ecs;

public readonly struct ComponentMask
{
    private readonly ulong[] _words = []; // Sized to fix max components

    public ComponentMask(int wordCount) { _words = new ulong[wordCount]; }

    public static ComponentMask FromMax(int max) => new(max / sizeof(ulong) + 1);

    public int NumSetBits
    {
        get
        {
            if (_words is null)
                return 0;
            
            var count = 0;
            foreach (var word in _words)
                count += BitOperations.PopCount(word);
            return count;
        }
    }

    public bool IsEmpty => _words is null || _words.All(w => w == 0);

    public void SetBit(int index)
    {
        int word = index >> 6; // / 64 
        int bit = index & 63;  // % 64
        _words[word] |= 1UL << bit;
    }

    public void ClearBit(int index)
    {
        int word = index >> 6; // / 64 
        int bit = index & 63;  // % 64
        _words[word] &= ~(1UL << bit);
    }

    public bool HasBit(int index)
    {
        int word = index >> 6; // / 64 
        int bit = index & 63;  // % 64
        return (_words[word] & (1UL << bit)) != 0;
    }

    public bool ContainsAll(in ComponentMask other)
    {
        if (_words is null)
            return false;
        
        for (var i = 0; i < _words.Length; ++i)
        {
            if ((_words[i] & other._words[i]) != other._words[i])
                return false;
        }
        return true;
    }

    public bool ContainsAny(in ComponentMask other)
    {
        if (_words is null)
            return false;
        
        for (var i = 0; i < _words.Length; ++i)
        {
            if ((_words[i] & other._words[i]) != 0)
                return true;
        }
        return false;
    }

    public bool ContainsNone(in ComponentMask other)
    {
        if (_words is null)
            return true;
        
        for (var i = 0; i < _words.Length; ++i)
        {
            if ((_words[i] & other._words[i]) != 0)
                return false;
        }
        return true;
    }
}