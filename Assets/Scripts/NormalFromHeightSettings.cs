#region

using System.ComponentModel;

#endregion

public class NormalFromHeightSettings
{
    [DefaultValue(0.5f)] public float AngularIntensity;

    [DefaultValue("0.5")] public string AngularIntensityText;

    [DefaultValue(0.0f)] public float Angularity;

    [DefaultValue("0")] public string AngularityText;

    [DefaultValue(20.0f)] public float Blur0Contrast;

    [DefaultValue("20")] public string Blur0ContrastText;

    [DefaultValue(0.3f)] public float Blur0Weight;

    [DefaultValue(0.35f)] public float Blur1Weight;

    [DefaultValue(0.5f)] public float Blur2Weight;

    [DefaultValue(0.8f)] public float Blur3Weight;

    [DefaultValue(1.0f)] public float Blur4Weight;

    [DefaultValue(0.95f)] public float Blur5Weight;

    [DefaultValue(0.8f)] public float Blur6Weight;

    [DefaultValue(5.0f)] public float FinalContrast;

    [DefaultValue("5")] public string FinalContrastText;

    [DefaultValue(0.0f)] public float LightRotation;

    [DefaultValue("0")] public string LightRotationText;

    [DefaultValue(0.5f)] public float ShapeBias;

    [DefaultValue("0.5")] public string ShapeBiasText;

    [DefaultValue(0.0f)] public float ShapeRecognition;

    [DefaultValue("0")] public string ShapeRecognitionText;

    [DefaultValue(50.0f)] public int SlopeBlur;

    [DefaultValue("50")] public string SlopeBlurText;

    [DefaultValue(true)] public bool UseDiffuse;

    public NormalFromHeightSettings()
    {
        Blur0Weight = 0.3f;
        Blur1Weight = 0.35f;
        Blur2Weight = 0.5f;
        Blur3Weight = 0.8f;
        Blur4Weight = 1.0f;
        Blur5Weight = 0.95f;
        Blur6Weight = 0.8f;

        Blur0Contrast = 20.0f;
        Blur0ContrastText = "20";

        FinalContrast = 5.0f;
        FinalContrastText = "5";

        Angularity = 0.0f;
        AngularityText = "0";

        AngularIntensity = 0.5f;
        AngularIntensityText = "0.5";

        UseDiffuse = true;

        ShapeRecognition = 0.0f;
        ShapeRecognitionText = "0";

        LightRotation = 0.0f;
        LightRotationText = "0";

        SlopeBlur = 50;
        SlopeBlurText = "50";

        ShapeBias = 0.5f;
        ShapeBiasText = "0.5";
    }
}