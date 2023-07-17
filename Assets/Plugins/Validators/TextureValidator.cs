#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using UnityEditor;
using System.IO;

[assembly: RegisterValidationRule(typeof(TextureValidator), Name = "Texture Validator", Description = "Checks textures for correct import settings.")]

public class TextureValidator : RootObjectValidator<Texture>
{
    protected override void Validate(ValidationResult result)
    {
        Texture texture = Object;
        string path = AssetDatabase.GetAssetPath(texture);
        bool checkCommon = false;
        string textureName = texture.name.ToLower();

        if(textureName.Contains("basecolor"))
        {
            checkCommon = true;
        }

        if (textureName.Contains("normaldx"))
        {
            checkCommon = true;
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                result.AddWarning("Normal map Texture Type must be set to Normal Map.")
                    .WithFix(() => SetNormalMap(texture, importer));
            }

        }

        if (textureName.Contains("roughao"))
        {
            checkCommon= true;
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (importer.sRGBTexture)
            {
                result.AddWarning("Mask map texture must have sRGB(Color Texture) turned off.")
                    .WithFix(() => SetSRGB(texture, importer, false));
            }
        }

        string extension = Path.GetExtension(path).ToLower();
        if(checkCommon && (!extension.Contains("tga") && !extension.Contains("tif")))
        {
            result.AddWarning("PBR textures must be exported in .tga format.");
        }
    }

    private void SetNormalMap(Texture texture, TextureImporter importer)
    {
        importer.textureType = TextureImporterType.NormalMap;
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture), ImportAssetOptions.ForceUpdate);
    }

    private void SetSRGB(Texture texture, TextureImporter importer, bool sRGBTrue)
    {
        importer.sRGBTexture = sRGBTrue;
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture), ImportAssetOptions.ForceUpdate);
    }
}
#endif
