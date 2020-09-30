
using System.Collections.Generic;

using UnityEngine;

public class OBJData
{
    //------------------------------------------------------------------------------------------------------------
    public List<Vector3> m_Vertices = new List<Vector3>();
    public List<Vector3> m_Normals = new List<Vector3>();
    public List<Vector2> m_UVs = new List<Vector2>();
    public List<Vector2> m_UV2s = new List<Vector2>();
    public List<Color> m_Colors = new List<Color>();

    //------------------------------------------------------------------------------------------------------------
    public List<OBJMaterial> m_Materials = new List<OBJMaterial>();
    public List<OBJGroup> m_Groups = new List<OBJGroup>();
}