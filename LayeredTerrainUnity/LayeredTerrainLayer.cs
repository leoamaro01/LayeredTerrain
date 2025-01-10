using LayeredTerrain;
using UnityEngine;

namespace LayeredTerrainUnity
{
    public abstract class LayeredTerrainLayer : ScriptableObject
    {
        public string layerName;
        public LayeredTerrainLayer[] _dependencyLayerAssets;
        public LayeredTerrainFeature[] _dependencyFeatureAssets;

        public abstract float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers);
    }
}
