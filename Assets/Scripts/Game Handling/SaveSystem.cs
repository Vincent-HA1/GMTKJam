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
    public int levelIndex;
    public int fragments;
    public PlayerUpgrades upgrades;
    public PlayerStats stats;
    public SaveData(Options options, int levelIndex, int fragments, PlayerUpgrades upgrades, PlayerStats stats)
    {
        this.options = options;
        this.levelIndex = levelIndex;
        this.fragments = fragments;
        this.upgrades = upgrades;
        this.stats = stats;
    }
}

/* Player Upgrades */
[Serializable]
public class PlayerUpgrades
{
    public bool healthUp;
    public bool speedUp;
    public bool rangeUp;
    public bool jumpUp;
    public bool bepoUnlocked;
    public bool specialUnlocked;

    // Returns true/false based on the enum requested
    public bool IsUnlocked(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Jump: return jumpUp;
            case UpgradeType.Speed: return speedUp; 
            case UpgradeType.Health: return healthUp; 
            case UpgradeType.Special: return specialUnlocked;
            case UpgradeType.Bepo: return bepoUnlocked; 
            case UpgradeType.Range: return rangeUp; 
            default: return false;
        }
    }

    // Unlocks the matching boolean flag
    public void SetUnlocked(UpgradeType type, bool unlocked = true)
    {
        switch (type)
        {
            case UpgradeType.Jump: jumpUp = unlocked; break;
            case UpgradeType.Speed: speedUp = unlocked; break;
            case UpgradeType.Health: healthUp = unlocked; break;
            case UpgradeType.Special: specialUnlocked = unlocked; break;
            case UpgradeType.Bepo: bepoUnlocked = unlocked; break;
            case UpgradeType.Range: rangeUp = unlocked; break;

        }
    }
}

[Serializable]
public enum UpgradeType
{
    Jump,
    Speed,
    Bepo,
    Special,
    Health,
    Range
}

[Serializable]
public class PlayerStats
{
    //Defaults
    public float jumpMult = 1;
    public float speedMult = 1;
    public float healthAdd = 0;
    public float rangeMult = 1;
}