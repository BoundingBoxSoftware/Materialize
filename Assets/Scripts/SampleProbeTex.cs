#region

using UnityEngine;

#endregion

[ExecuteInEditMode]
public class SampleProbeTex : MonoBehaviour
{
    private static readonly int UseProbeTexture = Shader.PropertyToID("_UseProbeTexture");
    private static readonly int Tex = Shader.PropertyToID("_Tex");
    private static readonly int ProbeCubemap = Shader.PropertyToID("_ProbeCubemap");
    public ReflectionProbe SampleProbe;
    public Material SkyboxMaterial;

    private void Start()
    {
        SampleProbe.RenderProbe();
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(UseProbeTexture, 0);
        Shader.EnableKeyword("_USE_BAKED_CUBEMAP_ON");
        Shader.DisableKeyword("_USE_BAKED_CUBEMAP_OFF");
    }

    private void Update()
    {
        SkyboxMaterial.SetTexture(Tex, SampleProbe.texture);
        Shader.SetGlobalTexture(ProbeCubemap, SampleProbe.texture);
    }
}