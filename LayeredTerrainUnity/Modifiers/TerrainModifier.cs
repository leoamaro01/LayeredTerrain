using UnityEngine;

namespace LayeredTerrainUnity.Modifiers
{
    [CreateAssetMenu(fileName = "TerrainModifier", menuName = "Layered Terrain/Layer Modifiers/Terrain Modifier")]
    public class TerrainModifier : LayeredTerrainLayerModifier
    {
        private static float TerrainEase(float x)
        {
            return (1 - 0.05f) * x * x + 0.05f;
            //dark magic home-made ease function
            //return x < 0.763242f ? 1.5f * MathF.Pow(x, 3) - 0.05f * x * x + 0.05f : 1 - MathF.Pow(-2.36f * x + 2.36f, 2);
        }

        public override float[,] ModifyLayer(float[,] layerResult)
        {
            var result = new float[layerResult.GetLength(0), layerResult.GetLength(1)];

            for (var i = 0; i < layerResult.GetLength(0); i++)
                for (var e = 0; e < layerResult.GetLength(1); e++)
                    result[i, e] = TerrainEase(layerResult[i, e]);

            return result;
        }
    }
}
