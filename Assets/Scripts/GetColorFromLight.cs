using UnityEngine;
using System.Collections;

public class GetColorFromLight : MonoBehaviour {

	Material thisMaterial;
	Light thisLight;

	public GameObject lightObject;

	// Use this for initialization
	void Start () {

		thisLight = lightObject.GetComponent<Light> ();
		thisMaterial = this.GetComponent<MeshRenderer> ().material;
	
	}
	
	// Update is called once per frame
	void Update () {

		thisMaterial.SetColor ("_Color", thisLight.color);
	
	}
}
