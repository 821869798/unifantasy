using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace UniFan.Res
{
    public static class CriAssetsUtility
    {

        private static readonly byte[] abKey = System.Text.Encoding.ASCII.GetBytes("8Yy7p8uiq9yF7XmyxRyl3");


        /// <summary>
        /// 生成密钥 15位十进制数
        /// 56字节7位无符号整形
        /// However, in order to avoid restrictions on the export of content, only the lower 56 bits are used in the plug-in.
        /// Therefore, the actual effective encryption key range is 1 to 72057594037927936.
        /// </summary>
        /// <returns></returns>
        public static string GenerateCryptKey()
        {
            var tmpKey = new byte[abKey.Length];
            System.Array.Copy(abKey, tmpKey, abKey.Length);
            for (int i = 0; i < tmpKey.Length; i++)
            {
                tmpKey[i] ^= tmpKey[i];
                tmpKey[i] ^= (byte)(abKey[i] ^ tmpKey[i]);
                tmpKey[i] ^= (byte)(i);
            }
            MD5 md5 = MD5.Create();
            byte[] MD5buffer = md5.ComputeHash(tmpKey);
            string result = "";
            for (int i = 0; i < 6; i++)
            {
                result += MD5buffer[i].ToString("D");
            }
            return result;
        }
    }
}