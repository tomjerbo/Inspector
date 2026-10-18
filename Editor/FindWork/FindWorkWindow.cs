using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        static readonly List<(string, GameObject, FindWork)> prefabs = new (64);

        Vector2 scroll_pos;


        void OnEnable() {
            find_assets();
        }

        static void find_assets() {
            prefabs.Clear();
            var prefab_guids = AssetDatabase.FindAssets("",  new []{"Assets/"});
            foreach (string guid in prefab_guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null && obj is GameObject gob) {
                    
                    PrefabAssetType prefab_type = PrefabUtility.GetPrefabAssetType(gob);
                    HashSet<FindWork> work_cmps = new (6);
                    bool skip = false;
                    
                    switch (prefab_type) {
                        case PrefabAssetType.Variant:
                        case PrefabAssetType.Model:
                            var comps = PrefabUtility.GetAddedComponents(gob);
                            foreach (AddedComponent comp in comps) {
                                if (comp.instanceComponent is FindWork work) {
                                    work_cmps.Add(work);
                                }
                            }
                            break;
                            
                        case PrefabAssetType.Regular:
                            work_cmps.Add(gob.GetComponent<FindWork>());
                            var child_comp = gob.transform.root.GetComponentsInChildren<FindWork>();
                            foreach (FindWork work in child_comp) {
                                work_cmps.Add(work);
                            }
                            break;
                        
                        default: skip = true;
                            break;
                    }
                    if (skip) continue;
                    
                    
                    
                    string found_valid = "";
                    FindWork fw = null;
                    foreach (FindWork work_cmp in work_cmps) {
                        if (work_cmp == null) 
                            continue;
                        
                        // prefabs.Add((gob.name, false));
                        var nearest_instance_root= PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(work_cmp);
                        var nearest_instance_root_gob = AssetDatabase.LoadAssetAtPath<GameObject>(nearest_instance_root);
                        // var nearest_prefab_instance_root = PrefabUtility.GetNearestPrefabInstanceRoot(work_cmp);
                        // var outer_prefab_instance_root = PrefabUtility.GetOutermostPrefabInstanceRoot(work_cmp);
                        // var original_source_root = PrefabUtility.GetOriginalSourceRootWhereGameObjectIsAdded(work_cmp.gameObject);

                        bool near_root_same        = obj == nearest_instance_root_gob;
                        // bool near_prefab_ist_same  = obj == nearest_prefab_instance_root;
                        // bool outer_prefab_ist_same = obj == outer_prefab_instance_root;
                        // bool org_source_root_same  = obj == original_source_root;
                        
                        
                        if (near_root_same) {
                            found_valid = work_cmp.name;
                            fw = work_cmp;
                        }
                        
                        // Debug.Log($"{prefab_type} - {work_cmp.gameObject.name}\n" +
                        //           $"nearest_instance_root {nearest_instance_root}\n" +
                        //           $"nearest_instance_root_gob {nearest_instance_root_gob}\n" + 
                        //           $"nearest_prefab_instance_root {nearest_prefab_instance_root}\n" + 
                        //           $"outer_prefab_instance_root {outer_prefab_instance_root}\n" + 
                        //           $"original_source_root {original_source_root}\n"
                        //           );
                    }

                    bool has_value = string.IsNullOrEmpty(found_valid) == false;
                    if (has_value) {
                        prefabs.Add((obj.name + (has_value && string.CompareOrdinal(obj.name, found_valid) != 0 ? " -> " + found_valid : ""), gob, fw));
                    }
                    
                    // prefabs.Add(obj);
                }
            }
            
            Debug.Log($"Added {prefabs.Count} prefabs");
        }
        
        void OnGUI() {
            float label_height  = 32;
            float button_height = 52;
            
            Rect label_rect  = EditorGUILayout.GetControlRect(false, label_height);
            if (GUI.Button(label_rect, "Find Work Window", FindWorkEditor.Styles.label_center)) {
                find_assets();
            }
            
            
            scroll_pos = GUILayout.BeginScrollView(scroll_pos);
            for (int idx = 0; idx < prefabs.Count * 8; idx++) {
                Texture prefab_tex = AssetPreview.GetAssetPreview(prefabs[idx % prefabs.Count].Item3);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent($"{prefabs[idx % prefabs.Count].Item1}", prefab_tex), GUILayout.Height(button_height))) {
                    // EditorGUIUtility.PingObject(prefabs[idx].Item1);
                }
                if (GUILayout.Button(new GUIContent($"{prefabs[idx % prefabs.Count].Item1}", prefab_tex), GUILayout.Height(button_height))) {
                    // EditorGUIUtility.PingObject(prefabs[idx].Item1);
                }
                
                if (GUILayout.Button(new GUIContent($"{prefabs[idx % prefabs.Count].Item1}", prefab_tex), GUILayout.Height(button_height))) {
                    // EditorGUIUtility.PingObject(prefabs[idx].Item1);
                }
                
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}
