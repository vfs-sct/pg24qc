#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using Cinemachine;

[assembly: RegisterValidationRule(typeof(ScenePrefabValidator), Name = "Scene Prefab Validator", Description = "Checks for non-prefab assets in the scene.")]

public class ScenePrefabValidator : SceneValidator
{

    protected override void Validate(ValidationResult result)
    {
        ValidateComponentType<Animator>(result, false);
        ValidateComponentType<MeshRenderer>(result, false);
        ValidateComponentType<SkinnedMeshRenderer>(result, false);
        ValidateComponentType<SpriteRenderer>(result, false);
        ValidateComponentType<CinemachineVirtualCameraBase>(result, false);
    }

    private void ValidateComponentType<T>(ValidationResult result, bool onlyCheckModelPrefabs) where T : Component
    {
        var components = FindComponentsOfType<T>(true);
        foreach (var component in components)
        {
            if ((!onlyCheckModelPrefabs || PrefabUtility.IsPartOfAnyPrefab(component)) && (!PrefabUtility.IsPartOfRegularPrefab(component) && !PrefabUtility.IsPartOfVariantPrefab(component)))
            {
                string path = GetTransformPath(component.transform);
                result.AddWarning($"{path} is not part of a prefab.");
            }
        }
    }

    private IEnumerable<T> FindComponentsOfType<T>(bool includeInactive = true) where T : Component
    {
        IEnumerable<UnityEngine.Object> enumerable = FindComponentsByType(typeof(T), includeInactive);
        foreach (UnityEngine.Object item in enumerable)
        {
            T val = item as T;
            if (val != null && val.gameObject.scene.path == ValidatedScene.Path)
            {
                yield return val;
            }
        }
    }

    private static IEnumerable<UnityEngine.Object> FindComponentsByType(Type t, bool includeInactive)
    {
        UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(t);
        UnityEngine.Object[] array2 = array;
        foreach (UnityEngine.Object @object in array2)
        {
            Component component = @object as Component;
            if ((object)component != null && (@object.hideFlags & HideFlags.DontSave) == 0 && component.gameObject.scene.IsValid())
            {
                yield return @object;
            }
        }
    }

    private static string GetTransformPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
#endif