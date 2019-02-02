#region

using UnityEngine;

#endregion

public class CopyScale : MonoBehaviour
{
    private static readonly int Parallax = Shader.PropertyToID("_Parallax");
    private static readonly int Tiling1 = Shader.PropertyToID("_Tiling");
    private Renderer _renderer;
    public GameObject TargetObject;

    private void Start()
    {
        _renderer = TargetObject.GetComponent<Renderer>();
    }

    private void Update()
    {
        var tempScale = TargetObject.transform.localScale;
        var targetMaterial = _renderer.sharedMaterial;

        if (targetMaterial.HasProperty("_Parallax"))
        {
            var height = targetMaterial.GetFloat(Parallax);
            var tiling = targetMaterial.GetVector(Tiling1);

            tempScale.z += height * (1.0f / tiling.x);
        }

        transform.localScale = tempScale;
    }
}