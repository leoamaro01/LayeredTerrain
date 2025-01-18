using UnityEngine;

namespace LayeredTerrainUnity.Blends
{
    [CreateAssetMenu(fileName = "RiverCarveBlend", menuName = "Layered Terrain/Layer Blends/River Carve")]
    public class RiverCarveBlend : LayeredTerrainLayerBlend
    {
        [SerializeField]
        private float maxElevationForCarve, minElevationForMaxCarve, maxCarveStrength;

        public override float[,] BlendFunction(float[,] accumulation, float[,] nextLayer)
        {
            float[,] result = new float[accumulation.GetLength(0), accumulation.GetLength(1)];

            for (int i = 0; i < accumulation.GetLength(0); i++)
                for (int e = 0; e < accumulation.GetLength(1); e++)
                {
                    var elevation = accumulation[i, e];

                    float carveStrength;
                    if (elevation <= minElevationForMaxCarve)
                        carveStrength = maxCarveStrength;
                    else if (elevation >= maxElevationForCarve)
                        carveStrength = 0f;
                    else
                        carveStrength = maxCarveStrength * ((maxElevationForCarve - elevation) / (maxElevationForCarve - minElevationForMaxCarve));

                    result[i, e] = elevation - nextLayer[i, e] * carveStrength;
                }

            return result;
        }
    }
}
