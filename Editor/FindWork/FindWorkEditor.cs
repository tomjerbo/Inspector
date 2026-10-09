using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static FindWork.Work_Status;
using Object = UnityEngine.Object;

[CustomEditor(typeof(FindWork))]
public class FindWorkEditor : Editor {

    const BindingFlags binding_flags = BindingFlags.Default |
                                       BindingFlags.DeclaredOnly |
                                       BindingFlags.Public |
                                       BindingFlags.Static |
                                       BindingFlags.NonPublic |
                                       BindingFlags.Instance |
                                       BindingFlags.InvokeMethod;
    
    static readonly GUIContent work_in_progress_text = new ("In Progress");
    static readonly GUIContent work_completed_text   = new ("Completed");
    static readonly GUIContent work_all_text         = new ("Everything");
    static readonly StringBuilder string_builder = new (256);
    
    bool bg_is_default_color = true; // avoid a few deep calls into GUI to set/check color
    bool gui_enabled_state = true;   // avoid a few deep calls into GUI to set/check state
    FindWork find_work;

    void OnEnable() {
        find_work = target as FindWork;
        if (find_work == null) {
            return;
        }
        
        find_refs(ref find_work.references, find_work.get_work_type, find_work.transform);
    }

    public override void OnInspectorGUI() {
        if (find_work == null) {
            base.OnInspectorGUI();
            return;
        }

        // reset state
        bg_is_default_color  = true;
        gui_enabled_state    = true;
        color_index          = 0;
        
        
        draw_buttons(ref find_work);
        EditorGUILayout.Space(12);

        Type work_type = find_work.get_work_type;
        for (int idx = 0; idx < find_work.references.Count; idx++) {
            draw_work_fields(find_work.references[idx], work_type);
        }
    }

    void draw_buttons(ref FindWork work_ref) {
        int num_buttons      = 3;
        float button_height  = 28;
        
        // Rect type_selection         = EditorGUILayout.GetControlRect(false, type_height);
        Rect work_status_area       = EditorGUILayout.GetControlRect(false, button_height);
        Rect find_ref_and_type      = EditorGUILayout.GetControlRect(false, button_height);
        Rect[] work_status_buttons  = work_status_area.split_even_horizontal(num_buttons, Styles.spacing);
        Rect[] ref_and_type         = find_ref_and_type.split_horizontal_custom(Styles.spacing, 0.5f, 0.5f);
        
        draw_work_all(work_status_buttons[0]);
        draw_work_in_progress(work_status_buttons[1]);
        draw_work_completed(work_status_buttons[2]);
        draw_find_refs(ref_and_type[0]);
        draw_work_type_dropdown(ref_and_type[1], ref work_ref);
    }
    
    void draw_work_type_dropdown(Rect rect, ref FindWork work_ref) {
        FindWork.Work_Type selected = (FindWork.Work_Type)EditorGUI.Popup(rect, (int)work_ref.work_type, FindWork.work_type_names, Styles.popup_center);

        if (work_ref.work_type != selected) {
            if (has_no_data_changes(ref work_ref) || EditorUtility.DisplayDialog("Are you sure?", "Changing the work type will remove any existing data", "ok", "cancel"))
            {
                Undo.RecordObject(work_ref, "Changed type of work");
                work_ref.work_type = selected;
                find_refs(ref find_work.references, find_work.get_work_type, find_work.transform);
            }
        }
    }

    bool has_no_data_changes(ref FindWork work_ref) {
        foreach (FindWork.Work_Reference work in work_ref.references) {
            if (work.description.Length != 0 || work.work_status != in_progress) {
                return false;
            }
        }

        return true;
    }
    
    void draw_work_fields(FindWork.Work_Reference work_ref, Type work_type) {
        if (find_work.display_setting != all && find_work.display_setting != work_ref.work_status) {
            return;
        }
        
        float full_element_height       = 86;
        float padding_between_elements  = 8;
        bool is_marked_completed        = work_ref.work_status == completed;
        
        Rect rect_full              = EditorGUILayout.GetControlRect(false, full_element_height + padding_between_elements);
        rect_full                   = rect_full.trim_top(padding_between_elements);
        Rect[] vertical_group       = rect_full.split_vertical_custom(Styles.spacing, 0.22f, 0.28f, 0.5f);
        Rect[] progress_and_object  = vertical_group[1].split_horizontal_custom(Styles.spacing, 0.2f, 0.8f);
        
        
        Rect rect_name      = vertical_group[0];
        Rect rect_object    = progress_and_object[1];
        Rect rect_info      = vertical_group[2];
        Rect rect_progress  = progress_and_object[0];

        // if (work_ref.was_restored_from_cache) {
        //     EditorGUI.DrawRect(rect_full.reduce(-4), Color.yellowNice * 0.7f);
        //     EditorGUI.DrawRect(rect_full.reduce(-3), new Color(0.22f,0.22f,0.22f));
        // }
        // else if (work_ref.was_removed) {
        //     EditorGUI.DrawRect(rect_full.reduce(-4), Color.softRed * 0.8f);
        //     EditorGUI.DrawRect(rect_full.reduce(-3), new Color(0.22f,0.22f,0.22f));
        // }
        
        draw_highlight(ref rect_full, rect_name.height, is_marked_completed);
        draw_name(rect_name, ref work_ref);
        draw_progress(rect_progress, ref work_ref);

        // float extra_height = (rect_object.height + Styles.spacing) * Mathf.Max(work_ref.event_index, 0);
        // if (work_ref.field_type != Field_Type.array && work_ref.field_type != Field_Type.list) {
        // }
        //     extra_height = 320;
        // rect_info.y += extra_height;
        disable_gui_if(is_marked_completed);
        draw_description_text(rect_info, ref work_ref);
        draw_object_field(rect_object, work_type, ref work_ref);
        set_gui_on();
        
        // rect_full.height += extra_height;
        
        // EditorGUILayout.Space(extra_height);
    }

    void draw_highlight(ref Rect rect_full, float name_rect_height, bool is_marked_completed) {
        Rect highlight_rect      = rect_full.extend_left(8);
        highlight_rect.width     = 2;
        float highlight_spacing  = 2f;
        
        Color color = is_marked_completed ? Styles.green : Styles.blue;
        EditorGUI.DrawRect(highlight_rect.trim_top(name_rect_height + Styles.spacing + highlight_spacing), color * 0.35f);
        EditorGUI.DrawRect(highlight_rect.trim_bottom(rect_full.height - (name_rect_height + Styles.spacing) + highlight_spacing), color * 0.5f);
    }


    void draw_find_refs(Rect rect) {
        set_bg_color(Styles.grey);
        if (GUI.Button(rect, "Find Work")) {
            Undo.RecordObject(find_work, "Find work references");
            find_refs(ref find_work.references, find_work.get_work_type, find_work.transform);
        }
    }

    void draw_work_all(Rect rect) {
        set_bg_color(find_work.display_setting == all ? Styles.green : Styles.grey);
        if (GUI.Button(rect, work_all_text)) {
            Undo.RecordObject(find_work, "Display all work");
            find_work.display_setting = all;
        }
        set_bg_white();
    }

    void draw_work_in_progress(Rect rect) {
        set_bg_color(find_work.display_setting == in_progress ? Styles.blue : Styles.grey);
        if (GUI.Button(rect, work_in_progress_text)) {
            Undo.RecordObject(find_work, "Display in progress work");
            find_work.display_setting = in_progress;
        }
        set_bg_white();
    }

    void draw_work_completed(Rect rect) {
        set_bg_color(find_work.display_setting == completed ? Styles.blue : Styles.grey);
        if (GUI.Button(rect, work_completed_text)) {
            Undo.RecordObject(find_work, "Display completed work");
            find_work.display_setting = completed;
        }
        set_bg_white();
    }
    
    void draw_name(Rect rect, ref FindWork.Work_Reference work_ref) {
        string_builder.Clear();
        string_builder.Append(work_ref.on_component.GetType().Name);
        string_builder.Append(" -> ");
        string_builder.Append(work_ref.field_name);
        
        if (work_ref.field_type != FindWork.Field_Type.field) {
            string_builder.Append("[");
            string_builder.Append(work_ref.event_index);
            string_builder.Append("]");
        }
        
        GUI.contentColor = (work_ref.work_status == completed ? Styles.green : Styles.blue) * 0.6f;
        if (GUI.Button(rect, string_builder.ToString(), Styles.label_left)) {
            EditorGUIUtility.PingObject(work_ref.on_component);
        }
        
        GUI.contentColor = Styles.grey;
        GUI.Label(rect, work_ref.on_object.name, Styles.label_right);
        GUI.contentColor = Color.white;
    }

    void draw_description_text(Rect rect, ref FindWork.Work_Reference work_ref) {
        GUI.skin.textField.wordWrap = true;
        string description_text = EditorGUI.TextField(rect, work_ref.description);
        if (work_ref.description.Length != description_text.Length || string.CompareOrdinal(work_ref.description, description_text) != 0) {
            Undo.RecordObject(find_work, "Change work description");
            work_ref.description = description_text;
        }
        GUI.skin.textField.wordWrap = false;
    }

    void draw_object_field(Rect rect, Type object_type, ref FindWork.Work_Reference work_ref) {
        // SerializedObject ser_objs = new UnityEditor.SerializedObject(work_ref.on_component);
        // SerializedProperty prop = ser_objs.FindProperty(work_ref.field_name);
        // if (EditorGUI.PropertyField(rect, prop)) {
        //     ser_objs.ApplyModifiedProperties();
        //     prop.serializedObject.ApplyModifiedProperties();
        // }

        Object active_value = get_value(ref work_ref);
        Object result = EditorGUI.ObjectField(rect, active_value, object_type, false);
        if (active_value != result) {
            Undo.RecordObject(work_ref.on_component, "Changed work value");
            set_value(ref work_ref, result);
        }
    }

    Object get_value(ref FindWork.Work_Reference work_ref) {
        switch (work_ref.field_type) {
            case FindWork.Field_Type.field: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                return (Object)field.GetValue(work_ref.on_component);
            }
            
            case FindWork.Field_Type.array: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                Array value_array = (Array)field.GetValue(work_ref.on_component);
                return (Object)value_array.GetValue(work_ref.event_index);

            }
            
            case FindWork.Field_Type.list: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                IList value_list = (IList)field.GetValue(work_ref.on_component);
                return (Object)value_list[work_ref.event_index];
            }
            
            case FindWork.Field_Type.unity_event_target:
            case FindWork.Field_Type.unity_event_value: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                UnityEventBase unity_event_base = (UnityEventBase)field.GetValue(work_ref.on_component);
                
                // if it's an UnityEventBase then these fields will always exist!
                FieldInfo persistent_calls_field = typeof(UnityEventBase).GetField("m_PersistentCalls", binding_flags);
                object persistent_calls_value = persistent_calls_field.GetValue(unity_event_base); // PersistentCallGroup
                
                FieldInfo persistent_call_list_field = persistent_calls_value.GetType().GetField("m_Calls", binding_flags);
                IList persistent_calls_list_value = (IList)persistent_call_list_field.GetValue(persistent_calls_value); // List<PersistentCall>

                object element = persistent_calls_list_value[work_ref.event_index];
                Type persistent_call_type = element.GetType(); // PersistentCall
                
                if (work_ref.field_type == FindWork.Field_Type.unity_event_target) {
                    FieldInfo target_field = persistent_call_type.GetField("m_Target", binding_flags); // UnityEngine.Object
                    return (Object)target_field.GetValue(element);
                }

                if (work_ref.field_type == FindWork.Field_Type.unity_event_value) {
                    FieldInfo argument_cache_field = persistent_call_type.GetField("m_Arguments", binding_flags);
                    object argument_cache_value = argument_cache_field.GetValue(element);
                    Type argument_cache_type = argument_cache_value.GetType(); // ArgumentCache
                    
                    FieldInfo object_argument_field = argument_cache_type.GetField("m_ObjectArgument", binding_flags);
                    return (Object)object_argument_field.GetValue(argument_cache_value);
                }

                break;
            }
            
            default: throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    void set_value(ref FindWork.Work_Reference work_ref, Object value) {
        switch (work_ref.field_type) {
            case FindWork.Field_Type.field: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                field.SetValue(work_ref.on_component, value);
                break;
            }
            
            case FindWork.Field_Type.array: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                Array value_array = (Array)field.GetValue(work_ref.on_component);
                value_array.SetValue(value, work_ref.event_index);
                break;
            }
            
            case FindWork.Field_Type.list: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                IList value_list = (IList)field.GetValue(work_ref.on_component);
                value_list[work_ref.event_index] = value;
                break;
            }
            
            case FindWork.Field_Type.unity_event_target:
            case FindWork.Field_Type.unity_event_value: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                UnityEventBase unity_event_base = (UnityEventBase)field.GetValue(work_ref.on_component);
                
                // if it's an UnityEventBase then these fields will always exist!
                FieldInfo persistent_calls_field = typeof(UnityEventBase).GetField("m_PersistentCalls", binding_flags);
                object persistent_calls_value = persistent_calls_field.GetValue(unity_event_base); // PersistentCallGroup
                
                FieldInfo persistent_call_list_field = persistent_calls_value.GetType().GetField("m_Calls", binding_flags);
                IList persistent_calls_list_value = (IList)persistent_call_list_field.GetValue(persistent_calls_value); // List<PersistentCall>

                object element = persistent_calls_list_value[work_ref.event_index];
                Type persistent_call_type = element.GetType(); // PersistentCall
                
                if (work_ref.field_type == FindWork.Field_Type.unity_event_target) {
                    FieldInfo target_field = persistent_call_type.GetField("m_Target", binding_flags); // UnityEngine.Object
                    target_field.SetValue(element, value);
                }

                if (work_ref.field_type == FindWork.Field_Type.unity_event_value) {
                    FieldInfo argument_cache_field = persistent_call_type.GetField("m_Arguments", binding_flags);
                    object argument_cache_value = argument_cache_field.GetValue(element);
                    Type argument_cache_type = argument_cache_value.GetType(); // ArgumentCache
                    
                    FieldInfo object_argument_field = argument_cache_type.GetField("m_ObjectArgument", binding_flags);
                    object_argument_field.SetValue(argument_cache_value, value);
                }

                break;
            }
            
            default: throw new ArgumentOutOfRangeException();
        }
    }

    (FieldInfo field, object owner) get_field(ref FindWork.Work_Reference work_ref) {
        switch (work_ref.field_type) {
            case FindWork.Field_Type.field: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                return (field, work_ref.on_component);
            }
            
            case FindWork.Field_Type.array: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                return (field, work_ref.on_component);
                
                    Array value_array = (Array)field.GetValue(work_ref.on_component);
                    if (value_array != null) {
                        for (int idx = 0; idx < value_array.Length; idx++) {
                        }
                    }
                
                    
                break;
            }
            
            case FindWork.Field_Type.list: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                return (field, work_ref.on_component);
                
                IList value_list = (IList)field.GetValue(work_ref.on_component);
                if (value_list != null) {
                    for (int idx = 0; idx < value_list.Count; idx++) {
                    }
                }
                break;
            }
            
            case FindWork.Field_Type.unity_event_target:
            case FindWork.Field_Type.unity_event_value: {
                FieldInfo field = work_ref.on_component.GetType().GetField(work_ref.field_name, binding_flags);
                UnityEventBase unity_event_base = (UnityEventBase)field.GetValue(work_ref.on_component);
                
                // if it's an UnityEventBase then these fields will always exist!
                FieldInfo persistent_calls_field = typeof(UnityEventBase).GetField("m_PersistentCalls", binding_flags);
                object persistent_calls_value = persistent_calls_field.GetValue(unity_event_base); // PersistentCallGroup
                
                FieldInfo persistent_call_list_field = persistent_calls_value.GetType().GetField("m_Calls", binding_flags);
                IList persistent_calls_list_value = (IList)persistent_call_list_field.GetValue(persistent_calls_value); // List<PersistentCall>

                object element = persistent_calls_list_value[work_ref.event_index];
                Type persistent_call_type = element.GetType(); // PersistentCall
                
                if (work_ref.field_type == FindWork.Field_Type.unity_event_target) {
                    FieldInfo target_field = persistent_call_type.GetField("m_Target", binding_flags); // UnityEngine.Object
                    return (target_field, element);
                }

                if (work_ref.field_type == FindWork.Field_Type.unity_event_value) {
                    FieldInfo argument_cache_field = persistent_call_type.GetField("m_Arguments", binding_flags);
                    object argument_cache_value = argument_cache_field.GetValue(element);
                    Type argument_cache_type = argument_cache_value.GetType(); // ArgumentCache
                    
                    FieldInfo object_argument_field = argument_cache_type.GetField("m_ObjectArgument", binding_flags);
                    return (object_argument_field, argument_cache_value);
                }

                break;
            }
            
            default: throw new ArgumentOutOfRangeException();
        }

        return (null, null);
    }
    
    void draw_progress(Rect rect, ref FindWork.Work_Reference work_ref) {
        bool is_marked_complete = work_ref.work_status == completed;
        set_bg_color(is_marked_complete ? Styles.green : Styles.blue);
        if (GUI.Button(rect, is_marked_complete ? work_completed_text : work_in_progress_text)) {
            Undo.RecordObject(find_work, "Change work status");
            work_ref.work_status = is_marked_complete ? in_progress : completed;
        }
        set_bg_white();
    }
    
    static int color_index;
    void display_rects(params Rect[] rects) {
        float color_scale = 0.67f;
        for (int idx = 0; idx < rects.Length; idx++) {
            float r = Mathf.Abs(Mathf.Sin(color_index++ * color_scale));
            float g = Mathf.Abs(Mathf.Sin(color_index++ * color_scale));
            float b = Mathf.Abs(Mathf.Sin(color_index++ * color_scale));
            EditorGUI.DrawRect(rects[idx], new Color(r,g,b));
        }
    }
    
    struct Styles {
        public const float spacing = 3;
        public const float color_tint_adjustment = 2.0f;

        public static readonly Color blue   = new Color(104f/255f, 154/255f, 194f/255f) * color_tint_adjustment;
        public static readonly Color green  = new Color(144f/255f, 194/255f, 104f/255f) * color_tint_adjustment;
        public static readonly Color grey   = new Color(96/255f, 96/255f, 96/255f) * color_tint_adjustment;

        public static readonly GUIContent empty_content = GUIContent.none;
        public static readonly GUIStyle empty_style     = new ();
        
        public static readonly GUIStyle label = new (EditorStyles.label) {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            contentOffset = new Vector2(0, -1),
        };
        
        public static readonly GUIStyle label_center = new (label) {
            alignment = TextAnchor.MiddleCenter,
        };       
        
        public static readonly GUIStyle label_center_thin = new (label_center) {
            fontStyle = FontStyle.Normal,
            fontSize = 13,
            contentOffset = Vector2.zero,
        };
        
        public static readonly GUIStyle label_left = new (label) {
            alignment = TextAnchor.MiddleLeft,
        };
        
        public static readonly GUIStyle label_right = new (label) {
            alignment = TextAnchor.MiddleRight,
        };
        
        public static readonly GUIStyle popup_center = new (EditorStyles.popup) {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            contentOffset = new Vector2(0, -1),
            fixedHeight = 0,
            stretchHeight = true,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void set_bg_color(Color color) {
        GUI.backgroundColor = color;
        bg_is_default_color = false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void set_bg_color_if(Color color, bool value) {
        if (value) {
            GUI.backgroundColor = color;
            bg_is_default_color = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void set_bg_white() {
        if (bg_is_default_color == false) {
            GUI.backgroundColor = Color.white;
            bg_is_default_color = true;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void disable_gui_if(bool value) {
        if (value) {
            GUI.enabled = false;
            gui_enabled_state = false;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void set_gui_on() {
        if (gui_enabled_state == false) {
            GUI.enabled = true;
            gui_enabled_state = true;
        }
    }
    
    public void find_refs(ref List<FindWork.Work_Reference> references, Type work_type, Transform transform) {
        // gets marked as existing inside 'has_seen'
        for (int idx = references.Count - 1; idx >= 0; idx--) {
            if (references[idx].work_type != work_type) {
                references.RemoveAt(idx);
            }
            else {
                references[idx].exists = false;
            }
        }
        
        HashSet<Component> visited_comps     = new (128);
        HashSet<FieldInfo> visited_fields    = new (512);
        Queue<Transform> children_to_search  = new (128);
        children_to_search.Enqueue(transform);
        
        do {
            Transform scan_target = children_to_search.Dequeue();
            Type single_type = work_type;
            Type array_type = work_type.MakeArrayType();
            Type list_type = typeof(List<>).MakeGenericType(work_type);
            
            
            /*
             * TODO recurse through all types as well?
             * when should we stop?
             * ex. you are trying to find Epic_SO
             * class Cutscene_Event      // nested inside custom class/struct
             * {
             *      Epic_SO data;                     
             *      float go_next_timer = 1.5f;
             * }
             * class Game_Cutscene      // nested down 2 levels, not unlikely for something like a cutscene setup
             * {
             *      List<Cutscene_Event> events_in_cutscene;
             * }
             */
            
            scan_object(ref references, ref visited_comps, ref visited_fields, scan_target, single_type, array_type, list_type);
            append_children_without_component(children_to_search, scan_target, find_work.GetType());
        } while (children_to_search.Count > 0);
        
        for (int idx = references.Count - 1; idx >= 0; idx--) {
            if (references[idx].exists == false) {
                references.RemoveAt(idx);
            }
            // if (references[idx].exists == false) {
            //     references[idx].was_removed = true;
            //     references[idx].was_restored_from_cache = false;
            //     continue;
            // }
            //
            // if (references[idx].exists && references[idx].was_removed) {
            //     references[idx].was_removed = false;
            //     references[idx].was_restored_from_cache = true;
            // }
        }
    }
    
    void append_children_without_component(Queue<Transform> child_list, Transform parent, Type comp_type) {
        for (int idx = 0; idx < parent.childCount; idx++) {
            Transform child = parent.GetChild(idx);
            if (child.GetComponent(comp_type) == null) {
                child_list.Enqueue(child);
            }
        }
    }
    
    bool has_seen(ref List<FindWork.Work_Reference> references, Transform on_obj, Component on_comp, FieldInfo info, FindWork.Field_Type field_type, int event_index) {
        for (int idx = 0; idx < references.Count; idx++) {
            if (references[idx].on_component == on_comp 
                && string.CompareOrdinal(info.Name, references[idx].field_name) == 0
                && references[idx].on_object == on_obj
                && references[idx].event_index == event_index
                && references[idx].field_type == field_type) 
            {
                references[idx].exists = true;
                return true;
            }
        }

        return false;
    }

    // rename target
    void add_reference(
        ref List<FindWork.Work_Reference> references, 
        Transform target,
        Component comp,
        FieldInfo info,
        FindWork.Field_Type field_type,
        Type target_type,
        int event_index) 
    {
        if (has_seen(ref references, target, comp, info, field_type, event_index) == false) {
            references.Add(new FindWork.Work_Reference {
                on_object = target,
                on_component = comp,
                field_name = $"{info.Name}",
                field_type = field_type,
                event_index = event_index,
                exists = true,
                work_type = target_type, 
            });
        }
    }

    // rename target
    void scan_object(
        ref List<FindWork.Work_Reference> references, 
        ref HashSet<Component> visited_comps, 
        ref HashSet<FieldInfo> visited_fields, 
        Transform target, 
        Type object_type, 
        Type array_type, 
        Type list_type) 
    {
        Component[] all_components_on_target = target.GetComponents<Component>();
        foreach (Component comp in all_components_on_target) {
            if (visited_comps.Add(comp) == false) {
                continue;
            }
            
            Type comp_type = comp.GetType();
            
            // only care about fields
            FieldInfo[] fields_on_comp = comp_type.GetFields(binding_flags);
            foreach (FieldInfo field_info in fields_on_comp) {
                if (visited_fields.Add(field_info) == false) {
                    continue;
                }

                if (field_info.FieldType == object_type) {
                    add_reference(ref references, target, comp, field_info, FindWork.Field_Type.field, object_type, 0);
                }
                else if (field_info.FieldType == array_type) {
                    Array value_array = (Array)field_info.GetValue(comp);
                    if (value_array != null) {
                        for (int idx = 0; idx < value_array.Length; idx++) {
                            add_reference(ref references, target, comp, field_info, FindWork.Field_Type.array, object_type, idx);
                        }
                    }
                }
                else if (field_info.FieldType == list_type) {
                    IList value_list = (IList)field_info.GetValue(comp);
                    if (value_list != null) {
                        for (int idx = 0; idx < value_list.Count; idx++) {
                            add_reference(ref references, target, comp, field_info, FindWork.Field_Type.list, object_type, idx);
                        }
                    }
                }
                else if (field_info.FieldType == typeof(UnityEvent)) {
                    
                    // if it's an UnityEventBase then these fields will always exist!
                    UnityEventBase unity_event_base = (UnityEventBase)field_info.GetValue(comp);
                    
                    FieldInfo persistent_calls_field = typeof(UnityEventBase).GetField("m_PersistentCalls", binding_flags);
                    object persistent_calls_value = persistent_calls_field.GetValue(unity_event_base);
                    
                    FieldInfo calls_info = persistent_calls_value.GetType().GetField("m_Calls", binding_flags);
                    IList calls_value = (IList)calls_info.GetValue(persistent_calls_value);
                    
                    for (int idx = 0; idx < calls_value.Count; idx++) {
                        object element = calls_value[idx];
                        if (element == null) {
                            continue;
                        }
                        
                        Type element_type = element.GetType();
                        
                        FieldInfo target_info = element_type.GetField("m_Target", binding_flags);
                        Object target_value = (Object)target_info.GetValue(element);
                        if (target_value == null) {
                            continue;
                        }
                        
                        if (target_value.GetType() == object_type) {
                            add_reference(ref references, target, comp, field_info, FindWork.Field_Type.unity_event_target, object_type, idx);
                        }
                        else {
                            // look at targets method input type
                            FieldInfo target_method_name_field = element_type.GetField("m_MethodName", binding_flags);
                            string method_name = (string)target_method_name_field.GetValue(element);
                            if (string.IsNullOrEmpty(method_name) == false) {
                                MethodInfo target_method = target_value.GetType().GetMethod(method_name, binding_flags);
                                ParameterInfo[] parameters = target_method.GetParameters();
                                
                                if (parameters.Length == 1 && parameters[0].ParameterType == object_type) {
                                    add_reference(ref references, target, comp, field_info, FindWork.Field_Type.unity_event_value, object_type, idx);
                                }
                            }
                        }
                    }
                }
            }
            
        }
    }

    void log_all_fields(Type type, object target) {
        var fields = type.GetFields(binding_flags);
        foreach (FieldInfo f in fields) {
            var value = f.GetValue(target);
            string val = "";
            if (value != null) {
                val = $"--->{value}";
            }

            Debug.Log($"Field: {f.FieldType}->{f.Name}{val}");
        }
    }
}