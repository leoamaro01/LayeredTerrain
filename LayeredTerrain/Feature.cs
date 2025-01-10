using LayeredTerrain.DependencyResolve;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LayeredTerrain
{
    public class Feature : ChunkCached
    {
        public delegate float[,] LayerBlendFunction(float[,] accumulated, float[,] nextLayer);
        public delegate float[,] LayerModifierFunction(float[,] computedLayer);

        public Layer[] Layers { get; }

        private readonly LayerBlendFunction[] layerBlends;
        private readonly LayerModifierFunction[] layerModifiers;
        private readonly Dictionary<string, Layer> namedLayers;

        public Feature(in FeatureOptions options, Dictionary<string, Feature> dependencyFeatures)
            : base(options.GenOptions.ChunkCachedOptions)
        {
            var layerOptions = options.LayerOptions;
            layerBlends = options.Blends;
            layerModifiers = options.LayerModifiers;


            if (layerOptions.Length > layerModifiers.Length)
                throw new ArgumentException("Too few layer modifier functions, there should be a modifier function for each layer", nameof(options));


            if (layerOptions.Length > layerBlends.Length + 1)
                throw new ArgumentException("Too few blends functions, there should be a blend function for each layer except the first one.", nameof(options));

            Layers = new Layer[layerOptions.Length];
            namedLayers = new Dictionary<string, Layer>();

            var nameIndexes = new Dictionary<string, int>();

            var resolver = new DependencyResolver();

            for (int i = 0; i < layerOptions.Length; i++)
            {
                resolver.AddItem(layerOptions[i].LayerName);
                nameIndexes.Add(layerOptions[i].LayerName, i);
            }

            for (int i = 0; i < layerOptions.Length; i++)
                resolver.AddDependencies(layerOptions[i].LayerName, layerOptions[i].LayerDependencies);

            var layerCreationOrder = resolver.Solve();

            foreach (var layer in layerCreationOrder)
            {
                var index = nameIndexes[layer];
                var opt = layerOptions[index];

                Layers[index] = new Layer(opt.LayerCompute,
                                          options.GenOptions,
                                          opt.FeatureDependencies.Select(f => dependencyFeatures[f]).ToArray(),
                                          opt.LayerDependencies.Select(l => namedLayers[l]).ToArray());

                if (namedLayers.ContainsKey(opt.LayerName))
                    throw new ArgumentException("There should be no repeated layer names.", nameof(options));

                namedLayers[opt.LayerName] = Layers[index];
            }
        }

        public Layer this[string layerName]
        {
            get
            {
                if (!namedLayers.TryGetValue(layerName, out Layer? value))
                    throw new InvalidOperationException("There is no layer with that name.");

                return value;
            }
        }

        public float[,] this[int x, int y]
        {
            get
            {
                if (TryGetCached(x, y, out var chunk))
                    return chunk!;

                if (!InCacheArea(x, y))
                    ExpandCache(x, y);

                var computedChunk = layerModifiers[0](Layers[0][x, y]);

                for (var i = 1; i < Layers.Length; i++)
                {
                    var lowerChunk = layerModifiers[i](Layers[i][x, y]);

                    computedChunk = layerBlends[i - 1](computedChunk, lowerChunk);
                }

                Cache(computedChunk, x, y);

                return computedChunk;
            }
        }
    }
}
