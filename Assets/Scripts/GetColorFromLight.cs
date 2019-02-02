#region

using UnityEngine;

#endregion

public class GetColorFromLight : MonoBehaviour
{
    private static readonly int Color = Shader.PropertyToID("_Color");
    private Light _thisLight;

    private Material _thisMaterial;
    public GameObject LightObject;

    private void Start()
    {
        _thisLight = LightObject.GetComponent<Light>();
        _thisMaterial = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        _thisMaterial.SetColor(Color, _thisLight.color);
    }
}