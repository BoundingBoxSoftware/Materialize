#region

using System.Collections;
using UnityEngine;

#endregion

public class WelcomeGui : MonoBehaviour
{
    private static readonly int GlobalCubemap = Shader.PropertyToID("_GlobalCubemap");


    private float _backgroundFade = 1.0f;
    private float _logoFade = 1.0f;
    public Texture2D Background;
    public GameObject CommandListExecutorObject;
    public GameObject ControlsGuiObject;

    public Texture2D Logo;

    public GameObject MainGuiObject;
    public GameObject SettingsGuiObject;

    public bool SkipWelcomeScreen;
    public Cubemap StartCubeMap;
    public GameObject TestObject;

    private void Start()
    {
        Application.runInBackground = true;
        if (PlayerPrefs.HasKey("targetFrameRate"))
        {
            Application.targetFrameRate = PlayerPrefs.GetInt("targetFrameRate");
            QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
        }
        if (SkipWelcomeScreen || Application.isEditor)
        {
            ActivateObjects();
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(Intro());
        }

        Shader.SetGlobalTexture(GlobalCubemap, StartCubeMap);
    }


    private void ActivateObjects()
    {
        TestObject.SetActive(true);
        MainGuiObject.SetActive(true);
        SettingsGuiObject.SetActive(true);
        ControlsGuiObject.SetActive(true);
        CommandListExecutorObject.SetActive(true);
    }

    private void OnGUI()
    {
        GUI.color = new Color(1, 1, 1, _backgroundFade);

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Background);

        var logoWidth = Mathf.FloorToInt(Screen.width * 0.75f);
        var logoHeight = Mathf.FloorToInt(logoWidth * 0.5f);
        var logoPosX = Mathf.FloorToInt(Screen.width * 0.5f - logoWidth * 0.5f);
        var logoPosY = Mathf.FloorToInt(Screen.height * 0.5f - logoHeight * 0.5f);

        GUI.color = new Color(1, 1, 1, _logoFade);

        GUI.DrawTexture(new Rect(logoPosX, logoPosY, logoWidth, logoHeight), Logo);
    }

    private IEnumerator FadeLogo(float target, float overTime)
    {
        var timer = overTime;
        var original = _logoFade;

        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            _logoFade = Mathf.Lerp(target, original, timer / overTime);
            yield return new WaitForEndOfFrame();
        }

        _logoFade = target;

        //yield return new WaitForEndOfFrame();
    }

    private IEnumerator FadeBackground(float target, float overTime)
    {
        var timer = overTime;
        var original = _backgroundFade;

        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            _backgroundFade = Mathf.Lerp(target, original, timer / overTime);
            yield return new WaitForEndOfFrame();
        }

        _backgroundFade = target;

        gameObject.SetActive(false);
    }

    private IEnumerator Intro()
    {
        StartCoroutine(FadeLogo(1.0f, 0.5f));

        yield return new WaitForSeconds(3.0f);

        StartCoroutine(FadeLogo(0.0f, 1.0f));

        yield return new WaitForSeconds(1.0f);

        StartCoroutine(FadeBackground(0.0f, 1.0f));

        ActivateObjects();
    }
}