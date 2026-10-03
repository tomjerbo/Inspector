using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Jerbo.Inspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using static FindWork.Field_Type;

public class Work<T> : MonoBehaviour {
    [SerializeField] public Object type_to_find;
    public Type work_type_object => typeof(T);
    public Type work_type_array => typeof(T[]);
    public Type work_type_list => typeof(List<T>);
}

public class FindWork : Work<ScriptableObject> {
    const BindingFlags binding_flags = BindingFlags.Default |
                                       BindingFlags.DeclaredOnly |
                                       BindingFlags.Public |
                                       BindingFlags.Static |
                                       BindingFlags.NonPublic |
                                       BindingFlags.Instance |
                                       BindingFlags.InvokeMethod;

    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
    
    [SerializeField] public Work_Status display_setting;
    [SerializeField] public List<Work_Reference> references = new (32);
    
    
    
    HashSet<Component> visited_comps = new ();
    HashSet<FieldInfo> visited_fields = new ();
    Queue<Transform> children_to_search = new (128);

    [Serializable]
    public class Work_Reference {
        public Transform on_object;
        public Component on_component;
        public Field_Type field_type;
        public string field_name;
        public int event_index;
        
        public Work_Status work_status;
        public string description;
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

    

    [Button]
    void name_all_children() {
        rename_children(transform, 0);
    }





    [Button]
    public void find_refs() {
        visited_comps.Clear();
        visited_fields.Clear();
        children_to_search.Clear();

        
        // add found into new list, compare with old, remove deleted fields
        
        children_to_search.Enqueue(transform);
        do {
            Transform scan_target = children_to_search.Dequeue();
            scan_object(scan_target, work_type_object, work_type_array, work_type_list);
            append_children_without_component(children_to_search, scan_target, GetType());
        } while (children_to_search.Count > 0);
    }
    
    void rename_children(Transform parent, int char_idx) {
        for (int idx = 0; idx < parent.childCount; idx++) {
            parent.GetChild(idx).name = $"Child: {alphabet[char_idx]}_{idx:00}";
            rename_children(parent.GetChild(idx), char_idx + 1);
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


    bool has_seen(Transform on_obj, Component on_comp, FieldInfo info, Field_Type field_type, int event_index) {
        for (int idx = 0; idx < references.Count; idx++) {
            if (references[idx].on_component == on_comp 
                && string.CompareOrdinal(info.Name, references[idx].field_name) == 0
                && references[idx].on_object == on_obj
                && references[idx].event_index == event_index
                && references[idx].field_type == field_type) 
            {
                return true;
            }
        }

        return false;
    }


    void add_reference(Transform target, Component comp, FieldInfo info, Field_Type field_type, int event_index) {
        if (has_seen(target, comp, info, field_type, event_index) == false) {
            references.Add(new Work_Reference() {
                on_object = target,
                on_component = comp,
                field_name = $"{info.Name}",
                field_type = field_type,
                event_index = event_index
            });
        }
    }

    void scan_object(Transform target, Type object_type, Type array_type, Type list_type) {
        Component[] all_components_on_target = target.GetComponents<Component>();
        foreach (Component comp in all_components_on_target) {
            if (visited_comps.Add(comp) == false) {
                continue;
            }
            
            Type comp_type = comp.GetType();

            
            // Fields --- only care about fields
            FieldInfo[] fields_on_comp = comp_type.GetFields(binding_flags);
            foreach (FieldInfo field_info in fields_on_comp) {
                if (visited_fields.Add(field_info) == false) {
                    continue;
                }

                if (field_info.FieldType == object_type) {
                    add_reference(target, comp, field_info, field, 0);
                }
                else if (field_info.FieldType == array_type) {
                    add_reference(target, comp, field_info, array, 0);
                }
                else if (field_info.FieldType == list_type) {
                    add_reference(target, comp, field_info, list, 0);
                }
                else if (field_info.FieldType == typeof(UnityEvent)) {
                    
                    UnityEventBase unity_event_base = (UnityEventBase)field_info.GetValue(comp);
                    
                    // if it's an UnityEventBase then these fields will always exist!
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
                        
                        
                        if (target_value.GetType() == work_type_object) {
                            add_reference(target, comp, field_info, unity_event_target, idx);
                        }
                        else {
                            // PersistenCall always have a valid ArgumentCache -> m_Arguments object
                            FieldInfo argument_cache_field = element_type.GetField("m_Arguments", binding_flags);
                            object argument_cache = argument_cache_field.GetValue(element);
                            if (argument_cache == null) {
                                continue;
                            }
                            
                            FieldInfo arg_value_field = argument_cache_field.FieldType.GetField("m_ObjectArgument", binding_flags);
                            if (arg_value_field == null) {
                                continue;
                            }
                            Object arg_value = (Object)arg_value_field.GetValue(argument_cache);
                            
                            if (arg_value != null && arg_value.GetType() == work_type_object) {
                                add_reference(target, comp, field_info, unity_event_value, idx);
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