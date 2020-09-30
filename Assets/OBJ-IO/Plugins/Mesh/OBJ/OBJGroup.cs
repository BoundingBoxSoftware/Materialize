
using System.Collections.Generic;

public class OBJGroup 
{
    //------------------------------------------------------------------------------------------------------------
    private readonly List<OBJFace> m_Faces = new List<OBJFace>();
        
    //------------------------------------------------------------------------------------------------------------
    public OBJGroup(string lName)
    {
        m_Name = lName;
    }

    //------------------------------------------------------------------------------------------------------------
    public string m_Name { get; private set; }
    public OBJMaterial m_Material { get; set; }

    //------------------------------------------------------------------------------------------------------------
    public IList<OBJFace> Faces { get { return m_Faces; } }

    //------------------------------------------------------------------------------------------------------------
    public void AddFace(OBJFace lFace)
    {
        m_Faces.Add(lFace);
    }
}