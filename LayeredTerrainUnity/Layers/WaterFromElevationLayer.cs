using LayeredTerrain;
using System;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "WaterFromElevationLayer", menuName = "Layered Terrain/Layers/Water From Elevation")]
    public class WaterFromElevationLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private float waterLevel;

        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            if (dependencyFeatures == null || dependencyFeatures.Length == 0)
                throw new InvalidOperationException("The WaterFromElevation Layer depends on the elevation feature of the terrain, provide that feature as an argument");

            var elevationFeature = dependencyFeatures[0];

            var result = elevationFeature[x, y];

            for (int i = 0; i < width; i++)
                for (int e = 0; e < height; e++)
                {
                    result[i, e] = result[i, e] <= waterLevel ? 1f : 0f;
                }

            return result;
        }
    }
}
