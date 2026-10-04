using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using static FindWork.Work_Status;

[CustomEditor(typeof(FindWork))]
public class FindWorkEditor : Editor {

    static readonly GUIContent work_in_progress_text = new ("In Progress");
    static readonly GUIContent work_completed_text   = new ("Completed");
    static readonly GUIContent work_all_text         = new ("Everything");
    static readonly StringBuilder string_builder = new (256);
    GUIContent find_references_text;
    
    bool bg_is_default_color = true; // avoid a few deep calls into GUI to set/check color
    bool gui_enabled_state = true;   // avoid a few deep calls into GUI to set/check state
    FindWork find_work;

    void OnEnable() {
        find_work = target as FindWork;
        if (find_work == null) {
            return;
        }
        
        // TODO remove
        find_work.find_refs();
        update_type_name();
    }

    void update_type_name() {
        string_builder.Clear();
        string_builder.Append("Find ");
        string_builder.Append(find_work.type_name);
        find_references_text = new GUIContent(string_builder.ToString());
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

        Type work_type = find_work.work_type.GetType();
        for (int idx = 0; idx < find_work.references.Count; idx++) {
            draw_work_fields(find_work.references[idx], work_type);
        }
    }

    void draw_buttons() {
        int num_buttons = 3;
        float button_height = 28;
        
        Rect work_status_area       = EditorGUILayout.GetControlRect(false, button_height);
        Rect find_ref_button        = EditorGUILayout.GetControlRect(false, button_height);
        Rect[] work_status_buttons  = work_status_area.split_even_horizontal(num_buttons, Styles.spacing);
        
        draw_find_refs(find_ref_button);
        draw_work_all(work_status_buttons[0]);
        draw_work_completed(work_status_buttons[1]);
        draw_work_in_progress(work_status_buttons[2]);
    }
    
    
    void draw_work_fields(FindWork.Work_Reference work_ref, Type work_type) {
        // work_ref.was_removed || 
        if ((find_work.display_setting != all && find_work.display_setting != work_ref.work_status)) {
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

        if (work_ref.was_restored_from_cache) {
            EditorGUI.DrawRect(rect_full.reduce(-4), Color.yellowNice * 0.7f);
            EditorGUI.DrawRect(rect_full.reduce(-3), new Color(0.22f,0.22f,0.22f));
        }
        else if (work_ref.was_removed) {
            EditorGUI.DrawRect(rect_full.reduce(-4), Color.softRed * 0.8f);
            EditorGUI.DrawRect(rect_full.reduce(-3), new Color(0.22f,0.22f,0.22f));
        }
        
        draw_highlight(ref rect_full, rect_name.height, is_marked_completed);
        draw_name(rect_name, ref work_ref);
        draw_progress(rect_progress, ref work_ref);
        
        disable_gui_if(is_marked_completed);
        draw_info_text(rect_info, ref work_ref);
        draw_object_field(rect_object, work_type, ref work_ref);
        set_gui_on();

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
        if (GUI.Button(rect, find_references_text)) {
            find_work.find_refs();
            update_type_name();
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
        
        string_builder.Clear();
        string_builder.Append(work_ref.on_component.GetType().Name);
        string_builder.Append(" -> ");
        string_builder.Append(work_ref.field_name);
        
        if (work_ref.field_type == FindWork.Field_Type.unity_event_target ||
            work_ref.field_type == FindWork.Field_Type.unity_event_value) {
            string_builder.Append(" (");
            string_builder.Append(work_ref.event_index);
            string_builder.Append(")");
        }
        
        GUI.contentColor = (work_ref.work_status == completed ? Styles.green : Styles.blue) * 0.6f;
        if (GUI.Button(rect, string_builder.ToString(), Styles.label_left)) {
            EditorGUIUtility.PingObject(work_ref.on_component);
        }
        
        GUI.contentColor = Styles.grey;
        GUI.Label(rect, work_ref.on_object.name, Styles.label_right);
        GUI.contentColor = Color.white;
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
        set_bg_color(is_marked_complete ? Styles.green : Styles.blue);
        if (GUI.Button(rect, is_marked_complete ? work_completed_text : work_in_progress_text)) {
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