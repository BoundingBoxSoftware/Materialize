﻿#region

using UnityEngine;

#endregion

public class CameraPanZoom : MonoBehaviour
{
    private Vector2 _lastMousePos;

    private Vector2 _mousePos;
    private float _targetFov;

    private Vector3 _targetPos;
    public KeyCode[] KeyToHold;

    public int MouseButtonPan;

    // Use this for initialization
    private void Start()
    {
        _targetPos = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!MainGui.Instance) return;
        _mousePos = Input.mousePosition;

        var mouseOffset = _mousePos - _lastMousePos;

        var keyHeld = false;

        foreach (var t in KeyToHold)
        {
            if (Input.GetKey(t))
            {
                keyHeld = true;
            }
        }

        var mouseDown = Input.GetMouseButton(MouseButtonPan);
        if ((KeyToHold.Length > 0 && keyHeld || KeyToHold.Length == 0) && mouseDown)
        {
            MainGui.Instance.SaveHideStateAndHideAndLock(this);

            _targetPos -= new Vector3(1, 0, 0) * mouseOffset.x * 0.025f;
            _targetPos -= new Vector3(0, 1, 0) * mouseOffset.y * 0.025f;
        }
        else
        {
            MainGui.Instance.HideGuiLocker.Unlock(this);
        }

        _targetPos += new Vector3(0, 0, 1) * Input.GetAxis("Mouse ScrollWheel") * 3.0f;

        var trf = transform;
        var position = trf.position;
        position += (_targetPos - position) * 0.05f;
        trf.position = position;

        _lastMousePos = _mousePos;
    }
}