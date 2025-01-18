using UnityEngine;

namespace LayeredTerrainUnity.Modifiers
{
    [CreateAssetMenu(fileName = "NoneModifier", menuName = "Layered Terrain/Layer Modifiers/None")]
    public class NoOpModifier : LayeredTerrainLayerModifier
    {
        public override float[,] ModifyLayer(float[,] layerResult)
        {
            return layerResult;
        }
    }
}
