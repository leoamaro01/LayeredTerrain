using System;
using UnityEngine;

namespace LayeredTerrainUnity.Modifiers
{
    [CreateAssetMenu(fileName = "PowerModifier", menuName = "Layered Terrain/Layer Modifiers/Power Modifier")]
    public class PowerModifier : LayeredTerrainLayerModifier
    {
        [SerializeField]
        private float power;

        public override float[,] ModifyLayer(float[,] layerResult)
        {
            var result = new float[layerResult.GetLength(0), layerResult.GetLength(1)];
            for (var i = 0; i < layerResult.GetLength(0); i++)
                for (var e = 0; e < layerResult.GetLength(1); e++)
                    result[i, e] = MathF.Pow(layerResult[i, e], power);

            return result;
        }
    }
}
