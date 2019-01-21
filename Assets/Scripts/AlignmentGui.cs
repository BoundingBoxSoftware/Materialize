using System.Collections;
using UnityEngine;

public class AlignmentGui : MonoBehaviour
{
    private RenderTexture _AlignMap;

    private RenderTexture _LensMap;
    private RenderTexture _PerspectiveMap;

    private Material blitMaterial;

    private bool doStuff;


    private int GrabbedPoint;

    private float LensDistort;
    private string LensDistortText = "0.0";

    private MainGui MGS;
    public bool newTexture;

    private float PerspectiveX;
    private string PerspectiveXText = "0.0";

    private float PerspectiveY;
    private string PerspectiveYText = "0.0";
    private Vector2 pointBL = new Vector2(0.0f, 0.0f);
    private Vector2 pointBR = new Vector2(1.0f, 0.0f);

    private Vector2 pointTL = new Vector2(0.0f, 1.0f);
    private Vector2 pointTR = new Vector2(1.0f, 1.0f);

    private float Slider = 0.5f;
    private Vector2 StartOffset = Vector2.zero;
    public GameObject testObject;

    private Texture2D textureToAlign;

    public Material thisMaterial;

    private Rect windowRect = new Rect(30, 300, 300, 530);

    // Use this for initialization
    private void Start()
    {
    }

    public void Initialize()
    {
        gameObject.SetActive(true);
        MGS = MainGui.Instance;
        testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
        blitMaterial = new Material(Shader.Find("Hidden/Blit_Alignment"));
        blitMaterial.hideFlags = HideFlags.HideAndDontSave;

        if (MGS.DiffuseMapOriginal != null)
            textureToAlign = MGS.DiffuseMapOriginal;
        else if (MGS.HeightMap != null)
            textureToAlign = MGS.HeightMap;
        else if (MGS.MetallicMap != null)
            textureToAlign = MGS.MetallicMap;
        else if (MGS.SmoothnessMap != null)
            textureToAlign = MGS.SmoothnessMap;
        else if (MGS.EdgeMap != null)
            textureToAlign = MGS.EdgeMap;
        else if (MGS.AoMap != null) textureToAlign = MGS.AoMap;


        doStuff = true;
    }


    private void CleanupTexture(RenderTexture _Texture)
    {
        if (_Texture != null)
        {
            _Texture.Release();
            _Texture = null;
        }
    }

    public void Close()
    {
        CleanupTexture(_LensMap);
        CleanupTexture(_AlignMap);
        CleanupTexture(_PerspectiveMap);
        gameObject.SetActive(false);
    }

    private void SelectClosestPoint()
    {
        if (!Input.GetMouseButton(0))
        {
            RaycastHit hit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                return;

            var hitTC = hit.textureCoord;

            var dist1 = Vector2.Distance(hitTC, pointTL);
            var dist2 = Vector2.Distance(hitTC, pointTR);
            var dist3 = Vector2.Distance(hitTC, pointBL);
            var dist4 = Vector2.Distance(hitTC, pointBR);

            var closestDist = dist1;
            var closestPoint = pointTL;
            GrabbedPoint = 0;
            if (dist2 < closestDist)
            {
                closestDist = dist2;
                closestPoint = pointTR;
                GrabbedPoint = 1;
            }

            if (dist3 < closestDist)
            {
                closestDist = dist3;
                closestPoint = pointBL;
                GrabbedPoint = 2;
            }

            if (dist4 < closestDist)
            {
                closestDist = dist4;
                closestPoint = pointBR;
                GrabbedPoint = 3;
            }

            if (closestDist > 0.1f)
            {
                closestPoint = new Vector2(-1, -1);
                GrabbedPoint = -1;
            }

            thisMaterial.SetVector("_TargetPoint", closestPoint);
        }
    }

    private void DragPoint()
    {
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        var hitTC = hit.textureCoord;

        if (Input.GetMouseButtonDown(0))
        {
            StartOffset = hitTC;
        }
        else if (Input.GetMouseButton(0))
        {
            switch (GrabbedPoint)
            {
                case 0:
                    pointTL += hitTC - StartOffset;
                    thisMaterial.SetVector("_TargetPoint", pointTL);
                    break;
                case 1:
                    pointTR += hitTC - StartOffset;
                    thisMaterial.SetVector("_TargetPoint", pointTR);
                    break;
                case 2:
                    pointBL += hitTC - StartOffset;
                    thisMaterial.SetVector("_TargetPoint", pointBL);
                    break;
                case 3:
                    pointBR += hitTC - StartOffset;
                    thisMaterial.SetVector("_TargetPoint", pointBR);
                    break;
            }

            StartOffset = hitTC;
        }

        doStuff = true;
    }

    // Update is called once per frame
    private void Update()
    {
        SelectClosestPoint();
        DragPoint();

        var aspect = textureToAlign.width / (float) textureToAlign.height;
        var area = 1.0f;
        var pointScale = Vector2.one;
        pointScale.x = aspect;
        var newArea = pointScale.x * pointScale.y;
        var areaScale = Mathf.Sqrt(area / newArea);

        pointScale.x *= areaScale;
        pointScale.y *= areaScale;

        thisMaterial.SetTexture("_MainTex", _LensMap);
        thisMaterial.SetTexture("_CorrectTex", _PerspectiveMap);

        thisMaterial.SetVector("_PointScale", pointScale);

        thisMaterial.SetVector("_PointTL", pointTL);
        thisMaterial.SetVector("_PointTR", pointTR);
        thisMaterial.SetVector("_PointBL", pointBL);
        thisMaterial.SetVector("_PointBR", pointBR);

        var realPerspectiveX = PerspectiveX;
        if (realPerspectiveX < 0.0f)
            realPerspectiveX = Mathf.Abs(1.0f / (realPerspectiveX - 1.0f));
        else
            realPerspectiveX = realPerspectiveX + 1.0f;

        var realPerspectiveY = PerspectiveY;
        if (realPerspectiveY < 0.0f)
            realPerspectiveY = Mathf.Abs(1.0f / (realPerspectiveY - 1.0f));
        else
            realPerspectiveY = realPerspectiveY + 1.0f;

        blitMaterial.SetVector("_PointTL", pointTL);
        blitMaterial.SetVector("_PointTR", pointTR);
        blitMaterial.SetVector("_PointBL", pointBL);
        blitMaterial.SetVector("_PointBR", pointBR);

        blitMaterial.SetFloat("_Width", textureToAlign.width);
        blitMaterial.SetFloat("_Height", textureToAlign.height);

        blitMaterial.SetFloat("_Lens", LensDistort);
        blitMaterial.SetFloat("_PerspectiveX", PerspectiveX);
        blitMaterial.SetFloat("_PerspectiveY", PerspectiveY);

        if (doStuff)
        {
            ProcessMap(textureToAlign);
            doStuff = false;
        }

        thisMaterial.SetFloat("_Slider", Slider);
    }

    private void DoMyWindow(int windowID)
    {
        var spacingX = 0;
        var spacingY = 50;
        var spacing2Y = 70;

        var offsetX = 10;
        var offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Alignment Reveal Slider");
        Slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), Slider, 0.0f, 1.0f);
        offsetY += 40;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Preview Map");
        offsetY += 30;

        if (MGS.DiffuseMapOriginal == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Original Diffuse Map"))
        {
            textureToAlign = MGS.DiffuseMapOriginal;
            doStuff = true;
        }

        if (MGS.DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Diffuse Map"))
        {
            textureToAlign = MGS.DiffuseMap;
            doStuff = true;
        }

        offsetY += 40;


        if (MGS.HeightMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Height Map"))
        {
            textureToAlign = MGS.HeightMap;
            doStuff = true;
        }

        offsetY += 40;

        if (MGS.MetallicMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Metallic Map"))
        {
            textureToAlign = MGS.MetallicMap;
            doStuff = true;
        }

        if (MGS.SmoothnessMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Smoothness Map"))
        {
            textureToAlign = MGS.SmoothnessMap;
            doStuff = true;
        }

        offsetY += 40;

        if (MGS.EdgeMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Edge Map"))
        {
            textureToAlign = MGS.EdgeMap;
            doStuff = true;
        }

        if (MGS.AoMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "AO Map"))
        {
            textureToAlign = MGS.AoMap;
            doStuff = true;
        }

        offsetY += 40;

        GUI.enabled = true;


        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Lens Distort Correction", LensDistort,
            LensDistortText, out LensDistort, out LensDistortText, -1.0f, 1.0f)) doStuff = true;
        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Perspective Correction X", PerspectiveX,
            PerspectiveXText, out PerspectiveX, out PerspectiveXText, -5.0f, 5.0f)) doStuff = true;
        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Perspective Correction Y", PerspectiveY,
            PerspectiveYText, out PerspectiveY, out PerspectiveYText, -5.0f, 5.0f)) doStuff = true;
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Reset Points"))
        {
            pointTL = new Vector2(0.0f, 1.0f);
            pointTR = new Vector2(1.0f, 1.0f);
            pointBL = new Vector2(0.0f, 0.0f);
            pointBR = new Vector2(1.0f, 0.0f);
        }


        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set All Maps")) StartCoroutine(SetMaps());


        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect.width = 300;
        windowRect.height = 430;

        windowRect = GUI.Window(21, windowRect, DoMyWindow, "Texture Alignment Adjuster");
    }

    private void ProcessMap(Texture2D textureTarget)
    {
        var width = textureTarget.width;
        var height = textureTarget.height;

        if (_LensMap == null)
            _LensMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        if (_AlignMap == null)
            _AlignMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        if (_PerspectiveMap == null)
            _PerspectiveMap =
                new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        Graphics.Blit(textureTarget, _LensMap, blitMaterial, 0);
        Graphics.Blit(_LensMap, _AlignMap, blitMaterial, 1);
        Graphics.Blit(_AlignMap, _PerspectiveMap, blitMaterial, 2);
    }

    private Texture2D SetMap(Texture2D textureTarget)
    {
        var width = textureTarget.width;
        var height = textureTarget.height;

        CleanupTexture(_LensMap);
        CleanupTexture(_AlignMap);
        CleanupTexture(_PerspectiveMap);

        _LensMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        _AlignMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        _PerspectiveMap =
            new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        Graphics.Blit(textureTarget, _LensMap, blitMaterial, 0);
        Graphics.Blit(_LensMap, _AlignMap, blitMaterial, 1);
        Graphics.Blit(_AlignMap, _PerspectiveMap, blitMaterial, 2);

        var replaceTexture = false;
        if (textureToAlign == textureTarget) replaceTexture = true;

        Destroy(textureTarget);
        textureTarget = null;

        RenderTexture.active = _PerspectiveMap;
        textureTarget = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        textureTarget.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        textureTarget.Apply();

        RenderTexture.active = null;

        CleanupTexture(_LensMap);
        CleanupTexture(_AlignMap);
        CleanupTexture(_PerspectiveMap);

        if (replaceTexture) textureToAlign = textureTarget;

        doStuff = true;

        return textureTarget;
    }

    private RenderTexture SetMap(RenderTexture textureTarget)
    {
        var width = textureTarget.width;
        var height = textureTarget.height;

        CleanupTexture(_LensMap);
        CleanupTexture(_AlignMap);
        CleanupTexture(_PerspectiveMap);

        _LensMap = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        _AlignMap = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        _PerspectiveMap = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);

        Graphics.Blit(textureTarget, _LensMap, blitMaterial, 0);
        Graphics.Blit(_LensMap, _AlignMap, blitMaterial, 1);
        Graphics.Blit(_AlignMap, _PerspectiveMap, blitMaterial, 2);

        if (textureTarget != null)
        {
            textureTarget.Release();
            textureTarget = null;
        }

        Graphics.Blit(_PerspectiveMap, textureTarget);

        CleanupTexture(_LensMap);
        CleanupTexture(_AlignMap);
        CleanupTexture(_PerspectiveMap);

        doStuff = true;

        return textureTarget;
    }

    private IEnumerator SetMaps()
    {
        if (MGS.HeightMap != null)
        {
            Debug.Log("Setting Height");
            MGS.HeightMap = SetMap(MGS.HeightMap);
        }

        if (MGS.HdHeightMap != null)
        {
            Debug.Log("Setting HD Height");
            MGS.HdHeightMap = SetMap(MGS.HdHeightMap);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.DiffuseMap != null)
        {
            Debug.Log("Setting Diffuse");
            MGS.DiffuseMap = SetMap(MGS.DiffuseMap);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.DiffuseMapOriginal != null)
        {
            Debug.Log("Setting Diffuse Original");
            MGS.DiffuseMapOriginal = SetMap(MGS.DiffuseMapOriginal);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.NormalMap != null)
        {
            Debug.Log("Setting Normal");
            MGS.NormalMap = SetMap(MGS.NormalMap);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.MetallicMap != null)
        {
            Debug.Log("Setting Metallic");
            MGS.MetallicMap = SetMap(MGS.MetallicMap);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.SmoothnessMap != null)
        {
            Debug.Log("Setting Smoothness");
            MGS.SmoothnessMap = SetMap(MGS.SmoothnessMap);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.EdgeMap != null)
        {
            Debug.Log("Setting Edge");
            MGS.EdgeMap = SetMap(MGS.EdgeMap);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.AoMap != null)
        {
            Debug.Log("Setting AO");
            MGS.AoMap = SetMap(MGS.AoMap);
        }

        yield return new WaitForSeconds(0.1f);

        Resources.UnloadUnusedAssets();
    }
}