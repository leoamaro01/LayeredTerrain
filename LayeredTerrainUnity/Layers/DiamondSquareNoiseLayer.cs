using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "DiamondSquareNoiseLayer", menuName = "Layered Terrain/Layers/Diamond Square Noise")]
    public class DiamondSquareNoiseLayer : LayeredTerrainLayer
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float smoothness;
        [SerializeField]
        private float randomRange, minValue, maxValue;
        [SerializeField]
        private int seedModifier;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            return NoiseLayerComputes.DiamondSquareNoise(x, y, width, height, seed + seedModifier, smoothness, randomRange, minValue, maxValue);
        }
    }
}
