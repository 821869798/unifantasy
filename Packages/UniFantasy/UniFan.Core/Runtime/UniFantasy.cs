
using System.Globalization;
using UniFan;
using UnityEngine;
using UnityEngine.Assertions;

public static class UniFantasy
{
    public static void InitUniFantasy(GameObject root)
    {
        Assert.IsNotNull(root ,"GameObject is null,can't to init!");

        root.AddComponent<MonoDriver>();
        Object.DontDestroyOnLoad(root);
    }

}
