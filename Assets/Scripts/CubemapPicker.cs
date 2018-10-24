using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubemapPicker : MonoBehaviour {

    public Cubemap[] CubeMaps;
    int selectedCubemap = 0;
    public KeyCode key;


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(key))
        {
            selectedCubemap += 1;
            if (selectedCubemap >= CubeMaps.Length)
            {
                selectedCubemap = 0;
            }
        }

        Shader.SetGlobalTexture("_GlobalCubemap", CubeMaps[selectedCubemap]);

    }
}
