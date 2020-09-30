#region

using System.ComponentModel;

#endregion

public class EdgeSettings
{
    [DefaultValue(1.0)] public float Blur0Contrast;

    [DefaultValue("1")] public string Blur0ContrastText;

    [DefaultValue(1.0f)] public float Blur0Weight;

    [DefaultValue(0.5f)] public float Blur1Weight;

    [DefaultValue(0.3f)] public float Blur2Weight;

    [DefaultValue(0.5f)] public float Blur3Weight;

    [DefaultValue(0.7f)] public float Blur4Weight;

    [DefaultValue(0.7f)] public float Blur5Weight;

    [DefaultValue(0.3f)] public float Blur6Weight;

    [DefaultValue(1.0f)] public float CreviceAmount;

    [DefaultValue("1")] public string CreviceAmountText;

    [DefaultValue(1.0f)] public float EdgeAmount;

    [DefaultValue("1")] public string EdgeAmountText;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue("1")] public string FinalContrastText;

    [DefaultValue(1.0f)] public float Pillow;

    [DefaultValue("1")] public string PillowText;

    [DefaultValue(1.0f)] public float Pinch;

    [DefaultValue("1")] public string PinchText;

    public EdgeSettings()
    {
        Blur0Contrast = 1.0f;
        Blur0ContrastText = "1";

        Blur0Weight = 1.0f;
        Blur1Weight = 0.5f;
        Blur2Weight = 0.3f;
        Blur3Weight = 0.5f;
        Blur4Weight = 0.7f;
        Blur5Weight = 0.7f;
        Blur6Weight = 0.3f;

        FinalContrast = 1.0f;
        FinalContrastText = "1";

        FinalBias = 0.0f;
        FinalBiasText = "0";

        EdgeAmount = 1.0f;
        EdgeAmountText = "1";

        CreviceAmount = 1.0f;
        CreviceAmountText = "1";

        Pinch = 1.0f;
        PinchText = "1";

        Pillow = 1.0f;
        PillowText = "1";
    }
}