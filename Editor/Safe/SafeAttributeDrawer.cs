using UnityEditor;
using UnityEngine;

namespace Jerbo.Inspector
{
    [CustomPropertyDrawer(typeof(SafeAttribute))]
    public class SafeAttributeDrawer : PropertyDrawer {
        bool canEdit;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect toggleRect = new Rect(position.position + new Vector2(EditorGUIUtility.labelWidth - 32, 0),
                new Vector2(32, 12));
            
            canEdit = EditorGUI.Toggle(toggleRect, canEdit);
            GUI.enabled = canEdit;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
