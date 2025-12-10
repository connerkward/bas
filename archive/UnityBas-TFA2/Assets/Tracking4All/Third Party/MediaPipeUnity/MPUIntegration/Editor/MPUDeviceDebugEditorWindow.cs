using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Editor Window to display detected devices which can be used with mpu.
/// </summary>
public class MPUDeviceDebugEditorWindow : EditorWindow
{
    [MenuItem("Tracking4All/UMP Device Debugging")]
    public static void ShowExample()
    {
        MPUDeviceDebugEditorWindow wnd = GetWindow<MPUDeviceDebugEditorWindow>();
        wnd.titleContent = new GUIContent("UMP Device Debugging");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Detected Available Devices:", EditorStyles.boldLabel);
        var cameras = WebCamTexture.devices;
        for (int i = 0; i < cameras.Length; ++i)
        {
            EditorGUILayout.BeginFoldoutHeaderGroup(true,cameras[i].name, EditorStyles.foldoutHeader);
            EditorGUILayout.LabelField("Index=" + i, EditorStyles.label);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        EditorGUILayout.LabelField("The index values may not be accurate!", EditorStyles.label);
    }

}