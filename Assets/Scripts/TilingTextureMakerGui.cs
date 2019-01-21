using System.Collections;
using UnityEngine;

public class TilingTextureMakerGui : MonoBehaviour
{
    private RenderTexture _AOMapTemp;
    private RenderTexture _DiffuseMapOriginalTemp;
    private RenderTexture _DiffuseMapTemp;
    private RenderTexture _EdgeMapTemp;

    private RenderTexture _HDHeightMapTemp;
    private RenderTexture _HeightMapTemp;
    private RenderTexture _MetallicMapTemp;
    private RenderTexture _NormalMapTemp;
    private RenderTexture _SmoothnessMapTemp;
    private RenderTexture _SplatTemp;
    private RenderTexture _SplatTempAlt;

    private RenderTexture _TileTemp;

    private Material blitMaterial;

    private bool doStuff;

    private float Falloff = 0.1f;

    private string FalloffText = "0.1";

    private float LastFalloff = 0.1f;

    private int LastNewTexSelectionX = 2;
    private int LastNewTexSelectionY = 2;
    private float LastOverlapX = 0.2f;
    private float LastOverlapY = 0.2f;
    private TileTechnique lastTileTech = TileTechnique.Overlap;

    private MainGui MGS;

    private int NewTexSelectionX = 2;
    private int NewTexSelectionY = 2;

    private int NewTexSizeX = 1024;
    private int NewTexSizeY = 1024;

    private Vector3 objectScale = Vector3.one;

    private Vector2[] offsetKernel;
    private float OverlapX = 0.2f;
    private string OverlapXText = "0.2";
    private float OverlapY = 0.2f;
    private string OverlapYText = "0.2";

    private Vector4[] splatKernel;

    private float SplatRandomize;
    private string SplatRandomizeText = "0.0";

    private float SplatRotation;

    private float SplatRotationRandom = 0.25f;
    private string SplatRotationRandomText = "0.25";
    private string SplatRotationText = "0.0";

    private float SplatScale = 1.0f;
    private string SplatScaleText = "1.0";

    private float SplatWobble = 0.2f;

    private float SplatWobbleRandom = 0.2f;
    private string SplatWobbleRandomText = "0.2";
    private string SplatWobbleText = "0.2";

    private Vector2 targetAR;

    private bool techniqueOverlap = true;
    private bool techniqueSplat;

    public GameObject testObject;
    private float TexOffsetX;
    private string TexOffsetXText = "0.0";
    private float TexOffsetY;
    private string TexOffsetYText = "0.0";

    private GUIContent[] TexSizes;

    private float TexTiling = 1.0f;

    private string TexTilingText = "1.0";

    private Material thisMaterial;

    private TileTechnique tileTech = TileTechnique.Overlap;

    private Rect windowRect = new Rect(30, 300, 300, 530);


    private void Start()
    {
        blitMaterial = new Material(Shader.Find("Hidden/Blit_Seamless_Texture_Maker"));

        TexSizes = new GUIContent[4];
        TexSizes[0] = new GUIContent("512");
        TexSizes[1] = new GUIContent("1024");
        TexSizes[2] = new GUIContent("2048");
        TexSizes[3] = new GUIContent("4096");

        //offsetKernel = new Vector2[4];
        //offsetKernel [0] = new Vector2 (-0.5f, -0.5f);
        //offsetKernel [1] = new Vector2 (-0.5f, 0.5f);
        //offsetKernel [2] = new Vector2 (0.5f, -0.5f);
        //offsetKernel [3] = new Vector2 (0.5f, 0.5f);

        offsetKernel = new Vector2[9];
        offsetKernel[0] = new Vector2(-1, -1);
        offsetKernel[1] = new Vector2(-1, 0);
        offsetKernel[2] = new Vector2(-1, 1);
        offsetKernel[3] = new Vector2(0, -1);
        offsetKernel[4] = new Vector2(0, 0);
        offsetKernel[5] = new Vector2(0, 1);
        offsetKernel[6] = new Vector2(1, -1);
        offsetKernel[7] = new Vector2(1, 0);
        offsetKernel[8] = new Vector2(1, 1);
    }

    public void Initialize()
    {
        MGS = MainGui.Instance;
        thisMaterial = MGS.FullMaterial;

        testObject.GetComponent<Renderer>().material = thisMaterial;
        doStuff = true;
    }

    private void Update()
    {
        thisMaterial.SetVector("_Tiling", new Vector4(TexTiling, TexTiling, TexOffsetX, TexOffsetY));

        if (LastOverlapX != OverlapX)
        {
            LastOverlapX = OverlapX;
            doStuff = true;
        }

        if (LastOverlapY != OverlapY)
        {
            LastOverlapY = OverlapY;
            doStuff = true;
        }

        if (LastFalloff != Falloff)
        {
            LastFalloff = Falloff;
            doStuff = true;
        }

        if (NewTexSelectionX != LastNewTexSelectionX)
        {
            LastNewTexSelectionX = NewTexSelectionX;
            doStuff = true;
        }

        if (NewTexSelectionY != LastNewTexSelectionY)
        {
            LastNewTexSelectionY = NewTexSelectionY;
            doStuff = true;
        }

        if (tileTech != lastTileTech)
        {
            lastTileTech = tileTech;
            doStuff = true;
        }

        if (doStuff)
        {
            doStuff = false;

            switch (NewTexSelectionX)
            {
                case 0:
                    NewTexSizeX = 512;
                    break;
                case 1:
                    NewTexSizeX = 1024;
                    break;
                case 2:
                    NewTexSizeX = 2048;
                    break;
                case 3:
                    NewTexSizeX = 4096;
                    break;
                default:
                    NewTexSizeX = 1024;
                    break;
            }

            switch (NewTexSelectionY)
            {
                case 0:
                    NewTexSizeY = 512;
                    break;
                case 1:
                    NewTexSizeY = 1024;
                    break;
                case 2:
                    NewTexSizeY = 2048;
                    break;
                case 3:
                    NewTexSizeY = 4096;
                    break;
                default:
                    NewTexSizeY = 1024;
                    break;
            }


            var aspect = NewTexSizeX / (float) NewTexSizeY;

            if (Mathf.Approximately(aspect, 8.0f))
                SKRectWide3();
            else if (Mathf.Approximately(aspect, 4.0f))
                SKRectWide2();
            else if (Mathf.Approximately(aspect, 2.0f))
                SKRectWide();
            else if (Mathf.Approximately(aspect, 1.0f))
                SKSquare();
            else if (Mathf.Approximately(aspect, 0.5f))
                SKRectTall();
            else if (Mathf.Approximately(aspect, 0.25f))
                SKRectTall2();
            else if (Mathf.Approximately(aspect, 0.125f)) SKRectTall3();


            var area = 1.0f;
            objectScale = Vector3.one;
            objectScale.x = aspect;
            var newArea = objectScale.x * objectScale.y;
            var areaScale = Mathf.Sqrt(area / newArea);

            objectScale.x *= areaScale;
            objectScale.y *= areaScale;

            testObject.transform.localScale = objectScale;

            StartCoroutine(TileTextures());
        }
    }

    private void SKSquare()
    {
        splatKernel = new Vector4[4];
        splatKernel[0] = new Vector4(0.0f, 0.25f, 0.8f, Random.value);
        splatKernel[1] = new Vector4(0.5f, 0.25f, 0.8f, Random.value);
        splatKernel[2] = new Vector4(0.25f, 0.75f, 0.8f, Random.value);
        splatKernel[3] = new Vector4(0.75f, 0.75f, 0.8f, Random.value);
    }

    private void SKRectWide()
    {
        splatKernel = new Vector4[6];
        splatKernel[0] = new Vector4(0.0f, 0.25f, 0.5f, Random.value);
        splatKernel[1] = new Vector4(0.333f, 0.25f, 0.5f, Random.value);
        splatKernel[2] = new Vector4(0.666f, 0.25f, 0.5f, Random.value);

        splatKernel[3] = new Vector4(0.166f, 0.75f, 0.5f, Random.value);
        splatKernel[4] = new Vector4(0.5f, 0.75f, 0.5f, Random.value);
        splatKernel[5] = new Vector4(0.833f, 0.75f, 0.5f, Random.value);
    }

    private void SKRectWide2()
    {
        splatKernel = new Vector4[4];
        splatKernel[0] = new Vector4(0.0f, 0.375f, 0.4f, Random.value);
        splatKernel[1] = new Vector4(0.25f, 0.625f, 0.4f, Random.value);
        splatKernel[2] = new Vector4(0.5f, 0.375f, 0.4f, Random.value);
        splatKernel[3] = new Vector4(0.75f, 0.625f, 0.4f, Random.value);
    }

    private void SKRectWide3()
    {
        splatKernel = new Vector4[8];
        splatKernel[0] = new Vector4(0.0f, 0.375f, 0.25f, Random.value);
        splatKernel[1] = new Vector4(0.125f, 0.625f, 0.25f, Random.value);
        splatKernel[2] = new Vector4(0.25f, 0.375f, 0.25f, Random.value);
        splatKernel[3] = new Vector4(0.375f, 0.625f, 0.25f, Random.value);
        splatKernel[4] = new Vector4(0.5f, 0.375f, 0.25f, Random.value);
        splatKernel[5] = new Vector4(0.625f, 0.625f, 0.25f, Random.value);
        splatKernel[6] = new Vector4(0.75f, 0.375f, 0.25f, Random.value);
        splatKernel[7] = new Vector4(0.875f, 0.625f, 0.25f, Random.value);
    }

    private void SKRectTall()
    {
        splatKernel = new Vector4[6];
        splatKernel[0] = new Vector4(0.25f, 0.0f, 0.5f, Random.value);
        splatKernel[1] = new Vector4(0.25f, 0.333f, 0.5f, Random.value);
        splatKernel[2] = new Vector4(0.25f, 0.666f, 0.5f, Random.value);

        splatKernel[3] = new Vector4(0.75f, 0.166f, 0.5f, Random.value);
        splatKernel[4] = new Vector4(0.75f, 0.5f, 0.5f, Random.value);
        splatKernel[5] = new Vector4(0.75f, 0.833f, 0.5f, Random.value);
    }

    private void SKRectTall2()
    {
        splatKernel = new Vector4[4];
        splatKernel[0] = new Vector4(0.375f, 0.0f, 0.4f, Random.value);
        splatKernel[1] = new Vector4(0.625f, 0.25f, 0.4f, Random.value);
        splatKernel[2] = new Vector4(0.375f, 0.5f, 0.4f, Random.value);
        splatKernel[3] = new Vector4(0.625f, 0.75f, 0.4f, Random.value);
    }

    private void SKRectTall3()
    {
        splatKernel = new Vector4[8];
        splatKernel[0] = new Vector4(0.375f, 0.0f, 0.25f, Random.value);
        splatKernel[1] = new Vector4(0.625f, 0.125f, 0.25f, Random.value);
        splatKernel[2] = new Vector4(0.375f, 0.25f, 0.25f, Random.value);
        splatKernel[3] = new Vector4(0.625f, 0.375f, 0.25f, Random.value);
        splatKernel[4] = new Vector4(0.375f, 0.5f, 0.25f, Random.value);
        splatKernel[5] = new Vector4(0.625f, 0.625f, 0.25f, Random.value);
        splatKernel[6] = new Vector4(0.375f, 0.75f, 0.25f, Random.value);
        splatKernel[7] = new Vector4(0.625f, 0.875f, 0.25f, Random.value);
    }

    private void DoMyWindow(int windowID)
    {
        var spacingX = 0;
        var spacingY = 50;
        var spacing2Y = 70;

        var offsetX = 10;
        var offsetY = 30;

        techniqueOverlap = GUI.Toggle(new Rect(offsetX, offsetY, 130, 30), techniqueOverlap, "Technique Overlap");
        if (techniqueOverlap)
        {
            techniqueSplat = false;
            tileTech = TileTechnique.Overlap;
        }
        else if (!techniqueSplat)
        {
            techniqueOverlap = true;
            tileTech = TileTechnique.Overlap;
        }

        techniqueSplat = GUI.Toggle(new Rect(offsetX + 150, offsetY, 130, 30), techniqueSplat, "Technique Splat");
        if (techniqueSplat)
        {
            techniqueOverlap = false;
            tileTech = TileTechnique.Splat;
        }
        else if (!techniqueOverlap)
        {
            techniqueSplat = true;
            tileTech = TileTechnique.Splat;
        }

        offsetY += 40;

        GUI.Label(new Rect(offsetX, offsetY, 150, 20), "New Texture Size X");
        NewTexSelectionX = GUI.SelectionGrid(new Rect(offsetX, offsetY + 30, 120, 50), NewTexSelectionX, TexSizes, 2);

        GUI.Label(new Rect(offsetX + 150, offsetY, 150, 20), "New Texture Size Y");
        NewTexSelectionY =
            GUI.SelectionGrid(new Rect(offsetX + 150, offsetY + 30, 120, 50), NewTexSelectionY, TexSizes, 2);

        offsetY += 100;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Edge Falloff", Falloff, FalloffText, out Falloff,
            out FalloffText, 0.01f, 1.0f)) doStuff = true;
        offsetY += 40;

        if (techniqueOverlap)
        {
            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlap X", OverlapX, OverlapXText, out OverlapX,
                out OverlapXText, 0.00f, 1.0f)) doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlap Y", OverlapY, OverlapYText, out OverlapY,
                out OverlapYText, 0.00f, 1.0f)) doStuff = true;
            offsetY += 50;
        }

        if (techniqueSplat)
        {
            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Rotation", SplatRotation,
                SplatRotationText, out SplatRotation, out SplatRotationText, 0.0f, 1.0f)) doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Random Rotation", SplatRotationRandom,
                SplatRotationRandomText, out SplatRotationRandom, out SplatRotationRandomText, 0.0f, 1.0f))
                doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Scale", SplatScale, SplatScaleText,
                out SplatScale, out SplatScaleText, 0.5f, 2.0f)) doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Wooble Amount", SplatWobble,
                SplatWobbleText, out SplatWobble, out SplatWobbleText, 0.0f, 1.0f)) doStuff = true;
            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Splat Randomize", SplatRandomize,
                SplatRandomizeText, out SplatRandomize, out SplatRandomizeText, 0.0f, 1.0f)) doStuff = true;
            offsetY += 50;
        }

        GUI.Label(new Rect(offsetX, offsetY, 150, 30), "Tiling Test Variables");
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling", TexTiling, TexTilingText, out TexTiling,
            out TexTilingText, 0.1f, 5.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset X", TexOffsetX, TexOffsetXText,
            out TexOffsetX, out TexOffsetXText, -1.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset Y", TexOffsetY, TexOffsetYText,
            out TexOffsetY, out TexOffsetYText, -1.0f, 1.0f);
        offsetY += 40;

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set Maps")) StartCoroutine(SetMaps());


        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect.width = 300;
        if (techniqueSplat)
            windowRect.height = 610;
        else
            windowRect.height = 490;

        windowRect = GUI.Window(18, windowRect, DoMyWindow, "Tiling Texture Maker");
    }

    private Texture2D SetMap(Texture2D textureTarget, RenderTexture textureToSet)
    {
        RenderTexture.active = textureToSet;
        textureTarget = new Texture2D(NewTexSizeX, NewTexSizeY, TextureFormat.ARGB32, true, true);
        textureTarget.ReadPixels(new Rect(0, 0, NewTexSizeX, NewTexSizeY), 0, 0);
        textureTarget.Apply();
        return textureTarget;
    }

    private RenderTexture SetMapRT(RenderTexture textureTarget, RenderTexture textureToSet)
    {
        textureTarget = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear);
        textureTarget.wrapMode = TextureWrapMode.Repeat;
        Graphics.Blit(textureToSet, textureTarget);
        return textureTarget;
    }

    private IEnumerator SetMaps()
    {
        if (MGS.HeightMap == null) yield break;

        if (MGS.DiffuseMap != null)
        {
            Debug.Log("Setting Diffuse");
            Destroy(MGS.DiffuseMap);
            MGS.DiffuseMap = null;
            MGS.DiffuseMap = SetMap(MGS.DiffuseMap, _DiffuseMapTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.DiffuseMapOriginal != null)
        {
            Debug.Log("Setting Original Diffuse");
            Destroy(MGS.DiffuseMapOriginal);
            MGS.DiffuseMapOriginal = null;
            MGS.DiffuseMapOriginal = SetMap(MGS.DiffuseMapOriginal, _DiffuseMapOriginalTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.MetallicMap != null)
        {
            Debug.Log("Setting Specular");
            Destroy(MGS.MetallicMap);
            MGS.MetallicMap = null;
            MGS.MetallicMap = SetMap(MGS.MetallicMap, _MetallicMapTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.SmoothnessMap != null)
        {
            Debug.Log("Setting Roughness");
            Destroy(MGS.SmoothnessMap);
            MGS.SmoothnessMap = null;
            MGS.SmoothnessMap = SetMap(MGS.SmoothnessMap, _SmoothnessMapTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.NormalMap != null)
        {
            Debug.Log("Setting Normal");
            Destroy(MGS.NormalMap);
            MGS.NormalMap = null;
            MGS.NormalMap = SetMap(MGS.NormalMap, _NormalMapTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.EdgeMap != null)
        {
            Debug.Log("Setting Edge");
            Destroy(MGS.EdgeMap);
            MGS.EdgeMap = null;
            MGS.EdgeMap = SetMap(MGS.EdgeMap, _EdgeMapTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.AoMap != null)
        {
            Debug.Log("Setting AO");
            Destroy(MGS.AoMap);
            MGS.AoMap = null;
            MGS.AoMap = SetMap(MGS.AoMap, _AOMapTemp);
        }

        yield return new WaitForSeconds(0.1f);

        if (MGS.HeightMap != null)
        {
            Debug.Log("Setting Height");
            Destroy(MGS.HeightMap);
            MGS.HeightMap = null;
            MGS.HeightMap = SetMap(MGS.HeightMap, _HeightMapTemp);
        }

        yield return new WaitForSeconds(0.1f);


        if (MGS.HdHeightMap != null)
        {
            Debug.Log("Setting Height");
            MGS.HdHeightMap.Release();
            MGS.HdHeightMap = null;
            MGS.HdHeightMap = SetMapRT(MGS.HdHeightMap, _HDHeightMapTemp);
        }

        yield return new WaitForSeconds(0.1f);


        Resources.UnloadUnusedAssets();
    }

    public void Close()
    {
        CleanupTextures();
        gameObject.SetActive(false);
    }

    private void CleanupTexture(RenderTexture _Texture)
    {
        if (_Texture != null)
        {
            _Texture.Release();
            _Texture = null;
        }
    }

    private void CleanupTexture(Texture2D _Texture)
    {
        if (_Texture != null)
        {
            Destroy(_Texture);
            _Texture = null;
        }
    }

    private void CleanupTextures()
    {
        Debug.Log("Cleaning Up Textures");

        CleanupTexture(_HDHeightMapTemp);
        CleanupTexture(_HeightMapTemp);
        CleanupTexture(_DiffuseMapTemp);
        CleanupTexture(_DiffuseMapOriginalTemp);
        CleanupTexture(_MetallicMapTemp);
        CleanupTexture(_SmoothnessMapTemp);
        CleanupTexture(_NormalMapTemp);
        CleanupTexture(_EdgeMapTemp);
        CleanupTexture(_AOMapTemp);

        CleanupTexture(_TileTemp);
        CleanupTexture(_SplatTemp);
        CleanupTexture(_SplatTempAlt);
    }

    // need an overload to turn Texture2D into RenderTexture;
    private RenderTexture TileTexture(Texture2D textureToTile, RenderTexture textureTarget, string TexName)
    {
        CleanupTexture(_TileTemp);
        _TileTemp = new RenderTexture(textureToTile.width, textureToTile.height, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear);
        _TileTemp.wrapMode = TextureWrapMode.Repeat;
        blitMaterial.SetTexture("_MainTex", textureToTile);
        Graphics.Blit(textureToTile, _TileTemp);

        return TileTexture(_TileTemp, textureTarget, TexName);
    }

    private RenderTexture TileTexture(RenderTexture textureToTile, RenderTexture textureTarget, string TexName)
    {
        switch (tileTech)
        {
            case TileTechnique.Overlap:
                return TileTextureOverlap(textureToTile, textureTarget, TexName);
                break;
            case TileTechnique.Splat:
                return TileTextureSplat(textureToTile, textureTarget, TexName);
                break;
            default:
                return TileTextureOverlap(textureToTile, textureTarget, TexName);
                break;
        }
    }

    private RenderTexture TileTextureSplat(RenderTexture textureToTile, RenderTexture textureTarget, string TexName)
    {
        if (textureTarget != null)
        {
            textureTarget.Release();
            textureTarget = null;
        }

        //Transform transHelper = new GameObject ().transform;

        CleanupTexture(_SplatTemp);
        CleanupTexture(_SplatTempAlt);

        if (TexName == "_HDDisplacementMap")
        {
            _SplatTemp = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _SplatTemp.wrapMode = TextureWrapMode.Repeat;
            _SplatTempAlt = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            _SplatTempAlt.wrapMode = TextureWrapMode.Repeat;
            textureTarget = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear);
            textureTarget.wrapMode = TextureWrapMode.Repeat;
        }
        else
        {
            _SplatTemp = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            _SplatTemp.wrapMode = TextureWrapMode.Repeat;
            _SplatTempAlt = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            _SplatTempAlt.wrapMode = TextureWrapMode.Repeat;
            textureTarget = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            textureTarget.wrapMode = TextureWrapMode.Repeat;
        }

        textureTarget.wrapMode = TextureWrapMode.Repeat;

        blitMaterial.SetTexture("_MainTex", textureToTile);
        blitMaterial.SetTexture("_HeightTex", MGS.HeightMap);
        blitMaterial.SetVector("_ObjectScale", objectScale);

        if (SettingsGui.instance.settings.normalMapMayaStyle)
            blitMaterial.SetFloat("_FlipY", 1.0f);
        else
            blitMaterial.SetFloat("_FlipY", 0.0f);

        // Clear the ping pong buffers
        Graphics.Blit(Texture2D.blackTexture, _SplatTemp, blitMaterial, 2);
        Graphics.Blit(Texture2D.blackTexture, _SplatTempAlt, blitMaterial, 2);

        var texARWidth = MGS.HeightMap.width / (float) MGS.HeightMap.height;
        var texARHeight = MGS.HeightMap.height / (float) MGS.HeightMap.width;
        var texAR = Vector2.one;
        if (texARWidth < texARHeight)
            texAR.x = texARWidth;
        else
            texAR.y = texARHeight;

        var targetARWidth = NewTexSizeX / (float) NewTexSizeY;
        var targetARHeight = NewTexSizeY / (float) NewTexSizeX;
        targetAR = Vector2.one;
        if (targetARWidth < targetARHeight)
            targetAR.x = targetARWidth;
        else
            targetAR.y = targetARHeight;

        blitMaterial.SetFloat("_SplatScale", SplatScale);
        blitMaterial.SetVector("_AspectRatio", texAR);
        blitMaterial.SetVector("_TargetAspectRatio", targetAR);

        blitMaterial.SetFloat("_SplatRotation", SplatRotation);
        blitMaterial.SetFloat("_SplatRotationRandom", SplatRotationRandom);

        var isEven = true;
        for (var i = 0; i < splatKernel.Length; i++)
        {
            blitMaterial.SetVector("_SplatKernel", splatKernel[i]);

            var offsetX = Mathf.Sin((SplatRandomize + 1.0f + i) * 128.352f);
            var offsetY = Mathf.Cos((SplatRandomize + 1.0f + i) * 243.767f);
            blitMaterial.SetVector("_Wobble", new Vector3(offsetX, offsetY, SplatWobble));

            blitMaterial.SetFloat("_SplatRandomize", Mathf.Sin((SplatRandomize + 1.0f + i) * 472.361f));

            if (isEven)
            {
                blitMaterial.SetTexture("_TargetTex", _SplatTempAlt);
                Graphics.Blit(textureToTile, _SplatTemp, blitMaterial, 1);
                isEven = false;
            }
            else
            {
                blitMaterial.SetTexture("_TargetTex", _SplatTemp);
                Graphics.Blit(textureToTile, _SplatTempAlt, blitMaterial, 1);
                isEven = true;
            }
        }

        //GameObject.Destroy(transHelper.gameObject);

        if (isEven)
            Graphics.Blit(_SplatTempAlt, textureTarget, blitMaterial, 3);
        else
            Graphics.Blit(_SplatTemp, textureTarget, blitMaterial, 3);

        thisMaterial.SetTexture(TexName, textureTarget);

        CleanupTexture(_SplatTemp);
        CleanupTexture(_SplatTempAlt);

        return textureTarget;
    }


    private RenderTexture TileTextureOverlap(RenderTexture textureToTile, RenderTexture textureTarget, string TexName)
    {
        if (textureTarget != null)
        {
            textureTarget.Release();
            textureTarget = null;
        }

        if (TexName == "_HDDisplacementMap")
        {
            textureTarget = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.RHalf,
                RenderTextureReadWrite.Linear);
            textureTarget.wrapMode = TextureWrapMode.Repeat;
        }
        else
        {
            textureTarget = new RenderTexture(NewTexSizeX, NewTexSizeY, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            textureTarget.wrapMode = TextureWrapMode.Repeat;
        }

        textureTarget.wrapMode = TextureWrapMode.Repeat;

        blitMaterial.SetTexture("_MainTex", textureToTile);

        Graphics.Blit(textureToTile, textureTarget, blitMaterial, 0);

        thisMaterial.SetTexture(TexName, textureTarget);

        return textureTarget;
    }

    private IEnumerator TileTextures()
    {
        Debug.Log("Processing Tile");

        blitMaterial.SetFloat("_Falloff", 1.0f);
        blitMaterial.SetFloat("_Falloff", Falloff);
        blitMaterial.SetFloat("_OverlapX", OverlapX);
        blitMaterial.SetFloat("_OverlapY", OverlapY);

        if (MGS.HeightMap == null)
        {
            yield break;
        }

        blitMaterial.SetTexture("_HeightTex", MGS.HeightMap);
        blitMaterial.SetFloat("_IsHeight", 1.0f);
        _HeightMapTemp = TileTexture(MGS.HeightMap, _HeightMapTemp, "_DisplacementMap");


        if (MGS.HdHeightMap != null)
        {
            blitMaterial.SetFloat("_IsHeight", 1.0f);
            _HDHeightMapTemp = TileTexture(MGS.HdHeightMap, _HDHeightMapTemp, "_HDDisplacementMap");
        }


        blitMaterial.SetFloat("_IsHeight", 0.0f);

        if (MGS.DiffuseMapOriginal != null)
        {
            _DiffuseMapOriginalTemp =
                TileTexture(MGS.DiffuseMapOriginal, _DiffuseMapOriginalTemp, "_DiffuseMapOriginal");
            thisMaterial.SetTexture("_DiffuseMap", _DiffuseMapOriginalTemp);
        }

        if (MGS.DiffuseMap != null)
        {
            _DiffuseMapTemp = TileTexture(MGS.DiffuseMap, _DiffuseMapTemp, "_DiffuseMap");
            thisMaterial.SetTexture("_DiffuseMap", _DiffuseMapTemp);
        }

        if (MGS.MetallicMap != null)
        {
            _MetallicMapTemp = TileTexture(MGS.MetallicMap, _MetallicMapTemp, "_MetallicMap");
            thisMaterial.SetTexture("_MetallicMap", _MetallicMapTemp);
        }

        if (MGS.SmoothnessMap != null)
        {
            _SmoothnessMapTemp = TileTexture(MGS.SmoothnessMap, _SmoothnessMapTemp, "_SmoothnessMap");
            thisMaterial.SetTexture("_SmoothnessMap", _SmoothnessMapTemp);
        }

        if (MGS.NormalMap != null)
        {
            blitMaterial.SetFloat("_IsNormal", 1.0f);
            _NormalMapTemp = TileTexture(MGS.NormalMap, _NormalMapTemp, "_NormalMap");
            thisMaterial.SetTexture("_NormalMap", _NormalMapTemp);
        }

        blitMaterial.SetFloat("_IsNormal", 0.0f);

        if (MGS.EdgeMap != null)
        {
            _EdgeMapTemp = TileTexture(MGS.EdgeMap, _EdgeMapTemp, "_EdgeMap");
            thisMaterial.SetTexture("_EdgeMap", _EdgeMapTemp);
        }

        if (MGS.AoMap != null)
        {
            _AOMapTemp = TileTexture(MGS.AoMap, _AOMapTemp, "_AOMap");
            thisMaterial.SetTexture("_AOMap", _AOMapTemp);
        }

        Resources.UnloadUnusedAssets();

        yield return new WaitForSeconds(0.1f);
    }

    private enum TileTechnique
    {
        Overlap,
        Splat
    }
}