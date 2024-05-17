using UnityEngine;
using UnityEditor;

public class CopyGameObjectPath : EditorWindow
{
    [MenuItem("Tools/Copy GameObject Path")]
    public static void ShowWindow()
    {
        GetWindow<CopyGameObjectPath>("Copy GameObject Path");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select a GameObject in the Hierarchy window and click the button below to copy its path.");

        if (GUILayout.Button("Copy Path"))
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                string path = GetGameObjectPath(selectedObject);
                EditorGUIUtility.systemCopyBuffer = path;
                Debug.Log("GameObject path copied to clipboard: " + path);
            }
            else
            {
                Debug.Log("No GameObject selected in the Hierarchy window.");
            }
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
}