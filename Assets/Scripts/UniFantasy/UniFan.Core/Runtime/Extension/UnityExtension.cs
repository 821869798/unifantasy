using UnityEngine;
using UnityEngine.UI;

public static class UnityExtension
{

    public static T Find<T>(this Transform transform, string path) where T : UnityEngine.Component
    {
        Transform node = transform.Find(path);
        if (node != null)
        {
            return node.GetComponent<T>();
        }
        return null;
    }

    public static T Find<T>(this GameObject gameOjbect, string path) where T : UnityEngine.Component
    {
        return gameOjbect.transform.Find<T>(path);
    }


    public static GameObject Instantiate(this GameObject go)
    {
        return GameObject.Instantiate(go, go.transform.parent);
    }

    public static GameObject Instantiate(this GameObject go, Transform parent)
    {
        return GameObject.Instantiate(go, parent);
    }

    public static GameObject Instantiate(this GameObject go, GameObject parent)
    {
        return GameObject.Instantiate(go, parent.transform);
    }

    public static UnityEngine.Component Instantiate(this UnityEngine.Component go)
    {
        return UnityEngine.Object.Instantiate(go, go.transform.parent);
    }

    public static void SetLayer(this Transform trans, int layerMask)
    {
        foreach (var child in trans.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layerMask;
        }
    }

    /// <summary>
    /// 删除transform的子物体
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="destroyImmediate"></param>
    public static void DestroyChildren(this Transform transform, bool destroyImmediate = false)
    {
        if (transform == null)
            return;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (destroyImmediate)
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            else
                GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 删除transform的子物体,除了exceptTransform
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="destroyImmediate"></param>
    public static void DestroyChildrenExcept(this Transform transform, Transform exceptTransform, bool destroyImmediate = false)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == exceptTransform)
                continue;
            if (destroyImmediate)
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            else
                GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 设置局部坐标
    /// </summary>
    /// <param name="t"></param>
    /// <param name="x"></param>
    public static void SetLocalX(this Transform t, float x)
    {
        var pos = t.localPosition;
        pos.x = x;
        t.localPosition = pos;
    }

    public static void SetLocalY(this Transform t, float y)
    {
        var pos = t.localPosition;
        pos.y = y;
        t.localPosition = pos;
    }

    public static void SetLocalZ(this Transform t, float z)
    {
        var pos = t.localPosition;
        pos.z = z;
        t.localPosition = pos;
    }

    public static void SetAnchoredPosition(this RectTransform t, float x, float y)
    {
        t.anchoredPosition = new Vector2(x, y);
    }

    public static void SetPosition(this Transform t, float x, float y, float z)
    {
        t.position = new Vector3(x, y, z);
    }

    public static void SetLocalPosition(this Transform t, float x, float y, float z)
    {
        t.localPosition = new Vector3(x, y, z);
    }

    public static void SetLocalScale(this Transform t, float x, float y, float z)
    {
        t.localScale = new Vector3(x, y, z);
    }

    //public static void ChangeEndValueV3(this Tweener t, float x, float y, float z, bool snapStartValue)
    //{
    //    t.ChangeEndValue(new Vector3(x, y, z), snapStartValue);
    //}

    private static readonly Vector3[] _rectWorldCorner = new Vector3[4];
    public static Vector3[] ExGetWorldCorners(this RectTransform transform)
    {
        transform.GetWorldCorners(_rectWorldCorner);
        return _rectWorldCorner;
    }

    //预先设置text的宽高
    public static Vector2 PreSetTextSizeDelta(this Text text, string content, bool withWidth, bool withHeight, Vector2? extents = null)
    {
        if (extents == null)
            extents = text.rectTransform.rect.size; //使用默认的限制范围

        var tGenerator = text.cachedTextGeneratorForLayout;
        var tSettings = text.GetGenerationSettings(extents.Value);
        tSettings.scaleFactor = 1; //解决低分辨率计算的结果太小的问题
        var size = text.rectTransform.sizeDelta;

        var sizeOffset = 3; //让计算结果大一点，防止大小刚刚好导致显示不出来

        if (withWidth)
        {
            var width = tGenerator.GetPreferredWidth(content, tSettings);
            size.x = width + sizeOffset;
        }
        if (withHeight)
        {
            var height = tGenerator.GetPreferredHeight(content, tSettings);
            size.y = height + sizeOffset;
        }

        text.rectTransform.sizeDelta = size;

        return size;
    }

    public static Material GetMaterial(this Renderer render)
    {
#if UNITY_EDITOR
        return render.material;
#else
		return render.sharedMaterial;
#endif
    }
    public static void SetMaterial(this Renderer render, Material material)
    {
#if UNITY_EDITOR
        render.material = material;
#else
		render.sharedMaterial = material;
#endif
    }


    public static Material[] GetMaterialArray(this Renderer render)
    {
#if UNITY_EDITOR
        return render.materials;
#else
		return render.sharedMaterials;
#endif
    }

}

