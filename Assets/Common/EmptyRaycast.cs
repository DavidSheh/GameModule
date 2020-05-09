namespace UnityEngine.UI
{
    public class EmptyRaycast : MaskableGraphic
    {
        protected EmptyRaycast()
        {
            useLegacyMeshGeneration = false;
        }
        
        public void EnableRaycast(bool enable)
        {
            this.raycastTarget = enable;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}