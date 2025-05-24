using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[InitializeOnLoad]
public static class ColoredHierarchyHighlighter
{
    private static readonly Dictionary<Type, Color> componentColors = new Dictionary<Type, Color>()
    {
        // { typeof(Rigidbody), new Color(0.4f, 0.8f, 1f, 0.3f) },       // Light blue
        // { typeof(Collider), new Color(1f, 0.6f, 0.4f, 0.3f) },        // Soft orange
        // { typeof(AudioSource), new Color(0.6f, 1f, 0.6f, 0.3f) },     // Pale green
        // { typeof(Light), new Color(1f, 1f, 0.4f, 0.3f) },             // Soft yellow
        // { typeof(Camera), new Color(1f, 0.4f, 0.8f, 0.3f) },          // Pink
        // { typeof(Animator), new Color(0.8f, 0.4f, 1f, 0.3f) },        // Purple
        // Add more as needed...
    };

    private static readonly HashSet<Type> uiComponentTypes = new HashSet<Type>()
    {
        typeof(Canvas),
        typeof(CanvasRenderer),
        typeof(Graphic),
        typeof(Image),
        typeof(Text),
        typeof(Button),
        typeof(RectTransform)
    };

    static ColoredHierarchyHighlighter()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;

            Type type = comp.GetType();

            // Ignore Transform and known UI components
            if (type == typeof(Transform) || IsUIComponent(type))
                continue;

            // If component matches one of our target types, draw color and break
            if (componentColors.TryGetValue(type, out Color color))
            {
                EditorGUI.DrawRect(selectionRect, color);
                break;
            }
        }
    }

    private static bool IsUIComponent(Type type)
    {
        foreach (var uiType in uiComponentTypes)
        {
            if (uiType.IsAssignableFrom(type))
                return true;
        }
        return false;
    }
}
