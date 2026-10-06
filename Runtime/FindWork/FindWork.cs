using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Jerbo.Inspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using static FindWork.Field_Type;





public class FindWork : MonoBehaviour {
    public const BindingFlags binding_flags = BindingFlags.Default |
                                       BindingFlags.DeclaredOnly |
                                       BindingFlags.Public |
                                       BindingFlags.Static |
                                       BindingFlags.NonPublic |
                                       BindingFlags.Instance |
                                       BindingFlags.InvokeMethod;

    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
    
    // TODO not sure if being able to select different types to look for is nice to have or not
    [SerializeField] public ScriptableObject work_type;
    [SerializeField, ReadOnly] public string type_name; // TODO remove
    
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

        public Type work_type;
        public bool exists;
        public bool was_removed;
        public bool was_restored_from_cache;
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

    

    [ContextMenu("Name children")]
    void name_all_children() {
        rename_children(transform, 0);
    }



    [Button]
    public void find_refs() {
        /*
         * is it still there? found = false
         * found -> default
         * not found -> was_removed
         *
         * found + was_removed -> restored_from_cache
         * 
         */
        
        // gets marked as existing inside 'has_seen'
        for (int idx = references.Count - 1; idx >= 0; idx--) {
            if (references[idx].work_type != work_type.GetType()) {
                references.RemoveAt(idx);
            }
            else {
                references[idx].exists = false;
            }
        }
        
        children_to_search.Enqueue(transform);
        do {
            Transform scan_target = children_to_search.Dequeue();
            Type single_type = work_type.GetType();
            Type array_type = single_type.MakeArrayType();
            Type list_type = typeof(List<>).MakeGenericType(single_type);
            type_name = single_type.Name;
            
            
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
            
            scan_object(scan_target, single_type, array_type, list_type);
            append_children_without_component(children_to_search, scan_target, GetType());
        } while (children_to_search.Count > 0);
        
        for (int idx = 0; idx < references.Count; idx++) {
            if (references[idx].exists == false) {
                references[idx].was_removed = true;
                references[idx].was_restored_from_cache = false;
                continue;
            }
            
            if (references[idx].exists && references[idx].was_removed) {
                references[idx].was_removed = false;
                references[idx].was_restored_from_cache = true;
            }
        }
        
        visited_comps.Clear();
        visited_fields.Clear();
        children_to_search.Clear();
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
                references[idx].exists = true;
                return true;
            }
        }

        return false;
    }


    void add_reference(Transform target, Component comp, FieldInfo info, Field_Type field_type, Type target_type, int event_index) {
        if (has_seen(target, comp, info, field_type, event_index) == false) {
            references.Add(new Work_Reference() {
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
                    add_reference(target, comp, field_info, field, object_type, 0);
                }
                else if (field_info.FieldType == array_type) {
                    add_reference(target, comp, field_info, array, object_type,0);
                }
                else if (field_info.FieldType == list_type) {
                    add_reference(target, comp, field_info, list, object_type,0);
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
                        
                        
                        if (target_value.GetType() == object_type) {
                            add_reference(target, comp, field_info, unity_event_target, object_type, idx);
                        }
                        else {
                            // look at targets method input type
                            FieldInfo target_method_name_field = element_type.GetField("m_MethodName", binding_flags);
                            string method_name = (string)target_method_name_field.GetValue(element);
                            if (string.IsNullOrEmpty(method_name) == false) {
                                MethodInfo target_method = target_value.GetType().GetMethod(method_name, binding_flags);
                                ParameterInfo[] parameters = target_method.GetParameters();
                                
                                if (parameters.Length == 1 && parameters[0].ParameterType == object_type) {
                                    // Debug.Log($"{field_info.Name}: hashcode {target_method.GetHashCode()}, tokenid: {target_method.MetadataToken}, handle: {target_method}");
                                    add_reference(target, comp, field_info, unity_event_value, object_type, idx);
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