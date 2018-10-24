using UnityEngine;
using System.Collections;

public class ObjRotator : MonoBehaviour {

	Vector2 mousePos;
	Vector2 lastMousePos;
	Vector3 rotation;
	Vector3 lerpRotation;

	int mouseDownCount = 0;

	public int MouseButton = 0;

	public bool AllowX = true;
	public bool InvertX = false;

	public bool AllowY = true;
	public bool InvertY = false;

	public bool holdKey = false;
	public bool noHoldKey = false;
	public KeyCode keyToHold;

	// Use this for initialization
	void Start () {

		mousePos = Input.mousePosition;
		lastMousePos = mousePos;
		
		rotation = this.transform.eulerAngles;
		lerpRotation = rotation;
	
	}

	public void Reset(){
		rotation = new Vector3(0,0,0);
		lerpRotation = rotation;
		this.transform.eulerAngles = lerpRotation;
	}
	
	// Update is called once per frame
	void Update() {
		
		mousePos = Input.mousePosition;
		
		Vector2 mouseOffset = mousePos - lastMousePos;
		
		if (Input.GetMouseButton (MouseButton)) {
			mouseDownCount ++;
		} else {
			mouseDownCount = 0;
		}

		// skip the first frame because we could just be regaining focus

		if ( ( holdKey && Input.GetKey (keyToHold) ) || holdKey == false ) {

			if (mouseDownCount > 1) {
				if (AllowX) {
					if (InvertX) {
						rotation -= new Vector3 (0, 1, 0) * mouseOffset.x * 0.3f;
					} else {
						rotation += new Vector3 (0, 1, 0) * mouseOffset.x * 0.3f;
					}
				}
				if (AllowY) {
					if (InvertY) {
						rotation -= new Vector3 (1, 0, 0) * mouseOffset.y * 0.3f;
					} else {
						rotation += new Vector3 (1, 0, 0) * mouseOffset.y * 0.3f;
					}
				}
				rotation.x = Mathf.Clamp (rotation.x, -80, 80);
			}

		}
		
		lerpRotation = lerpRotation * 0.95f + rotation * 0.05f;
		this.transform.eulerAngles = lerpRotation;
		
		lastMousePos = mousePos;
		
	}
}
