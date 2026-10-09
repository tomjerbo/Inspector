using System;
using System.Collections.Generic;
using UnityEngine;


public class FindWork : MonoBehaviour {
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
    
    // TODO not sure if being able to select different types to look for is nice to have or not
    [SerializeField] public Work_Type work_type;
    [SerializeField] public Work_Status display_setting;
    [SerializeField] public List<Work_Reference> references = new (32);

    [Serializable]
    public class Work_Reference {
        public Transform on_object;
        public Component on_component;
        public Field_Type field_type;
        public string field_name = string.Empty;
        public int event_index;

        public Type work_type;
        public bool exists;
        // public bool was_removed;
        // public bool was_restored_from_cache;
        public Work_Status work_status;
        public string description = string.Empty;
    }
    
    public enum Work_Status {
        in_progress,
        completed,
        all
    }
    
    public enum Field_Type {
        field,
        array,
        list,
        unity_event_target,
        unity_event_value,
    }

    public static readonly string[] work_type_names = Enum.GetNames(typeof(Work_Type));
    public Type get_work_type => types_of_work_refs[(int)work_type];

    static readonly Type[] types_of_work_refs = {
        typeof(TestSO),
        typeof(ScriptableObject),
    };
    
    public enum Work_Type {
        TestSO,
        ScriptableObject,
    }
    
    [ContextMenu("Name children")]
    void name_all_children() {
        rename_children(transform, 0);
    }

    void rename_children(Transform parent, int char_idx) {
        for (int idx = 0; idx < parent.childCount; idx++) {
            parent.GetChild(idx).name = $"Child: {alphabet[char_idx]}_{idx:00}";
            rename_children(parent.GetChild(idx), char_idx + 1);
        }
    }
    
}