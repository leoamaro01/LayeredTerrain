using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "NormalWorleyNoiseLayer", menuName = "Layered Terrain/Layers/Normal Worley Noise")]
    public class NormalWorleyNoiseLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int seedModifier;
        [SerializeField]
        private int cellWidth, cellHeight;
        [SerializeField]
        private float minDistance, maxDistance;
        [SerializeField]
        private bool inverse;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            return NoiseLayerComputes.WorleyNoiseNormal(x, y, width, height, seed + seedModifier, cellWidth, cellHeight, minDistance, maxDistance, inverse);
        }
    }
}
