#region

using UnityEngine;

#endregion

public class CubemapPicker : MonoBehaviour
{
    private static readonly int GlobalCubemap = Shader.PropertyToID("_GlobalCubemap");
    private int _selectedCubemap;
    public Cubemap[] CubeMaps;
    public KeyCode Key;

    private void Update()
    {
        if (Input.GetKeyDown(Key))
        {
            _selectedCubemap += 1;
            if (_selectedCubemap >= CubeMaps.Length) _selectedCubemap = 0;
        }

        Shader.SetGlobalTexture(GlobalCubemap, CubeMaps[_selectedCubemap]);
    }
}