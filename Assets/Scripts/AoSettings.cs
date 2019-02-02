#region

using System.ComponentModel;

#endregion

public class AoSettings
{
    [DefaultValue(1.0f)] public float Blend;

    [DefaultValue("1")] public string BlendText;

    [DefaultValue(100.0f)] public float Depth;

    [DefaultValue("100")] public string DepthText;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue("1")] public string FinalContrastText;

    [DefaultValue(5.0f)] public float Spread;

    [DefaultValue("50")] public string SpreadText;

    public AoSettings()
    {
        Spread = 50.0f;
        SpreadText = "50";

        Depth = 100.0f;
        DepthText = "100";

        FinalBias = 0.0f;
        FinalBiasText = "0";

        FinalContrast = 1.0f;
        FinalContrastText = "1";

        Blend = 1.0f;
        BlendText = "1";
    }
}