using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "WavyWorleyNoiseLayer", menuName = "Layered Terrain/Layers/Wavy Worley Noise")]
    public class WavyWorleyNoiseLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int seedModifier;
        [SerializeField]
        private int cellWidth, cellHeight;
        [SerializeField]
        private float maxWaveAmplitudeDistance, minDistance, maxDistance, waveAmplitude, waveFrequency;
        [SerializeField]
        private bool inverse;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            return NoiseLayerComputes.WorleyNoiseWavy(x, y, width, height, seed + seedModifier, cellWidth, cellHeight, maxWaveAmplitudeDistance, minDistance, maxDistance, inverse, waveFrequency, waveAmplitude);
        }
    }
}
