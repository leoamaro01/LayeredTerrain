using LayeredTerrain;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "FeatureResultLayer", menuName = "Layered Terrain/Layers/Feature Result")]
    public class FeatureResultLayer : LayeredTerrainLayer
    {
        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            if (dependencyFeatures == null || dependencyFeatures.Length == 0)
            {
                return new float[width, height];
            }

            return dependencyFeatures[0][x, y];
        }
    }
}
