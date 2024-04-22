using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LocalSaveLoader : MonoBehaviour
{
    private static string path = $"{Application.persistentDataPath}/SaveData/";
    public static void SaveDataWithLocal<T>(string fileName, T newData)
    {

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string jsonData = JsonUtility.ToJson(newData);
        string fullPath = path + fileName;
        File.WriteAllText(fullPath, jsonData);
    }
    public static bool LoadDataWithLocal<T>(string fileName, out T data)
    {
        string fullPath = path + fileName;
        if (!File.Exists(fullPath))
        {
            Debug.Log("NotFind LoadFile");
            data = default(T);
            return false;

        }
        try
        {
            string file = File.ReadAllText(fullPath);
            data = JsonUtility.FromJson<T>(file);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        data = default(T);
        return false;
    }

}
