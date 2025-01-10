namespace LayeredTerrain
{
    internal readonly struct ChunkCachedOptions
    {
        public ChunkCachedOptions(in int cacheExpandMargin, in int initialWidth, in int initialHeight)
        {
            CacheExpandMargin = cacheExpandMargin;
            InitialWidth = initialWidth;
            InitialHeight = initialHeight;
        }

        public int CacheExpandMargin { get; }
        public int InitialWidth { get; }
        public int InitialHeight { get; }
    }
}
