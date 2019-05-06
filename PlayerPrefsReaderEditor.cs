using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;


//fai schifo
//byte size total and specific
//fai schifo 2
public class PlayerPrefsReaderEditor : EditorWindow
{
    private string searchFilter;
    private bool intFilter = true;
    private bool floatFilter = true;
    private bool stringFilter = true;

    private bool isCollectionDirty = false;
    private bool sortState = false;
    private GUIContent cachedContent = null;
    private Vector2 scrollerPos;

    private Texture2D xmlLogo;
    private Texture2D jsonLogo;

    //cached layouts
    GUIStyle leftStyle;
    GUIStyle rightStyle;


    List<GUILayoutOption> leftLayoutOption;
    List<GUILayoutOption> rightLayoutOption;

    [MenuItem("PlayerPrefs/Fill Test")]
    public static void FillPrefs()
    {
        for (int i = 0; i < 25; i++)
        {
            PlayerPrefs.SetFloat("pippotest" + i, i);
        }
    }

    [MenuItem("PlayerPrefs/Show")]
    public static void Init()
    {
        PlayerPrefsReaderEditor window = EditorWindow.GetWindow<PlayerPrefsReaderEditor>();
        window.minSize = new Vector2(1000, 700);
        window.maxSize = new Vector2(1000, 700);
        window.Show();
    }

    private PlayerPrefsReader reader;

    private void OnGUI()
    {
        DataValidation();

        FiltersBar();



        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        //GUILayout.FlexibleSpace();
        searchFilter = EditorGUILayout.TextField(searchFilter, GUI.skin.FindStyle("ToolbarSeachTextField"));
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        {
            // Remove focus if cleared
            searchFilter = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        //scroller
        //import for both



        Left();

        Right();

        EditorGUILayout.EndHorizontal();
    }




    private void DataValidation()
    {
        if (reader == null)
        {
            reader = ScriptableObject.CreateInstance<PlayerPrefsReader>();
        }
        if (!reader.IsStructureValid())
        {
            reader.Init();
        }



        Color c;
        float darkGrey = .7f;
        float lightGrey = .7f;
        if (leftStyle == null)
        {
            c = new Color(darkGrey, darkGrey, darkGrey, 1f);
            leftStyle = new GUIStyle();
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixels(new Color[] { c, c });
            tex.Apply();
            leftStyle.normal.background = tex;
        }
        if (rightStyle == null)
        {
            c = new Color(lightGrey, lightGrey, lightGrey, 1f);
            rightStyle = new GUIStyle();
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixels(new Color[] { c, c });
            tex.Apply();
            rightStyle.normal.background = tex;
        }

        if (leftLayoutOption == null)
        {
            leftLayoutOption = new List<GUILayoutOption>();
            leftLayoutOption.Add(GUILayout.Width(40));
            leftLayoutOption.Add(GUILayout.Height(40));
        }
        if (xmlLogo == null)
        {
            xmlLogo = Resources.Load<Texture2D>("xmlLogo");
        }
        if (jsonLogo == null)
        {
            jsonLogo = Resources.Load<Texture2D>("jsonLogo");
        }
    }

    private void FiltersBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        intFilter = GUILayout.Toggle(intFilter, new GUIContent("int"), EditorStyles.toolbarButton);
        floatFilter = GUILayout.Toggle(floatFilter, new GUIContent("float"), EditorStyles.toolbarButton);
        stringFilter = GUILayout.Toggle(stringFilter, new GUIContent("string"), EditorStyles.toolbarButton);

        EditorGUILayout.EndHorizontal();
    }
    private void Right()
    {
        EditorGUILayout.BeginVertical();
        //scrollerPos = EditorGUILayout.BeginScrollView(scrollerPos);
        //scrollerPos = GUILayout.BeginScrollView(scrollerPos);
        if (isCollectionDirty)
        {
            reader.Refresh();
            isCollectionDirty = false;
        }

        Draw(reader.loadedPrefs);
        //GUILayout.EndScrollView();
        //EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    private void Left()
    {
        EditorGUILayout.BeginVertical(leftStyle, GUILayout.Height(10000), GUILayout.MaxWidth(100));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(EditorGUIUtility.IconContent("vcs_refresh"), leftLayoutOption.ToArray()))
        {
            reader.Refresh();
            GUI.FocusControl(null);

        }

        cachedContent = EditorGUIUtility.IconContent("SaveActive");
        cachedContent.tooltip = "Save all preferences";
        if (GUILayout.Button(cachedContent, leftLayoutOption.ToArray()))
        {
            GUI.FocusControl(null);
            bool succes = reader.SaveAll();
            if(!succes)
            {
                EditorUtility.DisplayDialog("ERROR", "Some key is empty.", "Continue");
                return;
            }
            reader.Refresh();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("Toolbar Plus"), FocusType.Keyboard, leftLayoutOption.ToArray()))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Int"), false, () =>
            {
                reader.AddPref("", (int)0);
                GUI.FocusControl(null);

            });
            menu.AddItem(new GUIContent("Float"), false, () =>
            {
                reader.AddPref("", (float)0f);
                GUI.FocusControl(null);

            });
            menu.AddItem(new GUIContent("String"), false, () =>
            {
                reader.AddPref("", "default");
                GUI.FocusControl(null);

            });
            menu.ShowAsContext();
        }
        Color c = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0f, 0f, 0.6f);
        cachedContent = EditorGUIUtility.IconContent("SVN_DeletedLocal");
        cachedContent.tooltip = "Delete all preferences";
        if (GUILayout.Button(cachedContent, leftLayoutOption.ToArray()))
        {
            bool result = EditorUtility.DisplayDialog("Warning!!", "Do you want to delete all Player Prefs?", "OK", "NO");
            if (result)
            {
                reader.DeleteAll();
                reader.Refresh();
                GUI.FocusControl(null);
            }
        }
        GUI.backgroundColor = c;
        EditorGUILayout.EndHorizontal();



        //sorting
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Width(90));
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Sort", GUILayout.Width(90));

        EditorGUILayout.BeginHorizontal();
        cachedContent = EditorGUIUtility.IconContent("AlphabeticalSorting");
        cachedContent.tooltip = "Alphabetic sort";
        if (GUILayout.Button(cachedContent, leftLayoutOption.ToArray()))
        {
            //alphabet sort
            GUI.FocusControl(null);
            reader.loadedPrefs.SortByAlphabetKey(sortState);
            sortState = !sortState;
        }
        cachedContent = EditorGUIUtility.IconContent("FilterByType");
        cachedContent.tooltip = "Type sort";
        if (GUILayout.Button(cachedContent, leftLayoutOption.ToArray()))
        {
            //typesort sort
            GUI.FocusControl(null);
            reader.loadedPrefs.SortByType();
        }

        EditorGUILayout.EndHorizontal();



        //options export
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Width(90));
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Export", GUILayout.Width(90));
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent(xmlLogo, "Export xml"), leftLayoutOption.ToArray()))
        {
            string path = EditorUtility.SaveFilePanel("Save file xml", "", "", "xml");
            reader.ExportXml(path);
        }

        if (GUILayout.Button(new GUIContent(jsonLogo, "Export Json"), leftLayoutOption.ToArray()))
        {
            string path = EditorUtility.SaveFilePanel("Save file json", "", "", "json");
            reader.ExportJson(path);
        }
        EditorGUILayout.EndHorizontal();


        //options import
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Width(90));
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Import", GUILayout.Width(90));
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent(xmlLogo, "Import Xml"), leftLayoutOption.ToArray()))
        {

        }

        if (GUILayout.Button(new GUIContent(jsonLogo, "Import Json"), leftLayoutOption.ToArray()))
        {
            bool result = EditorUtility.DisplayDialog("WARNING", "this action will delete all of your player prefs. Are you sure?", "YES", "NO");
            if (!result) return;

            string path = EditorUtility.OpenFilePanel("Select file", "", "");
            reader.ImportFromJson(path);
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.EndVertical();

    }
    private bool KeyControls(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        return true;
    }
    private void Draw(List<PrefsPair> list)
    {
        float h = Mathf.Min(35 * list.Count, 670);
        scrollerPos = EditorGUILayout.BeginScrollView(scrollerPos, GUILayout.MaxHeight(h));
        foreach (var elem in list)
        {
            System.Type t = elem.Type;
            if (!intFilter && t == typeof(int)) continue;
            if (!floatFilter && t == typeof(float)) continue;
            if (!stringFilter && t == typeof(string)) continue;

            Draw(elem);
        }
        EditorGUILayout.EndScrollView();
    }
    private void Draw(PrefsPair pair)
    {
        if (!string.IsNullOrEmpty(searchFilter) && !pair.key.Contains(searchFilter)) return;

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        Field(pair);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
    }
    private void Field(PrefsPair pair)
    {
        GUIStyle baseStyle = new GUIStyle(GUI.skin.textField);
        if (pair.IsDirty)
            baseStyle.fontStyle = FontStyle.Bold;

        GUIStyle typeStyle = new GUIStyle(GUI.skin.label);

        //EditorGUILayout.Space();
        string keyFocusName = "keyControlName";
        GUI.SetNextControlName(keyFocusName);
        pair.key = EditorGUILayout.TextField(pair.key, baseStyle, GUILayout.Width(250));
        EditorGUILayout.Space();

        if (pair.Type == typeof(int))
        {
            typeStyle.normal.textColor = Color.blue;
            GUILayout.Label("int", typeStyle, GUILayout.MaxWidth(45));
            pair.value = (int)EditorGUILayout.IntField(pair.Value<int>(), baseStyle, GUILayout.Width(150));
        }
        else if (pair.Type == typeof(float))
        {
            typeStyle.normal.textColor = Color.blue;
            GUILayout.Label("float", typeStyle, GUILayout.MaxWidth(45));
            pair.value = (float)EditorGUILayout.FloatField(pair.Value<float>(), baseStyle, GUILayout.Width(150));
        }
        else if (pair.Type == typeof(string))
        {
            typeStyle.normal.textColor = Color.red;
            GUILayout.Label("string", typeStyle, GUILayout.MaxWidth(45));
            pair.value = (string)EditorGUILayout.TextField(pair.Value<string>(), baseStyle, GUILayout.Width(150));
        }
        EditorGUILayout.Space();

        cachedContent = EditorGUIUtility.IconContent("Grid.EraserTool");
        cachedContent.tooltip = "Restore the default state";
        if (GUILayout.Button(cachedContent, GUILayout.Height(20), GUILayout.Width(35)))
        {
            pair.ResetToDiskState();
            GUI.FocusControl(null);
        }

        cachedContent = EditorGUIUtility.IconContent("SaveActive");
        cachedContent.tooltip = "Save this preference on disk";
        if (GUILayout.Button(cachedContent, GUILayout.Height(20), GUILayout.Width(35)))
        {
            GUI.FocusControl(null);
            pair.SaveOnPrefs(true, delegate { EditorUtility.DisplayDialog("ERROR", "Set a key for the selected value", "Continue"); EditorGUI.FocusTextInControl(keyFocusName); });
        }

        Color c = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0, 0, 0.6f);
        cachedContent = EditorGUIUtility.IconContent("SVN_DeletedLocal");
        cachedContent.tooltip = "Delete this preference from disk";
        if (GUILayout.Button(cachedContent, GUILayout.Height(20), GUILayout.Width(35)))
        {
            pair.DeleteFromPrefs();
            GUI.FocusControl(null);
            isCollectionDirty = true;
        }
        GUI.backgroundColor = c;

    }
}
