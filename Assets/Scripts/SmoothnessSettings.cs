#region

using System.ComponentModel;
using UnityEngine;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Global

public class SmoothnessSettings
{
    [DefaultValue(0.1f)] public float BaseSmoothness;

    [DefaultValue("0.1")] public string BaseSmoothnessText;

    [DefaultValue(3.0f)] public float BlurOverlay;

    [DefaultValue("3")] public string BlurOverlayText;

    [DefaultValue(0)] public int BlurSize;

    [DefaultValue("0")] public string BlurSizeText;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue("1")] public string FinalContrastText;

    [DefaultValue(1.0f)] public float HueWeight1;

    [DefaultValue(1.0f)] public float HueWeight2;

    [DefaultValue(1.0f)] public float HueWeight3;

    [DefaultValue(false)] public bool IsolateSample1;

    [DefaultValue(false)] public bool IsolateSample2;

    [DefaultValue(false)] public bool IsolateSample3;

    [DefaultValue(0.2f)] public float LumWeight1;

    [DefaultValue(0.2f)] public float LumWeight2;

    [DefaultValue(0.2f)] public float LumWeight3;

    [DefaultValue(1.0f)] public float MaskHigh1;

    [DefaultValue(1.0f)] public float MaskHigh2;

    [DefaultValue(1.0f)] public float MaskHigh3;

    [DefaultValue(0.0f)] public float MaskLow1;

    [DefaultValue(0.0f)] public float MaskLow2;

    [DefaultValue(0.0f)] public float MaskLow3;

    [DefaultValue(0.7f)] public float MetalSmoothness;

    [DefaultValue("0.7")] public string MetalSmoothnessText;

    [DefaultValue(30)] public int OverlayBlurSize;

    [DefaultValue("30")] public string OverlayBlurSizeText;

    [DefaultValue(0.5f)] public float Sample1Smoothness;

    [DefaultValue(0.3f)] public float Sample2Smoothness;

    [DefaultValue(0.2f)] public float Sample3Smoothness;

    [DefaultValue(0)] public bool Invert;

    //[DefaultValueAttribute(Color.black)]
    public Color SampleColor1;

    //[DefaultValueAttribute(Color.black)]
    public Color SampleColor2;

    //[DefaultValueAttribute(Color.black)]
    public Color SampleColor3;

    //[DefaultValueAttribute(Vector2.zero)]
    public Vector2 SampleUv1;

    //[DefaultValueAttribute(Vector2.zero)]
    public Vector2 SampleUv2;

    //[DefaultValueAttribute(Vector2.zero)]
    public Vector2 SampleUv3;

    [DefaultValue(0.5f)] public float SatWeight1;

    [DefaultValue(0.5f)] public float SatWeight2;

    [DefaultValue(0.5f)] public float SatWeight3;

    [DefaultValue(false)] public bool UseAdjustedDiffuse;

    [DefaultValue(true)] public bool UseOriginalDiffuse;

    [DefaultValue(false)] public bool UsePastedSmoothness;

    [DefaultValue(false)] public bool UseSample1;

    [DefaultValue(false)] public bool UseSample2;

    [DefaultValue(false)] public bool UseSample3;

    public SmoothnessSettings()
    {
        SampleColor1 = Color.black;
        SampleUv1 = Vector2.zero;

        SampleColor2 = Color.black;
        SampleUv2 = Vector2.zero;

        SampleColor3 = Color.black;
        SampleUv3 = Vector2.zero;

        MetalSmoothness = 0.7f;
        MetalSmoothnessText = "0.7";

        UseSample1 = false;
        IsolateSample1 = false;
        HueWeight1 = 1.0f;
        SatWeight1 = 0.5f;
        LumWeight1 = 0.2f;
        MaskLow1 = 0.0f;
        MaskHigh1 = 1.0f;
        Sample1Smoothness = 0.5f;

        UseSample2 = false;
        IsolateSample2 = false;
        HueWeight2 = 1.0f;
        SatWeight2 = 0.5f;
        LumWeight2 = 0.2f;
        MaskLow2 = 0.0f;
        MaskHigh2 = 1.0f;
        Sample2Smoothness = 0.3f;

        UseSample3 = false;
        IsolateSample3 = false;
        HueWeight3 = 1.0f;
        SatWeight3 = 0.5f;
        LumWeight3 = 0.2f;
        MaskLow3 = 0.0f;
        MaskHigh3 = 1.0f;
        Sample3Smoothness = 0.2f;

        BaseSmoothness = 0.1f;
        BaseSmoothnessText = "0.1";

        BlurSize = 0;
        BlurSizeText = "0";

        OverlayBlurSize = 30;
        OverlayBlurSizeText = "30";

        BlurOverlay = 3.0f;
        BlurOverlayText = "3";

        FinalContrast = 1.0f;
        FinalContrastText = "1";

        FinalBias = 0.0f;
        FinalBiasText = "0";
        Invert = false;
        UseAdjustedDiffuse = false;
        UseOriginalDiffuse = true;
    }
}