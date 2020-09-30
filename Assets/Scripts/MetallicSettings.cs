#region

using System.ComponentModel;
using UnityEngine;

#endregion

public class MetallicSettings
{
    [DefaultValue(1.0f)] public float BlurOverlay;

    [DefaultValue("1")] public string BlurOverlayText;

    [DefaultValue(0)] public int BlurSize;

    [DefaultValue("0")] public string BlurSizeText;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue("1")] public string FinalContrastText;

    [DefaultValue(1.0f)] public float HueWeight;

    [DefaultValue(0.2f)] public float LumWeight;

    [DefaultValue(1.0f)] public float MaskHigh;

    [DefaultValue(0.0f)] public float MaskLow;

    //[DefaultValueAttribute(Color.black)]
    public Color MetalColor;

    [DefaultValue(30)] public int OverlayBlurSize;

    [DefaultValue("30")] public string OverlayBlurSizeText;

    //[DefaultValueAttribute(Vector2.zero)]
    public Vector2 SampleUv;

    [DefaultValue(0.5f)] public float SatWeight;

    [DefaultValue(false)] public bool UseAdjustedDiffuse;

    [DefaultValue(true)] public bool UseOriginalDiffuse;

    public MetallicSettings()
    {
        MetalColor = Color.black;

        SampleUv = Vector2.zero;

        HueWeight = 1.0f;
        SatWeight = 0.5f;
        LumWeight = 0.2f;

        MaskLow = 0.0f;
        MaskHigh = 1.0f;

        BlurSize = 0;
        BlurSizeText = "0";

        OverlayBlurSize = 30;
        OverlayBlurSizeText = "30";

        BlurOverlay = 1.0f;
        BlurOverlayText = "1";

        FinalContrast = 1.0f;
        FinalContrastText = "1";

        FinalBias = 0.0f;
        FinalBiasText = "0";

        UseAdjustedDiffuse = false;
        UseOriginalDiffuse = true;
    }
}