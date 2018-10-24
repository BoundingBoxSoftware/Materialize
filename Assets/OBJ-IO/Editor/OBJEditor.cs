
using System;
using System.IO;
using System.Collections;

using UnityEngine;
using UnityEditor;

using UnityExtension;

public class OBJWindow : EditorWindow
{
	//------------------------------------------------------------------------------------------------------------
	private MeshFilter m_MeshFilter = null; 
	
    //------------------------------------------------------------------------------------------------------------
    [MenuItem("OBJ-IO/OBJ Mesh Exporter")]
    public static void Execute()
    {
        OBJWindow.GetWindow<OBJWindow>();
    }
	
    //------------------------------------------------------------------------------------------------------------
    private void OnGUI()
    {
        m_MeshFilter = (MeshFilter)EditorGUILayout.ObjectField("MeshFilter", m_MeshFilter, typeof(MeshFilter), true);
		
		if (m_MeshFilter != null)
		{
			if (GUILayout.Button("Export OBJ"))
			{
				var lOutputPath = EditorUtility.SaveFilePanel("Save Mesh as OBJ", "", m_MeshFilter.name + ".obj", "obj");
				
				if (File.Exists(lOutputPath))
				{
					File.Delete(lOutputPath);
				}
				
				var lStream = new FileStream(lOutputPath, FileMode.Create);
				var lOBJData = m_MeshFilter.sharedMesh.EncodeOBJ();
				OBJLoader.ExportOBJ(lOBJData, lStream);
				lStream.Close();
			}
		}
		else
		{
			GUILayout.Label("Please provide a MeshFilter");
		}
    }
}
