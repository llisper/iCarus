using UnityEngine;

using System;
using System.IO;
using System.Collections;

namespace Foundation
{
    public class BytesReader
    {
        public static IEnumerator Read(string path, Action<byte[]> callback)
        {
            path = Path.Combine(Application.streamingAssetsPath, path);
            if (path.Contains(":///"))
            {
                WWW www = new WWW(path);
                yield return www;
                callback(www.bytes);
            }
            else
            {
                try
                {
                    callback(File.ReadAllBytes(path));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    callback(null);
                }
            }
        }
    }
}
