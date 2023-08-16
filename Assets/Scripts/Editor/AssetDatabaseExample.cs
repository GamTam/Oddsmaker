using UnityEditor;
using UnityEngine;

public class AssetDatabaseExamples : MonoBehaviour
{
    [MenuItem("AssetDatabase/Remove Bundle Name")]
    static void RemoveAssetBundleNameExample()
    {
        //Remove Asset Bundle name that is on Cube.prefab and it's dependencies
        var prefabPath = "Assets/Prefabs/Characters/Frank Sahwit.prefab";
        var assetBundleName = "AssetBundles/default";
        var assetBundleDependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
        AssetDatabase.RemoveAssetBundleName(assetBundleName, true);
        foreach (var bundleName in assetBundleDependencies)
        {
            AssetDatabase.RemoveAssetBundleName(bundleName, true);
        }
    }
}