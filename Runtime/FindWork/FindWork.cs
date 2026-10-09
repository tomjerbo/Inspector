using System;
using System.Collections.Generic;
using UnityEngine;


public class FindWork : MonoBehaviour {
    [SerializeField] public Work_Type work_type;
    [SerializeField] public Work_Status display_setting;
    [SerializeField] public List<Work_Reference> references = new (32);

    [Serializable]
    public class Work_Reference {
        public Component on_component;
        public Field_Type field_type;
        public string field_name = string.Empty;
        public int event_index;

        public Type work_type;
        public bool exists;
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

    // add - typeof(YourScriptableObjectTypes) for each type of work
    static readonly Type[] types_of_work_refs = {
        typeof(ScriptableObject),
    };
    
    // add - 'YourScriptableObjectTypes' for each type of work
    public enum Work_Type {
        ScriptableObject,
    }
}