using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "PerlinNoiseLayer", menuName = "Layered Terrain/Layers/Perlin Noise")]
    public class PerlinNoiseLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int resolution;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            return NoiseLayerComputes.PerlinNoise(x, y, width, height, seed, resolution);
        }
    }
}
