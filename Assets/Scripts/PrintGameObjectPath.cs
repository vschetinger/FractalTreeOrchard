using UnityEngine;
using UnityEditor;

// public class PrintGameObjectPath : MonoBehaviour
// {
//     [MenuItem("Tools/Print Selected GameObject Path")]
//     static void PrintPath()
//     {
//         if (Selection.activeGameObject!= null)
//         {
//             string path = GetFullPath(Selection.activeGameObject.transform);
//             Debug.Log("Full path of selected GameObject: " + path);
//         }
//         else
//         {
//             Debug.Log("No GameObject selected.");
//         }
//     }

//     static string GetFullPath(Transform transform)
//     {
//         if (transform.parent == null)
//             return transform.name;
//         return GetFullPath(transform.parent) + "/" + transform.name;
//     }
// }