using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JsonDataService : IDataService
{
    public bool SaveData<T>(string relativePath, T data)
    {
        string path = Application.persistentDataPath + relativePath;

        try 
        {
            if (File.Exists(path))
            {
                Debug.Log("data exists, deleting old and writing new data file");
                File.Delete(path);
            }
            else
            {
                Debug.Log("data file does not exist, creating new data file.");
            }

            Debug.Log("Writing data file to: " + path);

            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e) 
        {
            Debug.LogError("Error writing data file: " + e.Message);
            return false;
        }
    }

    public T LoadData<T>(string relativePath)
    {
        string path = Application.persistentDataPath + relativePath;
        if (!File.Exists(path))
        {
            Debug.LogError($"Cannot load file at {path}, file does not exist.");
            throw new FileNotFoundException($"Cannot load file at {path}, file does not exist.");
        }

        try
        {
            T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data file: {e.Message}");
            throw e;
        }
    }
}
