using UnityEditor;
using UnityEngine;

public class AnchorTools : EditorWindow
{
    [MenuItem("Tools/UI Anchor Tools")]
    public static void ShowWindow()
    {
        GetWindow<AnchorTools>("Anchor Tools");
    }

    private void OnGUI()
    {
        GUILayout.Label("Anchor Utilities", EditorStyles.boldLabel);

        if (GUILayout.Button("Anchor to Corners (Preserve Position)"))
        {
            AnchorSelectedUIElements();
        }
    }

    private void AnchorSelectedUIElements()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform t = go.GetComponent<RectTransform>();
            if (t == null || t.parent == null) continue;

            Undo.RecordObject(t, "Anchor UI Element");

            RectTransform parent = t.parent as RectTransform;
            Vector2 parentSize = parent.rect.size;

            Vector2 newAnchorsMin = new Vector2(
                t.anchorMin.x + t.offsetMin.x / parentSize.x,
                t.anchorMin.y + t.offsetMin.y / parentSize.y);

            Vector2 newAnchorsMax = new Vector2(
                t.anchorMax.x + t.offsetMax.x / parentSize.x,
                t.anchorMax.y + t.offsetMax.y / parentSize.y);

            t.anchorMin = newAnchorsMin;
            t.anchorMax = newAnchorsMax;
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;

            EditorUtility.SetDirty(t);
        }
    }
}