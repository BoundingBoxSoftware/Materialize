using UnityEngine;
using System.Collections;

public class CopyScale : MonoBehaviour {

	public GameObject targetObject;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 tempScale = targetObject.transform.localScale;
		Material targetMaterial = targetObject.GetComponent<Renderer> ().sharedMaterial;

		if ( targetMaterial.HasProperty( "_Parallax" ) ) {
			float Height = targetMaterial.GetFloat ("_Parallax");
			Vector4 Tiling = targetMaterial.GetVector ("_Tiling");

			tempScale.z += Height * ( 1.0f / Tiling.x );
		}

		this.transform.localScale = tempScale;
	}
}
