#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

[assembly: RegisterValidationRule(typeof(LightingValidator), Name = "Lighting Validator", Description = "Checks scenes for correct lighting/post-processing settings.")]

public class LightingValidator : SceneValidator 
{
    private string[] _lightingIgnoreList = new string[] { "2D" };

    protected override void Validate(ValidationResult result)
    {
        string sceneName = ValidatedScene.Name.ToLower();
        bool ignoreLighting = false;
        for (int i = 0; i < _lightingIgnoreList.Length; i++)
        {
            if (sceneName.Contains(_lightingIgnoreList[i].ToLower())) ignoreLighting = true;
        }

        if (!ignoreLighting)
        {
            if (!Lightmapping.TryGetLightingSettings(out LightingSettings lightingSettings))
            {
                result.AddWarning("Lighting Settings asset is null, recommend applying NeutralLightSettings asset.");
            }

            if (Lightmapping.lightingDataAsset == null)
            {
                result.AddWarning("Light bake needs to be generated.");
            }
        }

        if(!ignoreLighting)
        {
            IEnumerable<LightProbeGroup> lightProbeGroups = FindAllComponentsInSceneOfType<LightProbeGroup>();
            int lightProbeGroupCount = 0;
            foreach (LightProbeGroup lightProbeGroup in lightProbeGroups)
            {
                lightProbeGroupCount++;
            }
            if (lightProbeGroupCount <= 0)
            {
                result.AddWarning("No lightprobes found in scene.");
            }
        }
    }
}
#endif
