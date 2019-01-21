using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class SampleProbeTex : MonoBehaviour
{
    public ReflectionProbe sampleProbe;
    public Material skyboxMaterial;

    // Use this for initialization
    private void Start()
    {
        sampleProbe.RenderProbe();
        //StartCoroutine (SwitchCubemaps ());
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat("_UseProbeTexture", 0);
        Shader.EnableKeyword("_USE_BAKED_CUBEMAP_ON");
        Shader.DisableKeyword("_USE_BAKED_CUBEMAP_OFF");
    }

    private IEnumerator SwitchCubemaps()
    {
        yield return new WaitForSeconds(1);
        for (var i = 1; i <= 100; i++)
        {
            Shader.SetGlobalFloat("_UseProbeTexture", i / 100.0f);
            yield return new WaitForSeconds(0.01f);
        }

        Shader.EnableKeyword("_USE_BAKED_CUBEMAP_OFF");
        Shader.DisableKeyword("_USE_BAKED_CUBEMAP_ON");
    }

    // Update is called once per frame
    private void Update()
    {
        skyboxMaterial.SetTexture("_Tex", sampleProbe.texture);
        Shader.SetGlobalTexture("_ProbeCubemap", sampleProbe.texture);
    }
}