using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

[InitializeOnLoad]
public static class MarketingTools
{
    private static string _overlayName = "MarketingOverlay";
    private static GameObject _overlay;

    static MarketingTools()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
    {
        if(LoadSettings(out MarketingToolsSettings settings))
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    if (settings.OnStartPlay) Screenshot(settings.AutoScreenshotPath, settings.AutoScreenshotScale);
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    if (settings.OnStopPlay) Screenshot(settings.AutoScreenshotPath, settings.AutoScreenshotScale);
                    break;
            }
        }
    }

    [MenuItem("Tools/Marketing/Marketing Screenshot _F11")]
    public static void MarketingScreenshot()
    {
        if(LoadSettings(out MarketingToolsSettings settings))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(MarketingScreenshotRoutine(settings.ScreenshotPath, settings.ScreenshotScale));
        }       
    }

    private static IEnumerator MarketingScreenshotRoutine(string folderName, int scale = 1)
    {
        if (_overlay == null) InstantiateMarketingOverlay();

        Screenshot(folderName, scale);

        yield return new WaitForEndOfFrame();

        if (_overlay != null) Editor.DestroyImmediate(_overlay);
    }

    private static void Screenshot(string folderName, int scale = 1)
    {
        string path = $"{Path.GetDirectoryName(Application.dataPath)}\\{folderName}";
        bool directoryExists = Directory.Exists(path);
        if (!directoryExists) Directory.CreateDirectory(path);

        string projectName = PlayerSettings.productName;
        string dateString = DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss");
        string fileType = ".png";
        string fullPath = $"{folderName}/{projectName}_{dateString}{fileType}";
        ScreenCapture.CaptureScreenshot(fullPath, scale);
        Debug.Log($"Screenshot saved to: {fullPath}");
    }

    [MenuItem("Tools/Marketing/Toggle Marketing Overlay _F12")]
    private static void ToggleMarketingOverlay()
    {
        if(_overlay != null)
        {
            Editor.DestroyImmediate(_overlay);
            return;
        }

        InstantiateMarketingOverlay();
    }

    [MenuItem("Tools/Marketing/Open Screenshots Folder")]
    private static void OpenScreenshotsFolder()
    {
        if (LoadSettings(out MarketingToolsSettings settings))
        {
            OpenFolder(settings.ScreenshotPath);
        }
    }

    [MenuItem("Tools/Marketing/Open Recordings Folder")]
    private static void OpenRecordingsFolder()
    {
        if (LoadSettings(out MarketingToolsSettings settings))
        {
            OpenFolder(settings.RecordingPath);
        }
    }

    private static void OpenFolder(string location)
    {
        string path = $"{Path.GetDirectoryName(Application.dataPath)}\\{location}";
        bool directoryExists = Directory.Exists(path);
        if (!directoryExists) Directory.CreateDirectory(path);

        path = path.Replace(@"/", @"\");
        System.Diagnostics.Process.Start("explorer.exe", "/open," + path);
    }

    private static bool LoadSettings(out MarketingToolsSettings settings)
    {
        settings = AssetDatabase.LoadAssetAtPath<MarketingToolsSettings>("Assets/Plugins/MarketingTools/Settings.asset");
        if (settings == null)
        {
            Debug.LogWarning("Settings file for marketing tools is missing!");
            return false;
        }
        
        return true;
    }

    private static bool LoadMarketingOverlay(out GameObject overlay)
    {
        overlay = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Plugins/MarketingTools/MarketingOverlay.prefab");
        if(overlay == null)
        {
            Debug.LogWarning("Overlay for marketing tools is missing!");
            return false;
        }

        return true;
    }

    private static void InstantiateMarketingOverlay()
    {
        if(LoadMarketingOverlay(out GameObject prefab))
        {
            GameObject overlay = Editor.Instantiate(prefab);
            overlay.name = _overlayName;
            _overlay = overlay;
        }
    }
}