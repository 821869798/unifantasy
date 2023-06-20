using UnityEngine.UI;

namespace UniFan
{
    //不绘制的UI，但是会阻挡UI事件，一般用于不可见的Mask
    public class UINoDrawRaycast : MaskableGraphic
    {
        protected UINoDrawRaycast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}