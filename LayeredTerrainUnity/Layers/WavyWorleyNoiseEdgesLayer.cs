using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "WavyWorleyNoiseEdgesLayer", menuName = "Layered Terrain/Layers/Wavy Worley Noise Edges")]
    public class WavyWorleyNoiseEdgesLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int seedModifier;
        [SerializeField]
        private int cellWidth, cellHeight;
        [SerializeField]
        private float maxWaveAmplitudeDistance, minDistance, minDifThreshold, maxDifThreshold, waveAmplitude, waveFrequency;
        [SerializeField]
        private bool inverse;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            return NoiseLayerComputes.WorleyNoiseWavyEdges(x, y, width, height, seed + seedModifier, cellWidth, cellHeight, maxWaveAmplitudeDistance, minDistance, minDifThreshold, maxDifThreshold, inverse, waveFrequency, waveAmplitude);
        }
    }
}
