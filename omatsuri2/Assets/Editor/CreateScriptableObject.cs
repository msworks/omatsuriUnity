using UnityEngine;
using UnityEditor;
using System.Collections;

public class CreateScriptableObject : MonoBehaviour {
    [MenuItem("Assets/Create/ScriptableObject")]
    public static void CreateAsset() {
        CreateAsset<ScriptableObject>();
    }

    public static void CreateAsset<Type>() where Type : ScriptableObject {
        Type item = ScriptableObject.CreateInstance<Type>();

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + typeof(Type) + ".asset");

        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}