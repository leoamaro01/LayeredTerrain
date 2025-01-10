namespace LayeredTerrain
{
    public readonly struct ChunkGenerationOptions
    {

        public ChunkGenerationOptions(in int chunkWidth, in int chunkHeight, in int seed, in int initialWidth,
                                      in int initialHeight, in int cacheExpandMargin)
        {
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
            Seed = seed;
            InitialWidth = initialWidth;
            InitialHeight = initialHeight;
            CacheExpandMargin = cacheExpandMargin;
            ChunkCachedOptions = new ChunkCachedOptions(cacheExpandMargin, initialWidth, initialHeight);
        }

        public int ChunkWidth { get; }
        public int ChunkHeight { get; }
        public int Seed { get; }
        public int InitialWidth { get; }
        public int InitialHeight { get; }
        public int CacheExpandMargin { get; }

        internal ChunkCachedOptions ChunkCachedOptions { get; }
    }
}
