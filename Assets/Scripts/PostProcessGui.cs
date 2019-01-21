using UnityEngine;

public class PostProcessGui : MonoBehaviour
{
    private bool AutoFocus = true;

    private float BloomAmount = 1.0f;
    private string BloomAmountText = "1.0";

    private float BloomThreshold = 0.8f;
    private string BloomThresholdText = "0.8";

    private float DOFFocalDepth = 10.0f;
    private string DOFFocalDepthText = "10.0";

    private float DOFMaxBlur;
    private string DOFMaxBlurText = "0.0";

    private float DOFMaxDistance = 50.0f;
    private string DOFMaxDistanceText = "50.0";

    private bool EnablePostProcess = true;

    private bool initialized;

    private float LensDirtAmount = 1.0f;
    private string LensDirtAmountText = "1.0";

    private float LensFlareAmount = 0.5f;
    private string LensFlareAmountText = "0.5";
    public GameObject MainCamera;

    public GameObject MainGuiObject;

    private OpaquePostProcess oppScript;
    private PostProcess ppScript;

    private bool UseTAA = true;

    private float VignetteAmount = 0.2f;
    private string VignetteAmountText = "0.2";

    private Rect windowRect = new Rect(360, 330, 300, 530);

    // Use this for initialization
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!initialized)
        {
            oppScript = MainCamera.GetComponent<OpaquePostProcess>();
            ppScript = MainCamera.GetComponent<PostProcess>();
            initialized = true;
        }
    }

    public void PostProcessOn()
    {
        Initialize();

        EnablePostProcess = true;

        oppScript.enabled = true;

        ppScript.enabled = true;
        ppScript.bloomThreshold = BloomThreshold;
        ppScript.bloomAmount = BloomAmount;

        ppScript.lensFlareAmount = LensFlareAmount;
        ppScript.lensDirtAmount = LensDirtAmount;
        ppScript.vignetteAmount = VignetteAmount;


        if (DOFMaxBlur > 12)
            ppScript.DOFMaxBlur = 16;
        else if (DOFMaxBlur > 6)
            ppScript.DOFMaxBlur = 8;
        else if (DOFMaxBlur > 3)
            ppScript.DOFMaxBlur = 4;
        else if (DOFMaxBlur > 1.5)
            ppScript.DOFMaxBlur = 2;
        else if (DOFMaxBlur > 0.5)
            ppScript.DOFMaxBlur = 1;
        else
            ppScript.DOFMaxBlur = 0;
        ppScript.focalDepth = DOFFocalDepth;
        ppScript.DOFMaxDistance = DOFMaxDistance;

        ppScript.AutoFocus = AutoFocus;
    }

    public void PostProcessOff()
    {
        Initialize();

        EnablePostProcess = false;

        oppScript.enabled = false;
        ppScript.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (EnablePostProcess)
            PostProcessOn();
        else
            PostProcessOff();
    }

    private void DoMyWindow(int windowID)
    {
        var spacingX = 0;
        var spacingY = 50;
        var spacing2Y = 70;

        var offsetX = 10;
        var offsetY = 30;

        EnablePostProcess = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), EnablePostProcess, "Enable Post Process");
        offsetY += 40;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Threshold", BloomThreshold, BloomThresholdText,
            out BloomThreshold, out BloomThresholdText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Bloom Amount", BloomAmount, BloomAmountText,
            out BloomAmount, out BloomAmountText, 0.0f, 8.0f);
        offsetY += 60;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Flare Amount", LensFlareAmount, LensFlareAmountText,
            out LensFlareAmount, out LensFlareAmountText, 0.0f, 4.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Dirt Amount", LensDirtAmount, LensDirtAmountText,
            out LensDirtAmount, out LensDirtAmountText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Vignette Amount", VignetteAmount, VignetteAmountText,
            out VignetteAmount, out VignetteAmountText, 0.0f, 1.0f);
        offsetY += 60;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Max Blur", DOFMaxBlur, DOFMaxBlurText,
            out DOFMaxBlur, out DOFMaxBlurText, 0.0f, 16.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Focal Depth", DOFFocalDepth, DOFFocalDepthText,
            out DOFFocalDepth, out DOFFocalDepthText, 1.0f, 50.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "DOF Max Distance", DOFMaxDistance, DOFMaxDistanceText,
            out DOFMaxDistance, out DOFMaxDistanceText, 5.0f, 200.0f);
        offsetY += 50;

        AutoFocus = GUI.Toggle(new Rect(offsetX, offsetY, 150, 20), AutoFocus, "Use Auto Focus");
        offsetY += 30;

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Close")) gameObject.SetActive(false);

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect.width = 300;
        windowRect.height = 510;

        windowRect = GUI.Window(19, windowRect, DoMyWindow, "Post Process");
    }
}