namespace LayeredTerrain
{
    public readonly struct FeatureOptions
    {
        public FeatureOptions(in string featureName,
                              in LayerOptions[] layers,
                              in Feature.LayerBlendFunction[] blends,
                              in Feature.LayerModifierFunction[] modifiers,
                              in ChunkGenerationOptions genOptions)
        {
            LayerOptions = layers;
            Blends = blends;
            GenOptions = genOptions;
            LayerModifiers = modifiers;
            FeatureName = featureName;
        }

        public string FeatureName { get; }
        public LayerOptions[] LayerOptions { get; }
        public Feature.LayerBlendFunction[] Blends { get; }
        public Feature.LayerModifierFunction[] LayerModifiers { get; }
        public ChunkGenerationOptions GenOptions { get; }
    }
}
