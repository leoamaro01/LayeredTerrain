namespace LayeredTerrain
{
    public class Layer : ChunkCached
    {
        public delegate float[,] ChunkCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers);

        private readonly ChunkCompute computeFunc;
        private readonly Layer[] dependencyLayers;
        private readonly Feature[] dependencyFeatures;
        private readonly ChunkGenerationOptions genOpts;

        internal Layer(in ChunkCompute computeFunc, in ChunkGenerationOptions options, in Feature[] dependencyFeatures, in Layer[] dependencyLayers) :
            base(options.ChunkCachedOptions)
        {
            this.computeFunc = computeFunc;
            this.dependencyLayers = dependencyLayers;
            this.dependencyFeatures = dependencyFeatures;
            genOpts = options;
        }

        public float[,] this[int x, int y]
        {
            get
            {
                if (TryGetCached(x, y, out var chunk))
                    return chunk!;

                if (!InCacheArea(x, y))
                    ExpandCache(x, y);

                var computedChunk = computeFunc(x, y, genOpts.ChunkWidth, genOpts.ChunkHeight, genOpts.Seed, dependencyFeatures, dependencyLayers);

                Cache(computedChunk, x, y);

                return computedChunk;
            }
        }
    }
}
