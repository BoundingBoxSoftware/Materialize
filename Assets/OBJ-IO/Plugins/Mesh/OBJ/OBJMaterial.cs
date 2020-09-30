
using UnityEngine;

public class OBJMaterial
{
    //------------------------------------------------------------------------------------------------------------
    public OBJMaterial(string lMaterialName)
    {
        m_Name = lMaterialName;
    }

    //------------------------------------------------------------------------------------------------------------
    public string m_Name;

    //------------------------------------------------------------------------------------------------------------
    public Color m_AmbientColor;
    public Color m_DiffuseColor;
    public Color m_SpecularColor;
    public float m_SpecularCoefficient;

    //------------------------------------------------------------------------------------------------------------
    public float m_Transparency;

    //------------------------------------------------------------------------------------------------------------
    public int m_IlluminationModel;

    //------------------------------------------------------------------------------------------------------------
    public string m_AmbientTextureMap;
    public string m_DiffuseTextureMap;

    //------------------------------------------------------------------------------------------------------------
    public string m_SpecularTextureMap;
    public string m_SpecularHighlightTextureMap;

    //------------------------------------------------------------------------------------------------------------
    public string m_BumpMap;
    public string m_DisplacementMap;
    public string m_StencilDecalMap;

    //------------------------------------------------------------------------------------------------------------
    public string m_AlphaTextureMap;
}