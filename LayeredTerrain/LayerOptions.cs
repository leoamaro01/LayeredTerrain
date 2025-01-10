namespace LayeredTerrain
{
    public class LayerOptions
    {
        public LayerOptions(in Layer.ChunkCompute layerCompute, in string layerName, in string[] featureDependencies,
                            in string[] layerDependencies)
        {
            LayerCompute = layerCompute;
            LayerName = layerName;
            LayerDependencies = layerDependencies;
            FeatureDependencies = featureDependencies;
        }

        public Layer.ChunkCompute LayerCompute { get; }
        public string LayerName { get; }

        public string[] LayerDependencies { get; }
        public string[] FeatureDependencies { get; }
    }
}
