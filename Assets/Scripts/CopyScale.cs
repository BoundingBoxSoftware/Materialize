using UnityEngine;

public class CopyScale : MonoBehaviour
{
    public GameObject targetObject;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        var tempScale = targetObject.transform.localScale;
        var targetMaterial = targetObject.GetComponent<Renderer>().sharedMaterial;

        if (targetMaterial.HasProperty("_Parallax"))
        {
            var Height = targetMaterial.GetFloat("_Parallax");
            var Tiling = targetMaterial.GetVector("_Tiling");

            tempScale.z += Height * (1.0f / Tiling.x);
        }

        transform.localScale = tempScale;
    }
}