using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Marketing/Settings")]
public class MarketingToolsSettings : ScriptableObject
{
    [Header("Manual Screenshots")]
    [Range(1, 4)] public int ScreenshotScale = 2;
    public string ScreenshotPath = "Screenshots";

    [Header("Auto Screenshots")]
    [Range(1, 4)] public int AutoScreenshotScale = 1;
    public string AutoScreenshotPath = "Screenshots/Auto";
    public bool OnStartPlay = true;
    public bool OnStopPlay = true;

    [Header("Recordings")]
    public string RecordingPath = "Recordings";
}