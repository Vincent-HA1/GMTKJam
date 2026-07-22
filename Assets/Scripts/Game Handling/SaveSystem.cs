using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* Static class used to handle saving the game */
public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/savefile.json";

    /* Write the SaveData instance to the path */
    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static SaveData Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null; // No save file found
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save deleted");
        }
    }
}

/* Class to represent the data needed to be saved */
[Serializable]
public class SaveData
{
    public Options options;

    public SaveData(Options options)
    {
        this.options = options;
    }
}