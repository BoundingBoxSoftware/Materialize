#region

using System.ComponentModel;
using UnityEngine;

#endregion

public class HeightFromDiffuseSettings
{
    [DefaultValue(1.0f)] public float Blur0Contrast;


    [DefaultValue(0.15f)] public float Blur0Weight;

    [DefaultValue(1.0f)] public float Blur1Contrast;

    [DefaultValue(0.19f)] public float Blur1Weight;

    [DefaultValue(1.0f)] public float Blur2Contrast;

    [DefaultValue(0.3f)] public float Blur2Weight;

    [DefaultValue(1.0f)] public float Blur3Contrast;

    [DefaultValue(0.5f)] public float Blur3Weight;

    [DefaultValue(1.0f)] public float Blur4Contrast;

    [DefaultValue(0.7f)] public float Blur4Weight;

    [DefaultValue(1.0f)] public float Blur5Contrast;

    [DefaultValue(0.9f)] public float Blur5Weight;

    [DefaultValue(1.0f)] public float Blur6Contrast;

    [DefaultValue(1.0f)] public float Blur6Weight;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.5f)] public float FinalContrast;

    [DefaultValue("1.5")] public string FinalContrastText;

    [DefaultValue(0.0f)] public float FinalGain;

    [DefaultValue("0")] public string FinalGainText;

    [DefaultValue(1.0f)] public float HueWeight1;

    [DefaultValue(1.0f)] public float HueWeight2;

    [DefaultValue(false)] public bool IsolateSample1;

    [DefaultValue(false)] public bool IsolateSample2;

    [DefaultValue(0.2f)] public float LumWeight1;

    [DefaultValue(0.2f)] public float LumWeight2;

    [DefaultValue(1.0f)] public float MaskHigh1;

    [DefaultValue(1.0f)] public float MaskHigh2;

    [DefaultValue(0.0f)] public float MaskLow1;

    [DefaultValue(0.0f)] public float MaskLow2;

    [DefaultValue(0.5f)] public float Sample1Height;

    [DefaultValue(0.5f)] public float Sample2Height;

    [DefaultValue(0.5f)] public float SampleBlend;

    [DefaultValue("0.5")] public string SampleBlendText;

    //[DefaultValueAttribute(Color.black)]
    public Color SampleColor1;

    //[DefaultValueAttribute(Color.black)]
    public Color SampleColor2;

    //[DefaultValueAttribute(Vector2.zero)]
    public Vector2 SampleUv1;

    //[DefaultValueAttribute(Vector2.zero)]
    public Vector2 SampleUv2;

    [DefaultValue(0.5f)] public float SatWeight1;

    [DefaultValue(0.5f)] public float SatWeight2;

    [DefaultValue(50.0f)] public float Spread;

    [DefaultValue(1.0f)] public float SpreadBoost;

    [DefaultValue("1")] public string SpreadBoostText;

    [DefaultValue("50")] public string SpreadText;

    [DefaultValue(true)] public bool UseAdjustedDiffuse;

    [DefaultValue(false)] public bool UseNormal;

    [DefaultValue(false)] public bool UseOriginalDiffuse;

    [DefaultValue(false)] public bool UseSample1;

    [DefaultValue(false)] public bool UseSample2;

    public HeightFromDiffuseSettings()
    {
        UseAdjustedDiffuse = true;
        UseOriginalDiffuse = false;
        UseNormal = false;

        Blur0Weight = 0.15f;
        Blur1Weight = 0.19f;
        Blur2Weight = 0.3f;
        Blur3Weight = 0.5f;
        Blur4Weight = 0.7f;
        Blur5Weight = 0.9f;
        Blur6Weight = 1.0f;

        Blur0Contrast = 1.0f;
        Blur1Contrast = 1.0f;
        Blur2Contrast = 1.0f;
        Blur3Contrast = 1.0f;
        Blur4Contrast = 1.0f;
        Blur5Contrast = 1.0f;
        Blur6Contrast = 1.0f;

        SampleColor1 = Color.black;
        SampleUv1 = Vector2.zero;
        UseSample1 = false;
        IsolateSample1 = false;
        HueWeight1 = 1.0f;
        SatWeight1 = 0.5f;
        LumWeight1 = 0.2f;
        MaskLow1 = 0.0f;
        MaskHigh1 = 1.0f;
        Sample1Height = 0.5f;

        SampleColor2 = Color.black;
        SampleUv2 = Vector2.zero;
        UseSample2 = false;
        IsolateSample2 = false;
        HueWeight2 = 1.0f;
        SatWeight2 = 0.5f;
        LumWeight2 = 0.2f;
        MaskLow2 = 0.0f;
        MaskHigh2 = 1.0f;
        Sample2Height = 0.3f;

        FinalContrast = 1.5f;
        FinalContrastText = "1.5";

        FinalBias = 0.0f;
        FinalBiasText = "0.0";

        FinalGain = 0.0f;
        FinalGainText = "0.0";

        SampleBlend = 0.5f;
        SampleBlendText = "0.5";

        Spread = 50.0f;
        SpreadText = "50";

        SpreadBoost = 1.0f;
        SpreadBoostText = "1";
    }
}