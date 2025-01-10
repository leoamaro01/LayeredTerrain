using UnityEngine;

namespace LayeredTerrainUnity
{
    public abstract class LayeredTerrainLayerModifier : ScriptableObject
    {
        public abstract float[,] ModifyLayer(float[,] layerResult);
    }
}
