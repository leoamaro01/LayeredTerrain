using UnityEngine;

namespace LayeredTerrainUnity.Modifiers
{
    [CreateAssetMenu(fileName = "ScaleModifier", menuName = "Layered Terrain/Layer Modifiers/Scale Modifier")]
    public class ScaleLayerModifier : LayeredTerrainLayerModifier
    {
        [SerializeField]
        private float scale;

        public override float[,] ModifyLayer(float[,] layerResult)
        {
            for (var i = 0; i < layerResult.GetLength(0); i++)
                for (var e = 0; e < layerResult.GetLength(1); e++)
                    layerResult[i, e] *= scale;

            return layerResult;
        }
    }
}
