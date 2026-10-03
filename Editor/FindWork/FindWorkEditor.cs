using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using static FindWork.Work_Status;

[CustomEditor(typeof(FindWork))]
public class FindWorkEditor : Editor {

    static readonly GUIContent work_in_progress_text = new ("In Progress");
    static readonly GUIContent work_completed_text   = new ("Completed");
    static readonly GUIContent work_all_text         = new ("Everything");
    static readonly GUIContent find_references_text  = new ("Find References");


    bool bg_is_default_color = true; // avoid a few deep calls into GUI to set/check color
    bool gui_enabled_state = true;   // avoid a few deep calls into GUI to set/check state
    FindWork find_work;

    void OnEnable() {
        find_work = target as FindWork;
        if (find_work == null) {
            return;
        }

        find_work.find_refs();
    }
    
    
    public override void OnInspectorGUI() {
        
        base.OnInspectorGUI();
        
        if (find_work == null) {
            return;
        }

        // reset state
        bg_is_default_color = true;
        gui_enabled_state = true;
        color_index = 0;
        
        
        // TODO add support for undo
        draw_buttons();
        EditorGUILayout.Space(12);
        
        for (int idx = 0; idx < find_work.references.Count; idx++) {
            draw_work_fields(find_work.references[idx]);
        }
    }

    void draw_buttons() {
        int num_buttons = 3;
        float button_height = 28;
        
        Rect button_field_rect = EditorGUILayout.GetControlRect(false, button_height);
        Rect[] button_rects = button_field_rect.split_even_horizontal(num_buttons, Styles.spacing);
        Rect button_find_refs_rect = EditorGUILayout.GetControlRect(false, button_height);
        
        draw_find_refs(button_find_refs_rect);
        draw_work_all(button_rects[0]);
        draw_work_completed(button_rects[1]);
        draw_work_in_progress(button_rects[2]);
    }
    
    
    void draw_work_fields(FindWork.Work_Reference work_ref) {
        if (find_work.display_setting != all && find_work.display_setting != work_ref.work_status) {
            return;
        }
        
        float full_element_height = 86;
        float padding_between_elements = 8;
        bool is_marked_completed = work_ref.work_status == completed;
        
        Rect rect_full              = EditorGUILayout.GetControlRect(false, full_element_height + padding_between_elements);
        rect_full                   = rect_full.trim_top(padding_between_elements);
        Rect[] vertical_group       = rect_full.split_vertical_custom(Styles.spacing, 0.22f, 0.28f, 0.5f);
        Rect[] progress_and_object  = vertical_group[1].split_horizontal_custom(Styles.spacing, 0.2f, 0.8f);
        
        
        Rect rect_name      = vertical_group[0];
        Rect rect_object    = progress_and_object[1];
        Rect rect_info      = vertical_group[2];
        Rect rect_progress  = progress_and_object[0];

        Rect highlight = rect_full.extend_left(8);
        highlight.width = 2;
        EditorGUI.DrawRect(highlight.trim_top(rect_name.height + Styles.spacing + 2), (is_marked_completed ? Styles.green : Styles.blue) * 0.35f);
        EditorGUI.DrawRect(highlight.trim_bottom(rect_full.height - (rect_name.height + Styles.spacing) + 2), (is_marked_completed ? Styles.green : Styles.blue) * 0.5f);
        
        // TODO separate obj and comp+field name, obj in grey from right side
        draw_name(rect_name, ref work_ref);
        draw_progress(rect_progress, ref work_ref);
        
        disable_gui_if(is_marked_completed);
        draw_info_text(rect_info, ref work_ref);
        draw_object_field(rect_object, find_work.work_type_object, ref work_ref);
        set_gui_on();
    }


    void draw_find_refs(Rect rect) {
        set_bg_color(Styles.grey);
        if (GUI.Button(rect, find_references_text)) {
            find_work.find_refs();
        }
    }

    void draw_work_all(Rect rect) {
        set_bg_color(find_work.display_setting == all ? Styles.green : Styles.grey);
        if (GUI.Button(rect, work_all_text)) {
            find_work.display_setting = all;
        }
        set_bg_white();
    }

    void draw_work_in_progress(Rect rect) {
        set_bg_color(find_work.display_setting == in_progress ? Styles.blue : Styles.grey);
        set_bg_color_if(Styles.blue, find_work.display_setting == in_progress);
        set_bg_color_if(Styles.grey, find_work.display_setting != in_progress);
        if (GUI.Button(rect, work_in_progress_text)) {
            find_work.display_setting = in_progress;
        }
        set_bg_white();
    }

    void draw_work_completed(Rect rect) {
        set_bg_color(find_work.display_setting == completed ? Styles.green : Styles.grey);
        if (GUI.Button(rect, work_completed_text)) {
            find_work.display_setting = completed;
        }
        set_bg_white();
    }
    
    void draw_name(Rect rect, ref FindWork.Work_Reference work_ref) {
        if (GUI.Button(rect, $"{work_ref.on_object.name} -> {work_ref.on_component.GetType().Name}.{work_ref.field_name}", Styles.label_left)) {
            EditorGUIUtility.PingObject(work_ref.on_component);
        }
    }

    void draw_info_text(Rect rect, ref FindWork.Work_Reference work_ref) {
        GUI.skin.textField.wordWrap = true;
        work_ref.description = EditorGUI.TextField(rect, work_ref.description);
        GUI.skin.textField.wordWrap = false;
    }

    void draw_object_field(Rect rect, Type object_type, ref FindWork.Work_Reference work_ref) {
        // TODO find and set value
        EditorGUI.ObjectField(rect, null, object_type, false);
    }
    
    void draw_progress(Rect rect, ref FindWork.Work_Reference work_ref) {
        bool is_marked_complete = work_ref.work_status == completed;
        set_bg_color_if(Styles.green, is_marked_complete);
        set_bg_color_if(Styles.blue, is_marked_complete == false);
        if (GUI.Button(rect, is_marked_complete ? work_completed_text : work_in_progress_text)) {
            work_ref.work_status = is_marked_complete ? in_progress : completed;
        }
        set_bg_white();
        
    }

    static int color_index;
    void display_rects(Rect[] rects) {
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
    
    
}