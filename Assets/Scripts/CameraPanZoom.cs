using UnityEngine;
using System.Collections;

public class CameraPanZoom : MonoBehaviour {

	public FileBrowser fileBrowser;

	Vector3 targetPos;
	float targetFov;

	Vector2 mousePos;
	Vector2 lastMousePos;

	public bool holdKey = false;
	public bool noHoldKey = false;
	public KeyCode[] keyToHold;
	
	int mouseDownCount = 0;
	
	public int MouseButtonPan = 0;

	// Use this for initialization
	void Start () {
	
		targetPos = this.transform.position;

	}
	
	// Update is called once per frame
	void Update () {

		mousePos = Input.mousePosition;
		
		Vector2 mouseOffset = mousePos - lastMousePos;
		
		if (Input.GetMouseButton (MouseButtonPan)) {
			mouseDownCount ++;
		} else {
			mouseDownCount = 0;
		}

		bool keyHeld = false;
		for (int i = 0; i < keyToHold.Length; i++) {
			if (Input.GetKey (keyToHold[i])) {
				keyHeld = true;
			}
		}

		if (noHoldKey && keyHeld == false) {
			if (mouseDownCount > 1) {
				targetPos -= new Vector3 (1, 0, 0) * mouseOffset.x * 0.025f;
				targetPos -= new Vector3 (0, 1, 0) * mouseOffset.y * 0.025f;
			}
		}

        if (fileBrowser)
        {
            if (fileBrowser.Active == false)
            {
                targetPos += new Vector3(0, 0, 1) * Input.GetAxis("Mouse ScrollWheel") * 3.0f;
            }
        }
        else
        {
            targetPos += new Vector3(0, 0, 1) * Input.GetAxis("Mouse ScrollWheel") * 3.0f;
        }


		this.transform.position += ( targetPos - this.transform.position ) * 0.05f;
		
		lastMousePos = mousePos;
	
	}
}
