namespace MP.Unity.Editor
{
    using UnityEngine;
    using Microsoft.Win32;
    using System.Text;

    public partial class PlayerPrefsReader : ScriptableObject
    {
        private class WindowsEditorReader
        {
            public void Read(PlayerPrefsReader myreader)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(myreader.GetPrefsPath());
                if (key == null)
                {
                    Debug.Log("There aren't player prefs saved");
                }

                foreach (var name in key.GetValueNames())
                {
                    if (name.StartsWith("Unity.") || name.StartsWith("unity.")) //don't show default unity prefs
                        continue;

                    int lastIndex = name.LastIndexOf('_');
                    string correctName = name.Substring(0, lastIndex);

                    object val = key.GetValue(name);

                    if (val is System.Int32)
                    {
                        if (PlayerPrefs.GetInt(correctName, -1) == -1 && PlayerPrefs.GetInt(correctName, 0) == 0)   //double check
                        {
                            myreader.AddPref(correctName, PlayerPrefs.GetFloat(correctName));
                        }
                        else
                        {
                            myreader.AddPref(correctName, (int)val);
                        }
                    }
                    else if (val is byte[])
                    {
                        string str = Encoding.UTF8.GetString((byte[])val);
                        myreader.AddPref(correctName, str);
                    }

                }
            }
        }
        private class OSXEditor
        {
            public void Read(PlayerPrefsReader myreader)
            {
                //string path = myreader.GetPrefsPath();
                //if (File.Exists(path))
                //{
                //    //HERE THERE IS A BUG
                //    object plist = Plist.readPlist(path);

                //    Dictionary<string, object> parsed = plist as Dictionary<string, object>;

                //    foreach (KeyValuePair<string, object> pair in parsed)
                //    {
                //        if (pair.Value.GetType() == typeof(System.Int32))
                //        {
                //            myreader.AddPref(pair.Key, (int)pair.Value);
                //        }
                //        else if (pair.Value.GetType() == typeof(System.Single))
                //        {
                //            myreader.AddPref(pair.Key, (float)pair.Value);
                //        }
                //        else
                //        {
                //            myreader.AddPref(pair.Key, (string)pair.Value);
                //        }
                //    }
                //}
            }
        }
    }
}
