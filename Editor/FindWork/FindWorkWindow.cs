using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Analytics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jerbo.Inspector
{
    
    public class FindWorkWindow : EditorWindow
    {
        [MenuItem("Jerbo/Find Work")]
        static void open_window() {
            find_work_window = GetWindow<FindWorkWindow>(window_title, true);
            find_work_window.minSize = new Vector2(240, 360);
            find_work_window.Show();
            find_assets();
        }

        const string window_title = "Find Work";
        static FindWorkWindow find_work_window;
        static List<Object> prefabs = new List<Object>();

        Vector2 scroll_pos;

        static void find_assets() {
            prefabs.Clear();
            var prefab_guids = AssetDatabase.FindAssets("t:ScriptableObject",  new []{"Assets/"});
            foreach (string guid in prefab_guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null)
                {
                    prefabs.Add(obj);
                }
            }
        }
        void OnGUI() {
            if (GUILayout.Button("Find Work Label", EditorStyles.boldLabel, GUILayout.Height(42))) {
                find_assets();
            }
            scroll_pos = GUILayout.BeginScrollView(scroll_pos);
            for (int idx = 0; idx < prefabs.Count; idx++) {
                if (GUILayout.Button($"{prefabs[idx].name}", GUILayout.Height(30))) {
                    EditorGUIUtility.PingObject(prefabs[idx]);
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
