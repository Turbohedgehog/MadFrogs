using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetCreator 
{
    public static void CreateAsset<T>() where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + typeof(T).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    [MenuItem("Assets/Mad Frogs/NN Example")]
    public static void CreateExampleAsset()
    {
        CreateAsset<AI.NNExamples>();
    }

    [MenuItem("Assets/Mad Frogs/NN Config")]
    public static void CreateNNConfigAsset()
    {
        CreateAsset<AI.NNConfig>();
    }

    [MenuItem("Assets/Mad Frogs/Frog")]
    public static void CreateFrog()
    {
        CreateAsset<Gameplay.FrogProfile>();
    }

    [MenuItem("Assets/Mad Frogs/Mosquito")]
    public static void CreateMosquito()
    {
        CreateAsset<Gameplay.MosquitoProfile>();
    }

    [MenuItem("Assets/Mad Frogs/Combat Scene Config")]
    public static void CreateCombatSceneConfig()
    {
        CreateAsset<Gameplay.CombatSceneProfile>();
    }
}
