using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TotalWinPreview))]
public sealed class TotalWinPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var preview = (TotalWinPreview)target;

        using (new EditorGUI.DisabledScope(!Application.isPlaying || !preview.isActiveAndEnabled))
        {
            using (new EditorGUI.DisabledScope(preview.IsOpen))
            {
                if (GUILayout.Button("Preview Total Win"))
                    preview.OpenPreview();
            }

            using (new EditorGUI.DisabledScope(!preview.IsOpen))
            {
                if (GUILayout.Button("Close Preview"))
                    preview.ClosePreview();
            }
        }
    }
}
