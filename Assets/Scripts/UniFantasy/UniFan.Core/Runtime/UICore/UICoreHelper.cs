using UnityEngine;
using UnityEngine.UI;

namespace UniFan
{
    public static class UICoreHelper
    {

        public static void ResetRootTransform(RectTransform t)
        {
            t.localScale = Vector3.one;
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;
            t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public static Canvas AddWindowCanvas(GameObject go, string layerName, int sortOrder)
        {

            var canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = go.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingLayerName = layerName;
            canvas.sortingOrder = sortOrder;

            var raycaster = go.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = go.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }
    }
}
