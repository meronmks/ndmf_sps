using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    using runtime;
    internal static class Localization
    {
        private static readonly string PathPref = $"{UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder}/com.meronmks";
        private static readonly string FilenameSetting = Values.PACKAGE_NAME + ".language.conf";
        private static string PathSetting => $"{PathPref}/{FilenameSetting}";
        
        private const string LocalizationPathGuid = "6ce4074f31144a99808b160c7f044db4";
        private static string localizationPathRoot = AssetDatabase.GUIDToAssetPath(LocalizationPathGuid);
        
        private static List<Dictionary<string, string>> languages = new();
        private static List<string> codes = new();
        private static string[] names;
        private static int number;
        private static bool isLoaded = false;

        [InitializeOnLoadMethod]
        internal static void LoadDatas()
        {
            var folder = GetLanguageFolder();
            if (string.IsNullOrEmpty(folder))
            {
                languages.Add(new Dictionary<string, string>());
                codes.Add("");
                names = new string[]{""};
                number = 0;
                Debug.LogError("Failed to load language file");
                return;
            }
            
            languages.Clear();
            codes.Clear();

            var paths = Directory.GetFiles(folder, "*.json");
            var tmpNames = new List<string>();
            foreach(var path in paths)
            {
                var langData = File.ReadAllText(path);
                var lang = JsonConvert.DeserializeObject<Dictionary<string,string>>(langData);
                if(lang == null) continue;

                // 言語ファイルの名前が言語コードと一致していることを期待
                var code = Path.GetFileNameWithoutExtension(path);
                languages.Add(lang);
                codes.Add(code);
                tmpNames.Add(lang.TryGetValue("Language", out string o) ? o : code);
            }
            names = tmpNames.ToArray();
            number = GetIndexByCode(LoadLanguageSettings());
            isLoaded = true;
        }
        
        internal static string[] GetCodes()
        {
            return codes.ToArray();
        }
        
        private static int GetIndexByCode(string code)
        {
            var index = codes.IndexOf(code);
            if(index == -1) index = codes.IndexOf("en-US");
            if(index == -1) index = 0;
            return index;
        }
        
        private static string S(string key, int index)
        {
            return languages[index].TryGetValue(key, out string o) ? o : null;
        }

        internal static string S(string key, string code)
        {
            return S(key, GetIndexByCode(code));
        }

        internal static string S(string key)
        {
            return S(key, number);
        }
        
        internal static GUIContent G(string key)
        {
            if(DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) return new GUIContent(S(key) ?? key);
            return new GUIContent(S(key) ?? key, S($"{key}.tooltip"));
        }

        internal static GUIContent G(SerializedProperty property)
        {
            return G($"inspector.{property.name}");
        }
        
        internal static bool SelectLanguageGUI()
        {
            if (!isLoaded)
            {
                if (GUILayout.Button("Reload Language"))
                {
                    LoadDatas();
                }
                return false;
            }
            
            EditorGUI.BeginChangeCheck();
            number = EditorGUILayout.Popup("Editor Language", number, names);
            if (EditorGUI.EndChangeCheck())
            {
                SaveLanguageSettings();
                return true;
            }
            return false;
        }

        private static string LoadLanguageSettings()
        {
            if(!Directory.Exists(PathPref)) Directory.CreateDirectory(PathPref);
            if(!File.Exists(PathSetting)) File.WriteAllText(PathSetting, CultureInfo.CurrentCulture.Name);
            return SafeIO.LoadFile(PathSetting);
        }
        
        private static void SaveLanguageSettings()
        {
            if(!Directory.Exists(PathPref)) Directory.CreateDirectory(PathPref);
            SafeIO.SaveFile(PathSetting, codes[number]);
        }

        private static string GetLanguageFolder()
        {
            var folder = AssetDatabase.GUIDToAssetPath(LocalizationPathGuid);
            if(!string.IsNullOrEmpty(folder) && Directory.Exists(folder)) return folder;
            return null;
        }
    }
    
    internal class SafeIO
    {
        internal static void SaveFile(string path, string content)
        {
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.SetLength(0);
                using(var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(content);
                }
            }
        }

        internal static string LoadFile(string path)
        {
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var sr = new StreamReader(fs, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
    }
}