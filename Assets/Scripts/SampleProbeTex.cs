using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SampleProbeTex : MonoBehaviour {

	public ReflectionProbe sampleProbe;
    public Material skyboxMaterial;

	// Use this for initialization
	void Start () {
		sampleProbe.RenderProbe ();
		//StartCoroutine (SwitchCubemaps ());
	}

	void OnDisable (){
		Shader.SetGlobalFloat ("_UseProbeTexture", 0);
		Shader.EnableKeyword ("_USE_BAKED_CUBEMAP_ON");
		Shader.DisableKeyword ("_USE_BAKED_CUBEMAP_OFF");
	}

	IEnumerator SwitchCubemaps(){
		yield return new WaitForSeconds(1);
		for( int i = 1; i <= 100; i++ ){
			Shader.SetGlobalFloat ("_UseProbeTexture", (float)i / 100.0f);
			yield return new WaitForSeconds(0.01f);
		}
		Shader.EnableKeyword ("_USE_BAKED_CUBEMAP_OFF");
		Shader.DisableKeyword ("_USE_BAKED_CUBEMAP_ON");
	}
	
	// Update is called once per frame
	void Update () {
		
        skyboxMaterial.SetTexture ("_Tex", sampleProbe.texture);
        Shader.SetGlobalTexture ( "_ProbeCubemap", sampleProbe.texture );
	}
}
