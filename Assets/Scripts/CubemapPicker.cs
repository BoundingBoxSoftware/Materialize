using UnityEngine;

public class CubemapPicker : MonoBehaviour
{
    public Cubemap[] CubeMaps;
    public KeyCode key;
    private int selectedCubemap;


    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            selectedCubemap += 1;
            if (selectedCubemap >= CubeMaps.Length) selectedCubemap = 0;
        }

        Shader.SetGlobalTexture("_GlobalCubemap", CubeMaps[selectedCubemap]);
    }
}