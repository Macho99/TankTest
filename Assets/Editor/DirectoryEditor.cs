using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DirectoryEditor : MonoBehaviour
{

    [MenuItem("MyMenu/OpenSaveDirectory")]
    public static void OpenSaveDataDirectory()
    {
        string path = $"{Application.persistentDataPath}/SaveData/";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        try
        {
            System.Diagnostics.Process.Start(directoryInfo.FullName);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

}
