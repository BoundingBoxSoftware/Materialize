using UnityEngine;

public class ObjRotator : MonoBehaviour
{
    public bool AllowX = true;

    public bool AllowY = true;

    public bool holdKey;
    public bool InvertX;
    public bool InvertY;
    public KeyCode keyToHold;
    private Vector2 lastMousePos;
    private Vector3 lerpRotation;

    public int MouseButton;

    private int mouseDownCount;

    private Vector2 mousePos;
    public bool noHoldKey;
    private Vector3 rotation;

    // Use this for initialization
    private void Start()
    {
        mousePos = Input.mousePosition;
        lastMousePos = mousePos;

        rotation = transform.eulerAngles;
        lerpRotation = rotation;
    }

    public void Reset()
    {
        rotation = new Vector3(0, 0, 0);
        lerpRotation = rotation;
        transform.eulerAngles = lerpRotation;
    }

    // Update is called once per frame
    private void Update()
    {
        mousePos = Input.mousePosition;

        var mouseOffset = mousePos - lastMousePos;

        if (Input.GetMouseButton(MouseButton))
            mouseDownCount++;
        else
            mouseDownCount = 0;

        // skip the first frame because we could just be regaining focus

        if (holdKey && Input.GetKey(keyToHold) || holdKey == false)
            if (mouseDownCount > 1)
            {
                if (AllowX)
                {
                    if (InvertX)
                        rotation -= new Vector3(0, 1, 0) * mouseOffset.x * 0.3f;
                    else
                        rotation += new Vector3(0, 1, 0) * mouseOffset.x * 0.3f;
                }

                if (AllowY)
                {
                    if (InvertY)
                        rotation -= new Vector3(1, 0, 0) * mouseOffset.y * 0.3f;
                    else
                        rotation += new Vector3(1, 0, 0) * mouseOffset.y * 0.3f;
                }

                rotation.x = Mathf.Clamp(rotation.x, -80, 80);
            }

        lerpRotation = lerpRotation * 0.95f + rotation * 0.05f;
        transform.eulerAngles = lerpRotation;

        lastMousePos = mousePos;
    }
}