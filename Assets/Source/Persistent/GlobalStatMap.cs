using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public static class GlobalStatMap
{
    // TODO change to false in the real build
    private static bool resetOnStartup = true;

    private static string globalStatsFilePath = Path.Combine(Application.persistentDataPath, "guess-whomstdve", "globalStats.json");
    public static StatMap statMap = loadGlobalStatMap();


    // keys
    public const string KEY_USERNAME = "username";


    // Implementation-dependent.
    public static StatMap initializeGlobalStatMap()
    {
        statMap = new StatMap();

        statMap.textMap.Add(KEY_USERNAME, UsernameGenerator.getNewUsername());
        return statMap;
    }

    public static void saveGlobalStatMap()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(globalStatsFilePath));

            string dataToStore = JsonConvert.SerializeObject(statMap);

            using (FileStream stream = new FileStream(globalStatsFilePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("FAILED to save global stats: " + e);
        }
    }

    public static StatMap loadGlobalStatMap()
    {
        if (File.Exists(globalStatsFilePath) && !resetOnStartup)
        {
            StatMap loadedData = null;
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(globalStatsFilePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonConvert.DeserializeObject<StatMap>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("FAILED to load global stats: " + e);
            }
            return loadedData;
        }
        else
        {
            Debug.LogWarning("Could not load global stat map. Creating one from scratch.");
            return initializeGlobalStatMap();
        }
    }


    /// SETTERS

    public static void AddOrModifyInt(string varName, int val)
    {
        if (statMap.intMap.ContainsKey(varName))
        {
            statMap.intMap[varName] = val;
        }
        else
        {
            statMap.intMap.Add(varName, val);
        }
        saveGlobalStatMap();
    }

    // Increment an integer value if it exists
    public static int IncrementInt(string varName, int inc)
    {
        if (statMap.intMap.ContainsKey(varName))
        {
            statMap.intMap[varName] += inc;
        }
        else
        {
            Debug.LogWarning("Tried to increment " + varName + " but it does not exist. Setting to supplied increment");
            statMap.intMap[varName] = inc;
        }
        saveGlobalStatMap();
        return statMap.intMap[varName];
    }

    public static void AddOrModifyFloat(string varName, float val)
    {
        if (statMap.floatMap.ContainsKey(varName))
        {
            statMap.floatMap[varName] = val;
        }
        else
        {
            statMap.floatMap.Add(varName, val);
        }
        saveGlobalStatMap();
    }

    public static void AddOrModifyText(string varName, string txt)
    {
        if (statMap.textMap.ContainsKey(varName))
        {
            statMap.textMap[varName] = txt;
        }
        else
        {
            statMap.textMap.Add(varName, txt);
        }
        saveGlobalStatMap();
    }

    public static bool AddNewBool(string varName, bool bs)
    {
        if (statMap.boolMap.ContainsKey(varName))
        {
            // This entry already exists - skip
            return false;
        }
        else
        {
            statMap.boolMap.Add(varName, bs);
            saveGlobalStatMap();
            return true;
        }
    }

    public static void AddOrModifyBool(string varName, bool bs)
    {
        if (statMap.boolMap.ContainsKey(varName))
        {
            statMap.boolMap[varName] = bs;
        }
        else
        {
            statMap.boolMap.Add(varName, bs);
        }
        saveGlobalStatMap();
    }

    // return true if this results in the variable's value being changed from what it was before
    public static bool AddOrModifyBoolReturnChanged(string varName, bool bs)
    {
        if (statMap.boolMap.ContainsKey(varName))
        {
            bool before = statMap.boolMap[varName];
            statMap.boolMap[varName] = bs;
            saveGlobalStatMap();
            return before == bs;
        }
        else
        {
            statMap.boolMap.Add(varName, bs);
            saveGlobalStatMap();
            return true;
        }
    }

    public static void AddFlag(string flagName)
    {
        statMap.flags.Add(flagName);
        saveGlobalStatMap();
    }

    public static void RemoveFlag(string flagName)
    {
        statMap.flags.Remove(flagName);
        saveGlobalStatMap();
    }

    /// GETTERS
    public static OptionType<int> GetIntMaybe(string intName)
    {
        if (statMap.intMap.ContainsKey(intName)) return new OptionType<int>(true, statMap.intMap[intName]);
        else return new OptionType<int>(false, -1);
    }

    public static OptionType<float> GetFloatMaybe(string floatName)
    {
        if (statMap.floatMap.ContainsKey(floatName)) return new OptionType<float>(true, statMap.floatMap[floatName]);
        else return new OptionType<float>(false, -1);
    }

    public static OptionType<string> GetTextMaybe(string textName)
    {
        if (statMap.textMap.ContainsKey(textName)) return new OptionType<string>(true, statMap.textMap[textName]);
        else return new OptionType<string>(false, "");
    }

    public static OptionType<bool> GetBoolMaybe(string boolName)
    {
        if (statMap.boolMap.ContainsKey(boolName)) return new OptionType<bool>(true, statMap.boolMap[boolName]);
        else return new OptionType<bool>(false, false);
    }

    public static void resetAllData()
    {
        // Delete everything but your preferences
        File.Delete(globalStatsFilePath);
        statMap = new StatMap();
    }
}

[System.Serializable]
public class StatMap
{
    ///  Other stats
    public Dictionary<string, int> intMap;
    public Dictionary<string, float> floatMap;
    public Dictionary<string, string> textMap;
    public Dictionary<string, bool> boolMap;
    public HashSet<string> flags;

    public StatMap()
    {
        intMap = new Dictionary<string, int>();
        floatMap = new Dictionary<string, float>();
        textMap = new Dictionary<string, string>();
        boolMap = new Dictionary<string, bool>();
        flags = new HashSet<string>();
    }

    /// GETTERS
    public OptionType<int> GetIntMaybe(string intName)
    {
        if (intMap.ContainsKey(intName)) return new OptionType<int>(true, intMap[intName]);
        else return new OptionType<int>(false, -1);
    }

    public OptionType<float> GetFloatMaybe(string floatName)
    {
        if (floatMap.ContainsKey(floatName)) return new OptionType<float>(true, floatMap[floatName]);
        else return new OptionType<float>(false, -1);
    }

    public OptionType<string> GetTextMaybe(string textName)
    {
        if (textMap.ContainsKey(textName)) return new OptionType<string>(true, textMap[textName]);
        else return new OptionType<string>(false, "");
    }

    // Don't need option types for bool. it's either true or not.
    public bool GetBool(string boolName)
    {
        if (boolMap.ContainsKey(boolName)) return boolMap[boolName];
        else return false;
    }
}

public class OptionType<T>
{
    public bool exists;
    public T value;

    public OptionType(bool e, T v)
    {
        exists = e;
        value = v;
    }
}