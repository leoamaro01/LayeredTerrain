using UnityEngine;

namespace LayeredTerrainUnity.Blends
{
    [CreateAssetMenu(fileName = "AdditionLayerBlend", menuName = "Layered Terrain/Layer Blends/Addition")]
    public class AdditionLayerBlend : LayeredTerrainLayerBlend
    {
        [SerializeField]
        private float upperWeight = 1f, nextWeight = 1f;

        public override float[,] BlendFunction(float[,] accumulation, float[,] nextLayer)
        {
            float[,] result = new float[accumulation.GetLength(0), accumulation.GetLength(1)];

            for (int i = 0; i < accumulation.GetLength(0); i++)
                for (int e = 0; e < accumulation.GetLength(1); e++)
                    result[i, e] = accumulation[i, e] * upperWeight + nextLayer[i, e] * nextWeight;

            return result;
        }
    }
}
