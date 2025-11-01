using UnityEditor;
using UnityEngine;

namespace Jerbo.Inspector
{
    [CustomPropertyDrawer(typeof(SafeAttribute))]
    public class SafeAttributeDrawer : PropertyDrawer {
        bool canEdit;
        bool hasLoadedTextures;
        Texture2D lockedIcon;
        Texture2D unlockedIcon;
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (hasLoadedTextures == false) {
                lockedIcon = Resources.Load<Texture2D>("Locked");
                unlockedIcon = Resources.Load<Texture2D>("Open");
            }
            
            Rect toggleRect = new Rect(position.position + new Vector2(EditorGUIUtility.labelWidth - 32, 0), new Vector2(32, 18));
            
            if (GUI.Button(toggleRect, canEdit ? unlockedIcon : lockedIcon)) {
                canEdit = !canEdit;
            }
            GUI.enabled = canEdit;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
