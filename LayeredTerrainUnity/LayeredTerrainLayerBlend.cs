using UnityEngine;

namespace LayeredTerrainUnity
{
    public abstract class LayeredTerrainLayerBlend : ScriptableObject
    {
        public abstract float[,] BlendFunction(float[,] accumulation, float[,] nextLayer);
    }
}
