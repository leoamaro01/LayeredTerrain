using LayeredTerrain;
using LayeredTerrain.Utilities;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "WhiteNoiseLayer", menuName = "Layered Terrain/Layers/White Noise")]
    public class WhiteNoiseLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int seedModifier;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            var result = new float[width, height];

            var random = new System.Random(Utils.GetCombinedSeed(x, y, seed + seedModifier));

            for (var i = 0; i < width; i++)
                for (var e = 0; e < height; e++)
                    result[i, e] = (float)random.NextDouble();

            return result;
        }
    }
}
