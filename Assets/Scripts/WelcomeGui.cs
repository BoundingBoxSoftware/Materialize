using System.Collections;
using UnityEngine;

public class WelcomeGui : MonoBehaviour
{
    public Texture2D background;


    private float backgroundFade = 1.0f;
    public GameObject CommandListExecutorObject;
    public GameObject ControlsGuiObject;

    public Texture2D logo;
    private float logoFade = 1.0f;

    public GameObject MainGuiObject;
    public GameObject SettingsGuiObject;

    public bool skipWelcomeScreen;
    public Cubemap startCubeMap;
    public GameObject testObject;

    // Use this for initialization
    private void Start()
    {
        Application.runInBackground = true;

        var backgroundFade = 1.0f;
        var logoFade = 0.0f;

        if (skipWelcomeScreen || Application.isEditor)
        {
            ActivateObjects();
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(Intro());
        }

        Shader.SetGlobalTexture("_GlobalCubemap", startCubeMap);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void ActivateObjects()
    {
        testObject.SetActive(true);
        MainGuiObject.SetActive(true);
        SettingsGuiObject.SetActive(true);
        ControlsGuiObject.SetActive(true);
        CommandListExecutorObject.SetActive(true);
    }

    private void OnGUI()
    {
        GUI.color = new Color(1, 1, 1, backgroundFade);

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);

        var logoWidth = Mathf.FloorToInt(Screen.width * 0.75f);
        var logoHeight = Mathf.FloorToInt(logoWidth * 0.5f);
        var logoPosX = Mathf.FloorToInt(Screen.width * 0.5f - logoWidth * 0.5f);
        var logoPosY = Mathf.FloorToInt(Screen.height * 0.5f - logoHeight * 0.5f);

        GUI.color = new Color(1, 1, 1, logoFade);

        GUI.DrawTexture(new Rect(logoPosX, logoPosY, logoWidth, logoHeight), logo);
    }

    private IEnumerator FadeLogo(float target, float overTime)
    {
        var timer = overTime;
        var original = logoFade;

        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            logoFade = Mathf.Lerp(target, original, timer / overTime);
            yield return new WaitForEndOfFrame();
        }

        logoFade = target;

        //yield return new WaitForEndOfFrame();
    }

    private IEnumerator FadeBackground(float target, float overTime)
    {
        var timer = overTime;
        var original = backgroundFade;

        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            backgroundFade = Mathf.Lerp(target, original, timer / overTime);
            yield return new WaitForEndOfFrame();
        }

        backgroundFade = target;

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