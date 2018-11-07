
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityExtension;

public class OBJFace
{
    //------------------------------------------------------------------------------------------------------------
    private readonly List<OBJFaceVertex> m_Vertices = new List<OBJFaceVertex>();

    //------------------------------------------------------------------------------------------------------------
    public void AddVertex(OBJFaceVertex lVertex)
    {
        m_Vertices.Add(lVertex);
    }

    //------------------------------------------------------------------------------------------------------------
    public void ParseVertex(string lVertexString)
    {
        var fields = lVertexString.Split(new[] { '/' }, StringSplitOptions.None);

        var lIndex = fields[0].ParseInvariantInt();
        var faceVertex = new OBJFaceVertex
        {
            m_VertexIndex = lIndex - 1
        };

        if (fields.Length > 1)
        {
            lIndex = fields[1].Length == 0 ? 0 : fields[1].ParseInvariantInt();
            faceVertex.m_UVIndex = lIndex - 1;
        }

        if (fields.Length > 2)
        {
            lIndex = fields[2].Length == 0 ? 0 : fields[2].ParseInvariantInt();
            faceVertex.m_NormalIndex = lIndex - 1;
        }

        if (fields.Length > 3)
        {
            lIndex = fields[3].Length == 0 ? 0 : fields[3].ParseInvariantInt();
            faceVertex.m_UV2Index = lIndex - 1;
        }

        if (fields.Length > 4)
        {
            lIndex = fields[4].Length == 0 ? 0 : fields[4].ParseInvariantInt();
            faceVertex.m_ColorIndex = lIndex - 1;
        }

        AddVertex(faceVertex);
    }

    //------------------------------------------------------------------------------------------------------------
    public string ToString(int lIndex)
    {
        OBJFaceVertex lFaceVertex = m_Vertices[lIndex];

        string lOutput = (lFaceVertex.m_VertexIndex + 1).ToString();

        if (lFaceVertex.m_UVIndex > -1)
        {
            lOutput += string.Format("/{0}", (lFaceVertex.m_UVIndex + 1).ToString());
        }

        if (lFaceVertex.m_NormalIndex > -1)
        {
            lOutput += string.Format("/{0}", (lFaceVertex.m_NormalIndex + 1).ToString());
        }

        if (lFaceVertex.m_UV2Index > -1)
        {
            lOutput += string.Format("/{0}", (lFaceVertex.m_UV2Index + 1).ToString());
        }

        if (lFaceVertex.m_ColorIndex > -1)
        {
            lOutput += string.Format("/{0}", (lFaceVertex.m_ColorIndex + 1).ToString());
        }

        return lOutput;
    }

    //------------------------------------------------------------------------------------------------------------
    public OBJFaceVertex this[int i]
    {
        get { return m_Vertices[i]; }
    }

    //------------------------------------------------------------------------------------------------------------
    public int Count
    {
        get { return m_Vertices.Count; }
    }
}
