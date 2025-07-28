using UnityEditor;
using UnityEngine;

namespace Jerbo.Inspector
{
    [CustomPropertyDrawer(typeof(NoteAttribute))]
    public class NoteAttributeDrawer : DecoratorDrawer
    {
        GUIStyle style = new (EditorStyles.helpBox);
        public override void OnGUI(Rect position)
        {
            style ??= new GUIStyle(EditorStyles.helpBox);
            
            NoteAttribute noteAttribute = attribute as NoteAttribute;
            if (noteAttribute == null)
                return;
            
            GUIContent content = new (noteAttribute.noteText);
            
            switch (noteAttribute.noteType)
            {
                case NoteType.Info:
                    content = EditorGUIUtility.IconContent("console.infoicon", noteAttribute.noteText);
                    break;
                case NoteType.Warning:
                    content = EditorGUIUtility.IconContent("console.warnicon", noteAttribute.noteText);
                    break;
            }       
            content.text = noteAttribute.noteText;
            style.richText = true;
            GUI.Box(position, content, style);      
        }
    
        public override float GetHeight()
        {
            NoteAttribute helpBoxAttribute = attribute as NoteAttribute;
            if (helpBoxAttribute == null) return base.GetHeight();
            if (style == null) style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            if (style == null) return base.GetHeight();
            return Mathf.Max(EditorGUIUtility.singleLineHeight, style.CalcHeight(new GUIContent(helpBoxAttribute.noteText), EditorGUIUtility.currentViewWidth) + 10);
        }
    }
}
