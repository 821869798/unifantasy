using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotMain
{

    public static void EnterHotMain()
    {
        Debug.Log("Enter HotMain");
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

    }


}
