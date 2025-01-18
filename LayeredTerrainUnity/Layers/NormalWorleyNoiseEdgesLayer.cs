using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "NormalWorleyNoiseEdgesLayer", menuName = "Layered Terrain/Layers/Normal Worley Noise Edges")]
    public class NormalWorleyNoiseEdgesLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int seedModifier;
        [SerializeField]
        private int cellWidth, cellHeight;
        [SerializeField]
        private float minDistance, minDifThreshold, maxDifThreshold;
        [SerializeField]
        private bool inverse;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            return NoiseLayerComputes.WorleyNoiseNormalEdges(x, y, width, height, seed + seedModifier, cellWidth, cellHeight, minDistance, minDifThreshold, maxDifThreshold, inverse);
        }
    }
}
