using LayeredTerrain;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredTerrainUnity.Layers
{
    [CreateAssetMenu(fileName = "WaterProximityLayer", menuName = "Layered Terrain/Layers/Water Proximity")]
    public class WaterProximityLayer : LayeredTerrainLayer
    {
        [SerializeField]
        private int maxDistanceToWater;
        public override float[,] LayerCompute(int x, int y, int width, int height, int seed, Feature[] dependencyFeatures, Layer[] dependencyLayers)
        {
            if (dependencyFeatures == null || dependencyFeatures.Length == 0)
                throw new System.InvalidOperationException("The WaterProximity Layer depends on the water layer, provide that layer as an argument");

            int sourceGridMarginH = Mathf.CeilToInt((float)maxDistanceToWater / width);
            int sourceGridMarginV = Mathf.CeilToInt((float)maxDistanceToWater / height);

            var sourceGrid = new int[width * (sourceGridMarginH * 2 + 1), height * (sourceGridMarginV * 2 + 1)];

            var waterFeature = dependencyFeatures[0];

            var edge = new Queue<(int x, int y)>();

            for (var i = -sourceGridMarginH; i <= sourceGridMarginH; i++)
                for (var e = -sourceGridMarginV; e <= sourceGridMarginV; e++)
                {
                    var waterChunk = waterFeature[x + i, y + e];

                    for (var cellX = 0; cellX < width; cellX++)
                        for (var cellY = 0; cellY < height; cellY++)
                        {
                            var posX = i + sourceGridMarginH + cellX;
                            var posY = e + sourceGridMarginV + cellY;

                            var value = waterChunk[cellX, cellY] > 0.001f ? 0 : -1;

                            if (value == 0)
                                edge.Enqueue((posX, posY));

                            sourceGrid[posX, posY] = value;
                        }
                }

            while (edge.Count > 0)
            {
                var (cellX, cellY) = edge.Dequeue();

                var value = sourceGrid[cellX, cellY];

                for (var i = -1; i <= 1; i++)
                    for (var e = -1; e <= 1; e++)
                    {
                        if (cellX + i < 0 || cellX + i >= sourceGrid.GetLength(0) || cellY + e < 0 || cellY + e >= sourceGrid.GetLength(1))
                            continue;

                        if (sourceGrid[cellX + i, cellY + e] >= 0)
                            continue;

                        sourceGrid[cellX + i, cellY + e] = value + 1;
                        edge.Enqueue((cellX + i, cellY + e));
                    }
            }

            var result = new float[width, height];

            for (var i = 0; i < width; i++)
                for (var e = 0; e < height; e++)
                {
                    var value = sourceGrid[sourceGridMarginH * width + i, sourceGridMarginV * height + e];

                    result[i, e] = Mathf.Max(maxDistanceToWater - value, 0f) / maxDistanceToWater;
                }

            return result;
        }
    }
}
