#region

using UnityEngine;

#endregion

public class ObjRotator : MonoBehaviour
{
    private Vector2 _lastMousePos;
    private Vector3 _lerpRotation;

    private int _mouseDownCount;

    private Vector2 _mousePos;
    private Vector3 _rotation;
    public bool AllowX = true;

    public bool AllowY = true;

    public bool HoldKey;
    public bool InvertX;
    public bool InvertY;
    public bool AllowHideUI;

    public KeyCode KeyToHold = KeyCode.L;
    
    public int MouseButton;

    // Use this for initialization
    private void Start()
    {
        _mousePos = Input.mousePosition;
        _lastMousePos = _mousePos;

        _rotation = transform.eulerAngles;
        _lerpRotation = _rotation;
    }

    public void Reset()
    {
        _rotation = new Vector3(0, 0, 0);
        _lerpRotation = _rotation;
        transform.eulerAngles = _lerpRotation;
    }

    private void FixedUpdate()
    {
        if (!MainGui.Instance) return;
        _mousePos = Input.mousePosition;

        var mouseOffset = _mousePos - _lastMousePos;

        if (Input.GetMouseButton(MouseButton))
            _mouseDownCount++;
        else
            _mouseDownCount = 0;

        // skip the first frame because we could just be regaining focus

        if ((HoldKey && Input.GetKey(KeyToHold) || HoldKey == false) && _mouseDownCount > 1)
        {
            if (AllowX)
            {
                if (InvertX)
                    _rotation -= new Vector3(0, 1, 0) * mouseOffset.x * 0.3f;
                else
                    _rotation += new Vector3(0, 1, 0) * mouseOffset.x * 0.3f;
            }

            if (AllowY)
            {
                if (InvertY)
                    _rotation -= new Vector3(1, 0, 0) * mouseOffset.y * 0.3f;
                else
                    _rotation += new Vector3(1, 0, 0) * mouseOffset.y * 0.3f;
            }

            _rotation.x = Mathf.Clamp(_rotation.x, -80, 80);
            if(AllowHideUI) MainGui.Instance.SaveHideStateAndHideAndLock(this);
        }
        else
        {
            MainGui.Instance.HideGuiLocker.Unlock(this);
        }


        _lerpRotation = _lerpRotation * 0.95f + _rotation * 0.05f;
        transform.eulerAngles = _lerpRotation;

        _lastMousePos = _mousePos;
    }
}