using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class SetMeshImporterData
{
    [MenuItem("Assets/Import Settings/Enable Mesh Combine (Experimental)")]
    private static void EnableMeshCombine()
    {
        Object obj = Selection.activeObject;
        string userData = "meshCombine=true";
        SetModelUserData(obj, userData);
    }

    [MenuItem("Assets/Import Settings/Disable Mesh Combine (Experimental)")]
    private static void DisableMeshCombine()
    {
        Object obj = Selection.activeObject;
        string userData = "";
        SetModelUserData(obj, userData);
    }

    [MenuItem("Assets/Import Settings/Enable Mesh Combine", true)]
    private static bool EnableMeshCombineValidation()
    {
        Object obj = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(obj);
        AssetImporter importer = AssetImporter.GetAtPath(path);
        return importer is ModelImporter;
    }

    [MenuItem("Assets/Import Settings/Disable Mesh Combine", true)]
    private static bool DisableMeshCombineValidation()
    {
        Object obj = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(obj);
        AssetImporter importer = AssetImporter.GetAtPath(path);
        return importer is ModelImporter;
    }

    private static void SetModelUserData(Object obj, string userData)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer is ModelImporter modelImporter)
        {
            modelImporter.userData = userData;
            modelImporter.SaveAndReimport();
        }
    }
}
