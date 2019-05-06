using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Microsoft.Win32;
using System.IO;
using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

public partial class PlayerPrefsReader : ScriptableObject
{
    //will be written by a specific reader
    public List<PrefsPair> loadedPrefs { get; private set; }

    // Use this for initialization
    public void Init()
    {
        loadedPrefs = new List<PrefsPair>();

        Refresh();
    }

    /// <summary>
    /// Read all data from persistent path
    /// </summary>
    public void Refresh()
    {
        loadedPrefs.Clear();

        if (Application.platform == RuntimePlatform.WindowsEditor)
            new WindowsEditorReader().Read(this);
        else if (Application.platform == RuntimePlatform.OSXEditor)
            new OSXEditor().Read(this);

        loadedPrefs.LogOnConsole();
    }
    /// <summary>
    /// save to the persistent data path all the values. Return true if the save was succesful
    /// </summary>
    public bool SaveAll()
    {
        foreach (var p in loadedPrefs)
        {
            if (string.IsNullOrEmpty(p.key)) return false;
        }

        PlayerPrefs.DeleteAll();

        loadedPrefs.SaveOnPrefs();

        PlayerPrefs.Save();
        return true;
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public void AddPref(string key, object value)
    {
        PrefsPair newPref = new PrefsPair(key, value);
        loadedPrefs.Add(newPref);
    }

    /// <summary>
    /// Check if there is some data inconsistency in loaded prefs
    /// </summary>
    /// <returns></returns>
    public bool IsStructureValid()
    {
        if (loadedPrefs == null) return false;
        foreach (var p in loadedPrefs)
        {
            if (p.value == null) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the path where current player prefs are stored
    /// </summary>
    /// <param name="logPath"></param>
    /// <returns></returns>
    private string GetPrefsPath(bool logPath = true)
    {
        string path;
        string companyName = UnityEditor.PlayerSettings.companyName;
        string productName = UnityEditor.PlayerSettings.productName;


        if (Application.platform == RuntimePlatform.WindowsEditor)
            path = "Software\\Unity\\UnityEditor\\" + companyName + "\\" + productName;
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
            path = "HKEY_CURRENT_USER/Software/" + companyName + "/" + productName;
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            string plistFileName = string.Format("unity.{0}.{1}.plist", companyName, productName);
            path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Preferences"), plistFileName);
        }
        else
            path = null;

        if (logPath)
            Debug.Log(path);
        return path;
    }

    //private void MockPrefs()
    //{
    //    for (int i = 0; i < 5; i++)
    //        PlayerPrefs.SetInt("test0_int" + i, 10 + i);

    //    for (int i = 0; i < 5; i++)
    //        PlayerPrefs.SetFloat("test0_float" + i, 10.5f);

    //    for (int i = 0; i < 5; i++)
    //        PlayerPrefs.SetString("test0_string" + i, "str_" + (10 - i));

    //    for (int i = 0; i < 5; i++)
    //        PlayerPrefs.SetString("Unity.test0_string" + i, "str_" + (10 - i));
    //}



    #region EXPORTERS
    public void ExportXml(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        XmlSerializer serializer = new XmlSerializer(typeof(JsonHandler));
        JsonHandler handler = new JsonHandler();
        handler.prefs = this.loadedPrefs.ToArray();

        using (FileStream stream = File.Create(path))
        {
            serializer.Serialize(stream, handler);
        }
    }
    public void ExportJson(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        JsonHandler handler = new JsonHandler();
        handler.prefs = this.loadedPrefs.ToArray();

        string json = JsonConvert.SerializeObject(handler);
        using (StreamWriter sw = new StreamWriter(File.Create(path)))
        {
            sw.Write(json);
        }
    }
    #endregion

    #region IMPORTERS
    public void ImportFromJson(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (!File.Exists(path)) return;

        string file = File.ReadAllText(path);
        JsonHandler handler = JsonConvert.DeserializeObject<JsonHandler>(file);
        if(handler == null)
        {
            Debug.LogError("I'm not able to read the file");
            return;
        }

        this.DeleteAll();
        this.loadedPrefs.Clear();
        this.loadedPrefs = handler.prefs.ToList();
        this.SaveAll();
    }
}
#endregion

#region INTERNAL_SERIALIZABLE
[System.Serializable]
public class JsonHandler
{
    public PrefsPair[] prefs;

}

[System.Serializable]
public class PrefsPair
{
    public PrefsPair()
    {
        //used only for xml serialization
    }
    public string key;
    public object value;

    private string onDiskKey;
    private object onDiskValue;

    public byte size;

    [JsonIgnore]
    public bool IsDirty
    {
        get
        {
            bool isValueEqual = true;
            if (value.GetType() == typeof(int))
            {
                isValueEqual = (int)onDiskValue == (int)value;
            }
            else if (value.GetType() == typeof(float))
            {
                isValueEqual = (float)onDiskValue == (float)value;
            }
            else if (value.GetType() == typeof(string))
            {
                isValueEqual = (string)onDiskValue == (string)value;
            }

            return (onDiskKey != key || !isValueEqual);
        }
    }
    [JsonIgnore]
    public Type Type { get { return value.GetType(); } }
    public T Value<T>() { return (T)value; }
    public void SetNotDirty()
    {
        onDiskKey = key;
        onDiskValue = value;
    }

    public PrefsPair(string key, object value)
    {
        this.key = key;
        this.value = value;

        this.onDiskKey = key;
        this.onDiskValue = value;
    }

    public string Log()
    {
        return key + " => " + value;
    }
    public void ResetToDiskState()
    {
        key = onDiskKey;
        value = onDiskValue;
    }
    public void SaveOnPrefs(bool destroyPrevKey, UnityEngine.Events.UnityAction onFailure = null)
    {
        if(string.IsNullOrEmpty(this.key))
        {
            if (onFailure != null) onFailure.Invoke();
            return;
        }

        if (destroyPrevKey)
            PlayerPrefs.DeleteKey(onDiskKey);

        SetNotDirty();

        System.Type t = value.GetType();
        if (t == typeof(int))
        {
            PlayerPrefs.SetInt(key, Value<int>());
        }
        else if (t == typeof(float))
        {
            PlayerPrefs.SetFloat(key, Value<float>());
        }
        else if (value.GetType() == typeof(string))
        {
            PlayerPrefs.SetString(key, Value<string>());
        }
        else if(t == typeof(long))
        {
            value = Convert.ToInt32(value);
            onDiskValue = Convert.ToInt32(value);
            PlayerPrefs.SetInt(key, (int)value);
        }
        else if (t == typeof(double))
        {
            value = Convert.ToSingle(value);
            onDiskValue = Convert.ToSingle(value);
            PlayerPrefs.SetFloat(key, (float)value);
        }
        PlayerPrefs.Save(); //idk if it's needed
    }
    public void DeleteFromPrefs()
    {
        PlayerPrefs.DeleteKey(onDiskKey);
    }
}
#endregion