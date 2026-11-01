using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
        Vector2 scroll_pos_target;

        void OnEnable() {
            find_assets();
        }

        static void find_assets() {
            prefabs.Clear();
            var prefab_guids = AssetDatabase.FindAssetGUIDs("",  new []{"Assets/"});
            foreach (GUID guid in prefab_guids) {
                var obj = AssetDatabase.LoadAssetByGUID<Object>(guid);
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
                    
                    
                    
                    string found_valid = null;
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
            float scroll_bar_width = 12;
            float rect_auto_spacing = 2;
            float button_and_spacing_height = button_height + FindWorkEditor.Styles.spacing;
            float scroll_view_height = position.height - label_height - FindWorkEditor.Styles.spacing * 3;
            
            Rect label_rect  = EditorGUILayout.GetControlRect(false, label_height);
            EditorGUI.DrawRect(label_rect, FindWorkEditor.Styles.grey_dark);
            if (GUI.Button(label_rect, "Find Work Window", FindWorkEditor.Styles.label_center)) {
                find_assets();
            }

            Rect scroll_view_rect = EditorGUILayout.GetControlRect(false, scroll_view_height);
            Rect button_rect = new (scroll_view_rect) {
                height = button_height,
                width = scroll_view_rect.width - FindWorkEditor.Styles.spacing * 2 - scroll_bar_width + rect_auto_spacing * 2,
                x = scroll_view_rect.x - rect_auto_spacing,
                y = scroll_view_rect.y - FindWorkEditor.Styles.spacing,
            };
            Rect scroll_bar_rect = new () {
                height = scroll_view_rect.height,
                width = scroll_bar_width - FindWorkEditor.Styles.spacing,
                x = scroll_view_rect.xMax - scroll_bar_width + rect_auto_spacing,
                y = scroll_view_rect.y,
            };
            
            scroll_pos.x = 0;
            Vector2 scroll_offset = new (0, -32);
            GUI.BeginClip(scroll_view_rect, scroll_offset-scroll_pos_target, Vector2.zero, false);
            int button_count = prefabs.Count * 6;
            for (int idx = 0; idx < button_count; idx++) {
                GUI.Button(button_rect, prefabs[idx % prefabs.Count].Item1);
                button_rect.y += button_and_spacing_height;
            }
            GUI.EndClip();
            
            
            // Scroll pos
            float scroll_speed = 6f;
            float scroll_lerp_speed = 0.8f;
            float scrollable_size = Math.Max(0, button_count * button_and_spacing_height - scroll_view_rect.height);
            if (Event.current.isScrollWheel) {
                scroll_pos.y += Event.current.delta.y * scroll_speed;
            }
            scroll_pos.y = Math.Clamp(scroll_pos.y, 0, scrollable_size);
            if (Vector2.Distance(scroll_pos, scroll_pos_target) > 0.05) {
                Repaint();
            }
            scroll_pos_target.y = Mathf.Lerp(scroll_pos_target.y, scroll_pos.y, Time.unscaledDeltaTime * scroll_lerp_speed);

            
            
            // Scroll bar
            float scroll_bar_height = scroll_bar_rect.height;
            float view_area_height = scroll_bar_height;
            float content_height = button_count * button_and_spacing_height;

            float scroll_handle_height = Mathf.InverseLerp(0, content_height, view_area_height);
            float room_to_scroll = Mathf.Max(content_height - view_area_height, 0);
            
            float scroll_handle_lerp = Mathf.InverseLerp(0, room_to_scroll, scroll_pos.y);
            float scroll_handle_pos = Mathf.Lerp(0, (scroll_bar_height - scroll_bar_height * scroll_handle_height), Mathf.Clamp(scroll_handle_lerp, 0f, 1f));

            Rect scroll_bar_handle_rect = new Rect(scroll_bar_rect) {
                height = scroll_handle_height * scroll_bar_height - rect_auto_spacing,
                y = scroll_bar_rect.y + scroll_handle_pos
            };
            
            EditorGUI.DrawRect(scroll_bar_rect, FindWorkEditor.Styles.grey_dark * 0.6f);
            EditorGUI.DrawRect(scroll_bar_handle_rect, FindWorkEditor.Styles.grey_dark);
        }
    }
}
