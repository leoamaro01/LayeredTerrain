namespace LayeredTerrain
{
    public class ChunkCached
    {
        private readonly int cacheExpandMargin;

        private int cacheMaxX;
        private int cacheMaxY;
        private int cacheMinX;
        private int cacheMinY;
        private float[,]?[,] chunkCache;

        internal ChunkCached(in ChunkCachedOptions options)
        {
            this.cacheExpandMargin = options.CacheExpandMargin;
            var initialWidth = options.InitialWidth;
            var initialHeight = options.InitialHeight;

            chunkCache = new float[initialWidth, initialHeight][,];
            cacheMinX = -initialWidth / 2;
            cacheMinY = -initialHeight / 2;
            cacheMaxX = cacheMinX + initialWidth;
            cacheMaxY = cacheMinY + initialHeight;
        }

        protected void ExpandCache(int accomodateX, int accomodateY)
        {
            int newMinX = cacheMinX,
                newMinY = cacheMinY,
                newMaxX = cacheMaxX,
                newMaxY = cacheMaxY;

            if (accomodateX < cacheMinX)
                newMinX = accomodateX - cacheExpandMargin;
            else if (accomodateX >= cacheMaxX)
                newMaxX = accomodateX + cacheExpandMargin;

            if (accomodateY < cacheMinY)
                newMinY = accomodateY - cacheExpandMargin;
            else if (accomodateY >= cacheMaxY)
                newMaxY = accomodateY + cacheExpandMargin;

            var newWidth = newMaxX - newMinX;
            var newHeight = newMaxY - newMinY;

            float[,]?[,] newCache = new float[newWidth, newHeight][,];

            var xOffset = cacheMinX - newMinX;
            var yOffset = cacheMinY - newMinY;

            for (var x = xOffset; x < cacheMaxX - newMinX; x++)
                for (var y = yOffset; y < cacheMaxY - newMinY; y++)
                {
                    var oldX = x - xOffset;
                    var oldY = y - yOffset;

                    newCache[x, y] = chunkCache[oldX, oldY];
                }

            cacheMinX = newMinX;
            cacheMinY = newMinY;
            cacheMaxX = newMaxX;
            cacheMaxY = newMaxY;
            chunkCache = newCache;
        }

        protected bool InCacheArea(int x, int y)
        {
            return x >= cacheMinX && y >= cacheMinY && x < cacheMaxX && y < cacheMaxY;
        }

        protected bool TryGetCached(int x, int y, out float[,]? chunk)
        {
            chunk = null;
            if (!InCacheArea(x, y))
                return false;

            var cacheX = x - cacheMinX;
            var cacheY = y - cacheMinY;

            var cached = chunkCache[cacheX, cacheY];

            if (cached == null)
                return false;

            var width = cached.GetLength(0);
            var height = cached.GetLength(1);

            chunk = new float[width, height];

            for (var i = 0; i < width; i++)
                for (var e = 0; e < height; e++)
                {
                    chunk[i, e] = cached[i, e];
                }

            return chunk != null;
        }

        protected void Cache(float[,] item, int x, int y)
        {
            var cacheX = x - cacheMinX;
            var cacheY = y - cacheMinY;

            var width = item.GetLength(0);
            var height = item.GetLength(1);
            var cached = new float[width, height];

            for (var i = 0; i < width; i++)
                for (var e = 0; e < height; e++)
                {
                    cached[i, e] = item[i, e];
                }

            chunkCache[cacheX, cacheY] = cached;
        }
    }
}