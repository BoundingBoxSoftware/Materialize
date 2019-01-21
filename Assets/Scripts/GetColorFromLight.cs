using UnityEngine;

public class GetColorFromLight : MonoBehaviour
{
    public GameObject lightObject;
    private Light thisLight;

    private Material thisMaterial;

    // Use this for initialization
    private void Start()
    {
        thisLight = lightObject.GetComponent<Light>();
        thisMaterial = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    private void Update()
    {
        thisMaterial.SetColor("_Color", thisLight.color);
    }
}