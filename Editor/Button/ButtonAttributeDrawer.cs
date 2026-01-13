using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
Object = UnityEngine.Object;

namespace Jerbo.Inspector
{
    [CustomEditor(typeof(Object), true)]
    public class ButtonAttributeDrawer : Editor
    {
        BindingFlags BINDING_FLAGS = BindingFlags.Default | 
                                     BindingFlags.Instance | 
                                     BindingFlags.Public |
                                     BindingFlags.NonPublic | 
                                     BindingFlags.Static;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Object buttonTarget = (Object)target;
            if (buttonTarget == null) return;

            MethodInfo[] methods = buttonTarget.GetType().GetMethods(BINDING_FLAGS);
            
            foreach (MethodInfo method in methods)
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
                if (buttonAttribute == null) continue;
                
                string displayText = buttonAttribute.displayText;
                if (string.IsNullOrEmpty(displayText)) displayText = method.Name;

                GUILayout.Space(4);
                if (GUILayout.Button(displayText, GUILayout.Height((float)buttonAttribute.buttonSize)))
                {
                    method.Invoke(buttonTarget, null);
                }
            }
        }
    }
}