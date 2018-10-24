using UnityEngine;
using System.Collections;

public static class GLUtils {

	//=========================================================================
	static Material glMaterial = null;
	private static Material GetGLMaterial() {
	    if (glMaterial != null) return glMaterial;
        glMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
            "SubShader { Pass { " + 
            "    Blend SrcAlpha OneMinusSrcAlpha " + 
            "    ZWrite Off Cull Off Fog { Mode Off } " + 
            "    BindChannels {" + 
            "      Bind \"vertex\", vertex Bind \"color\", color }" + 
            "} } }" ); 
        return glMaterial;
    }
    
	//=============================================================================
	// Render lines in OpenGL - Call these functions inside a PostRender pass
	// Points in screen coordinates
	public static void RenderLines(Vector2[] points, Color color) {
		RenderLines(points, color, false);
	}
	// disconnectSegments: if true, do not connect the end of the previous segment with the start of the next one
	public static void RenderLines(Vector2[] points, Color color, bool disconnectSegments)
	{
		if (points.Length < 2) return;
		
		GetGLMaterial().SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix();
		
	    GL.Begin(GL.LINES);
	    GL.Color(color);
	    int increment = disconnectSegments ? 2 : 1;
	    for (int i = 0; i < points.Length - 1; i += increment) {
	        GL.Vertex3(points[i].x,   points[i].y  +1.5f, 0);
	        GL.Vertex3(points[i+1].x, points[i+1].y+1.5f, 0);
	    }
	    GL.End();
        GL.PopMatrix();
	}

	//=============================================================================
	// Similar to RenderLines, but make segments in pairs (0-1, 2-3, 4-5, etc)
	public static void RenderSegments(Vector2[] points, Color color) {
		RenderLines(points, color, true);
	}
	
	//=============================================================================
	// Render vertices in OpenGL - Call these functions inside a PostRender pass
	// Points in screen coordinates
	public static void RenderVertices(Vector2[] points, Color color)
	{
		if (points.Length < 1) return;
		
		GetGLMaterial().SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix();
        
	    GL.Begin(GL.LINES);
	    GL.Color(color);
	    for (int i = 0; i < points.Length; i++)
	    {
	    	float x = ((int) points[i].x) + 0.5f;
	    	float y = ((int) points[i].y) + 0.5f;
	    
			Vector2 pos1 = new Vector2(x,    y-0f);
			Vector2 pos2 = new Vector2(x+1f, y+1);
			Vector2 pos3 = new Vector2(x,    y+2f);
			Vector2 pos4 = new Vector2(x-1f, y+1);

	        GL.Vertex(pos1);
	        GL.Vertex(pos2);
	        GL.Vertex(pos2);
	        GL.Vertex(pos3);
	        GL.Vertex(pos3);
	        GL.Vertex(pos4);
	        GL.Vertex(pos4);
	        GL.Vertex(pos1);
	    }
	    GL.End();
        GL.PopMatrix();
	}

	//=============================================================================
	// Renders a rectangle in OpenGL (call on a OnPostRender; screen coords)
	public static void RenderRect(Rect rect, Color bgColor, Color borderColor)
	{
		Vector2 pos1 = new Vector2(rect.xMin, rect.yMin + 1.5f);
		Vector2 pos2 = new Vector2(rect.xMax, rect.yMin + 1.5f);
		Vector2 pos3 = new Vector2(rect.xMax, rect.yMax + 1.5f);
		Vector2 pos4 = new Vector2(rect.xMin, rect.yMax + 1.5f);

		GetGLMaterial().SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix();
        
	    GL.Begin(GL.QUADS);
	    GL.Color(bgColor);
	        GL.Vertex(pos1);
	        GL.Vertex(pos2);
	        GL.Vertex(pos3);
	        GL.Vertex(pos4);
	    GL.End();
	    
	    GL.Begin(GL.LINES);
	    GL.Color(borderColor);
	        GL.Vertex(pos1);
	        GL.Vertex(pos2);
	        GL.Vertex(pos2);
	        GL.Vertex(pos3);
	        GL.Vertex(pos3);
	        GL.Vertex(pos4);
	        pos4.y += 0.5f;
	        GL.Vertex(pos4);
	        GL.Vertex(pos1);
	    GL.End();
	    
        GL.PopMatrix();
	}
	
}
