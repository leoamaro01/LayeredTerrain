namespace LayeredTerrainUnity.Modifiers
{
    public class NoOpModifier : LayeredTerrainLayerModifier
    {
        public override float[,] ModifyLayer(float[,] layerResult)
        {
            return layerResult;
        }
    }
}
