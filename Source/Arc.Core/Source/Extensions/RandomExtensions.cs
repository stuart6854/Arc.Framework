namespace Arc.Core.Extensions;

public static class RandomExtensions
{
    extension(Random random)
    {
        public float NextNormalizedSingle() => random.NextSingle() * 2 - 1;
        public double NextNormalizedDouble() => random.NextDouble() * 2 - 1;
    }
}