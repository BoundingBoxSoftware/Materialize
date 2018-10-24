using UnityEngine;
using System.Collections;
//[ExecuteInEditMode]
public class MotionBlurMatrixHelper : MonoBehaviour
{
	//private Matrix4x4 lastModelView = Matrix4x4.identity;
	private Matrix4x4 lastModel = Matrix4x4.identity;
	private Renderer thisRenderer;
	public Material[] materials;
	
	private SkinnedMeshRenderer thisSMR;
	private Mesh sharedMesh;
	private Mesh lastMesh;
	private bool isSkinnedMesh = false;
	Vector3[] lastPositions;
	
	public bool useSharedMaterials = false;
	public bool updateMaterialsEveryFrame = false;
	public bool updateRendererEveryFrame = false;

	 
	
	void Start() {
	
		thisRenderer = this.GetComponent<Renderer>();
		if( useSharedMaterials ){
			materials = thisRenderer.sharedMaterials;
		}else{
			materials = thisRenderer.materials;
		}
		
		if( GetComponent<Renderer>() is SkinnedMeshRenderer )
		{
			//Debug.Log ( "Is Skinned Mesh Renderer" );
			isSkinnedMesh = true;
			thisSMR = GetComponent<SkinnedMeshRenderer>();
			sharedMesh = (Mesh) Instantiate(thisSMR.sharedMesh);
			thisSMR.sharedMesh = sharedMesh;
			lastMesh = new Mesh();
			thisSMR.BakeMesh( lastMesh );
			lastPositions = lastMesh.vertices;
		}
		
		lastModel = calculateModelMatrix();
	}
	
	void OnWillRenderObject()
	//void LateUpdate()
	{
		if (updateRendererEveryFrame) {
			thisRenderer = this.GetComponent<Renderer> ();
		}

		if (updateMaterialsEveryFrame) {
			if( useSharedMaterials ){
				materials = thisRenderer.sharedMaterials;
			}else{
				materials = thisRenderer.materials;
			}
		}

		if( Camera.current.tag != "MainCamera" ) return;
		//Debug.Log ( "Will Render Object" );
		
		for( int i = 0; i < materials.Length; i ++ )
		{
			materials[i].SetInt( "_IsMover", 1 );
			materials[i].SetMatrix("_LAST_MODEL_MATRIX", lastModel );
		}
		
		lastModel = calculateModelMatrix();
	}
	
	Matrix4x4 calculateModelMatrix()
	{
		//Debug.Log ( "Calculating Model Matrix" );
		if( isSkinnedMesh )
		{

			thisSMR.BakeMesh( lastMesh );
			
			Vector3[] thisPositions = lastMesh.vertices;
			Vector2[] newUV2 = new Vector2[lastMesh.vertexCount];
			Vector2[] newUV3 = new Vector2[lastMesh.vertexCount];
			
			for( int i = 0; i < lastMesh.vertexCount; i++ ){
				newUV2[i] = new Vector2( thisPositions[i].x - lastPositions[i].x, thisPositions[i].y - lastPositions[i].y );
				newUV3[i] = new Vector2( thisPositions[i].z - lastPositions[i].z, 1.0f );
			}
			
			sharedMesh.uv2 = newUV2;
			sharedMesh.uv3 = newUV3;
			
			lastPositions = thisPositions;
			
			Transform rootBone = ((SkinnedMeshRenderer)GetComponent<Renderer>()).rootBone;
			return Matrix4x4.TRS( rootBone.position, rootBone.rotation, Vector3.one );
			
		}
		
		if( thisRenderer.isPartOfStaticBatch )
		{
			return Matrix4x4.identity;
		}
		
		return Matrix4x4.TRS( transform.position, transform.rotation, calculateScale() );
	}
	
	Vector3 calculateScale()
	{
		Vector3 scale = Vector3.one;
		
		// the model is uniformly scaled, so we'll use localScale in the model matrix
		if( transform.localScale == Vector3.one * transform.localScale.x )
		{
			scale = transform.localScale;
		}

		scale = transform.localScale;
		
		// recursively multiply scale by each parent up the chain
		Transform parent = transform.parent;
		while( parent != null )
		{
			scale = new Vector3( scale.x * parent.localScale.x, scale.y * parent.localScale.y, scale.z * parent.localScale.z );
			parent = parent.parent;
		}
		return scale;
	}
}