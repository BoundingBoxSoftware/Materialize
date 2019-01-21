using UnityEngine;

public class CameraPanZoom : MonoBehaviour
{
    public bool holdKey;
    public KeyCode[] keyToHold;
    private Vector2 lastMousePos;

    public int MouseButtonPan;

    private int mouseDownCount;

    private Vector2 mousePos;
    public bool noHoldKey;
    private float targetFov;

    private Vector3 targetPos;

    // Use this for initialization
    private void Start()
    {
        targetPos = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        mousePos = Input.mousePosition;

        var mouseOffset = mousePos - lastMousePos;

        if (Input.GetMouseButton(MouseButtonPan))
            mouseDownCount++;
        else
            mouseDownCount = 0;

        var keyHeld = false;
        for (var i = 0; i < keyToHold.Length; i++)
            if (Input.GetKey(keyToHold[i]))
                keyHeld = true;

        if (noHoldKey && keyHeld == false)
            if (mouseDownCount > 1)
            {
                targetPos -= new Vector3(1, 0, 0) * mouseOffset.x * 0.025f;
                targetPos -= new Vector3(0, 1, 0) * mouseOffset.y * 0.025f;
            }

        //todo: Checar comportamento da roda do mouse
//        if (fileBrowser)
//        {
//            if (fileBrowser.Active == false)
//            {
//                targetPos += new Vector3(0, 0, 1) * Input.GetAxis("Mouse ScrollWheel") * 3.0f;
//            }
//        }
//        else
//        {
//            targetPos += new Vector3(0, 0, 1) * Input.GetAxis("Mouse ScrollWheel") * 3.0f;
//        }
        targetPos += new Vector3(0, 0, 1) * Input.GetAxis("Mouse ScrollWheel") * 3.0f;

        transform.position += (targetPos - transform.position) * 0.05f;

        lastMousePos = mousePos;
    }
}