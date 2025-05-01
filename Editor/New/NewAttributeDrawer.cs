using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jerbo.Inspector
{
    [CustomPropertyDrawer(typeof(NewAttribute))]
    public class NewAttributeDrawer : PropertyDrawer
    {
        const string DEFAULT_SAVE_PATH = "Assets/Scriptable Objects"; // Change to your local folder for scriptable objects
        const string BUTTON_TEXT = "New";
        const string DEFAULT_ICON_PATH = "Default New Asset Icon";
        const bool ONLY_SHOW_BUTTON_ON_NULL_VALUE = false;
        
        readonly Type assetType = typeof(ScriptableObject);
        SerializedProperty property;

        public override void OnGUI(Rect position, SerializedProperty inProperty, GUIContent label)
        {
            NewAttribute newAttributeAttribute = attribute as NewAttribute;
            if (newAttributeAttribute == null) return;

            
            // New attribute is put on something that isn't a ScriptableObject or it should hide button when value isn't null
            if (assetType.IsAssignableFrom(fieldInfo.FieldType) == false || (ONLY_SHOW_BUTTON_ON_NULL_VALUE && inProperty.objectReferenceValue != null))
            {
                // EditorGUILayout.PropertyField(inProperty, label);
                EditorGUI.PropertyField(position, inProperty, label);
                return;
            }
            
            // SerializedProperty saved so we can access it when creating a new asset
            property = inProperty;
            EditorGUI.BeginProperty(position, label, property);
        
            
            // Setup general variables
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float propertyHeight = EditorGUIUtility.singleLineHeight;
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = position.width - labelWidth;
        
            
            // Calculate size percentage per element
            float createAssetButtonWidthPercentage = newAttributeAttribute.buttonWidthPercentage;
            float objectFieldWidthPercentage = 1f - createAssetButtonWidthPercentage;
            
            
            // Label
            Rect labelRect = new (position.position, new Vector2(labelWidth, propertyHeight));
            EditorGUI.LabelField(labelRect, label);

            
            // Object field
            Vector2 objectFieldPos = new (labelRect.xMax + spacing, position.y);
            Rect objectFieldRect = new (objectFieldPos, new Vector2(fieldWidth * objectFieldWidthPercentage - spacing * 0.5f, propertyHeight));
            Object selectedObject = EditorGUI.ObjectField(objectFieldRect, property.objectReferenceValue, fieldInfo.FieldType, false);
            
            if (selectedObject != property.objectReferenceValue)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Setting NewAttribute Field Value");
                property.objectReferenceValue = selectedObject;
                property.serializedObject.ApplyModifiedProperties();
            }
            
            
            // Create asset button
            Vector2 buttonPos = new (objectFieldRect.xMax + spacing, position.y);
            Vector2 buttonSize = new(fieldWidth * createAssetButtonWidthPercentage - spacing - 1f, propertyHeight);
            Rect buttonRect = new (buttonPos, buttonSize);
            
            if (GUI.Button(buttonRect, BUTTON_TEXT))
            {
                EditorApplication.update += SaveWindow;
                EditorApplication.QueuePlayerLoopUpdate();
            }


            if (buttonRect.width < 64)
            {
                EditorGUI.EndProperty();
                return;
            }
            
            // Asset Icon Thumbnail
            Texture typeIcon;
            if (selectedObject != null)
            {
                typeIcon = EditorGUIUtility.GetIconForObject(selectedObject);
            }
            else
            {
                Object tempObject = ScriptableObject.CreateInstance(fieldInfo.FieldType);
                typeIcon = EditorGUIUtility.GetIconForObject(tempObject);
                Object.DestroyImmediate(tempObject);
            }
            typeIcon ??= Resources.Load<Texture2D>(DEFAULT_ICON_PATH);

            Vector2 iconPosition = buttonPos + new Vector2(buttonRect.width / 2 - 32, 1f);
            Rect iconRect = new (iconPosition, new Vector2(16,16));
            GUI.DrawTexture(iconRect, typeIcon, ScaleMode.ScaleToFit, true);
            
            EditorGUI.EndProperty();
        }

        
        void SaveWindow()
        {
            EditorApplication.update -= SaveWindow;

            string pathToOpen = DEFAULT_SAVE_PATH;
            if (property.objectReferenceValue != null)
            {
                pathToOpen = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                pathToOpen = pathToOpen.Replace(pathToOpen.Split('/')[^1], "");
            }
            
            string path = EditorUtility.SaveFilePanelInProject(
                $"Create new {fieldInfo.FieldType} asset.",
                $"New {fieldInfo.FieldType}",
                "asset",
                $"Choose save location.",
                pathToOpen);
            
            if (string.IsNullOrEmpty(path)) return;
            
            ScriptableObject asset = ScriptableObject.CreateInstance(fieldInfo.FieldType);
            AssetDatabase.CreateAsset(asset, path);
                
            property.objectReferenceValue = asset;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}

