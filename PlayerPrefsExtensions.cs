using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class PlayerPrefsExtensions
{

    public static void LogOnConsole(this List<PrefsPair> me, LogType logType = LogType.Log)
    {
        foreach (var p in me)
        {
            string log = p.Log();
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Log:
                    {
                        Debug.Log(log);
                        break;
                    }
                case LogType.Warning:
                    {
                        Debug.LogWarning(log);
                        break;
                    }
                case LogType.Error:
                case LogType.Exception:
                    {
                        Debug.LogError(log);
                        break;
                    }
            }
        }
    }
    public static void SortByAlphabetKey(this List<PrefsPair> list, bool inverse)
    {
        if (!inverse)
            list.Sort((x, y) => x.key.CompareTo(y.key));
        else
            list.Sort((x, y) => y.key.CompareTo(x.key));
    }
    public static void SortByType(this List<PrefsPair> list)
    {
        List<PrefsPair> ints = new List<PrefsPair>();
        List<PrefsPair> floats = new List<PrefsPair>();
        List<PrefsPair> strings = new List<PrefsPair>();
        foreach (var e in list)
        {
            if (e.Type == typeof(int)) ints.Add(e);
            else if (e.Type == typeof(float)) floats.Add(e);
            else strings.Add(e);
        }
        list.Clear();
        foreach (var i in ints)
            list.Add(i);
        foreach (var i in floats)
            list.Add(i);
        foreach (var i in strings)
            list.Add(i);
    }

    public static string AddRichTextBold(this string source)
    {
        return "<bold>" + source + "</bold>";
    }
    public static void SaveOnPrefs(this List<PrefsPair> me)
    {
        foreach (var p in me)
        {
            p.SaveOnPrefs(false);
        }
    }

}
