using UnityEngine;

namespace LayeredTerrainUnity
{
    [CreateAssetMenu(fileName = "LayeredTerrainFeature", menuName = "Layered Terrain/Feature")]
    public class LayeredTerrainFeature : ScriptableObject
    {
        public string featureName;
        public LayeredTerrainLayer[] featureLayers;
        public LayeredTerrainLayerBlend[] layerBlends;
        public LayeredTerrainLayerModifier[] layerModifiers;
    }
}
