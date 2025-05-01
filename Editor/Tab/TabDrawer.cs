using UnityEditor;
using UnityEngine;

namespace Jerbo.Inspector {
    [CustomPropertyDrawer(typeof(Tab))]
    public class TabDrawer : DecoratorDrawer {
        readonly Color editorTintColor = new Color32(255,255,255, 56);
        
        const int TAB_HEIGHT = 18;
        const int TAB_THICKNESS = 2;
        const int TAB_X_OFFSET = 10;
        
        const int TAB_PADDING_ABOVE = 24;
        const int TAB_PADDING_BELOW = 8;

        const int TEXT_PADDING_WIDTH = 3;
        const float HEADER_X_OFFSET = 2;
        
        readonly GUIStyle headerStyle = new () {
            normal = new GUIStyleState() { textColor = new Color32(255,255,255, 200) },
            padding = new RectOffset(TEXT_PADDING_WIDTH, TEXT_PADDING_WIDTH, 0, 0),
            alignment = TextAnchor.MiddleLeft,
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };

        public override float GetHeight() {
            return TAB_HEIGHT + TAB_PADDING_ABOVE + TAB_PADDING_BELOW;
        }

        public override void OnGUI(Rect rect) {
            Tab tabProperty = (Tab)attribute;
            if (GUI.enabled == false) GUI.enabled = true;

            // Variables
            GUIContent titleContent = new (tabProperty.title);
            float titleWidth = headerStyle.CalcSize(titleContent).x;
            float yPos = rect.position.y + TAB_PADDING_ABOVE;
            float xPos = rect.position.x - TAB_X_OFFSET;
            float fullWidth = rect.width;


            // Tab Icon
            Vector2 rectPos = new (xPos - TEXT_PADDING_WIDTH, yPos);
            Rect tabVertRect = new (rectPos.x, rectPos.y, TAB_THICKNESS, TAB_HEIGHT);
            Rect tabHorRect = new (rectPos.x, tabVertRect.yMax, TEXT_PADDING_WIDTH + TEXT_PADDING_WIDTH + titleWidth, TAB_THICKNESS);
            
            EditorGUI.DrawRect(tabVertRect, tabProperty.color);
            EditorGUI.DrawRect(tabHorRect, tabProperty.color);
            

            // Title
            Rect textTitleRect = new (xPos, yPos, fullWidth - HEADER_X_OFFSET, TAB_HEIGHT);
            EditorGUI.LabelField(textTitleRect, titleContent, headerStyle);


            // Accent stripe
            Rect slimStripe = new (tabHorRect);
            slimStripe.x += tabHorRect.width + TEXT_PADDING_WIDTH;
            slimStripe.width = fullWidth - titleWidth + TEXT_PADDING_WIDTH;
            EditorGUI.DrawRect(slimStripe, tabProperty.color * editorTintColor);
        }
    }
}
