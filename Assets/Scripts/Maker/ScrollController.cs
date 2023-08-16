using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScrollController : MonoBehaviour
{
    private Camera _cam;
    
    private PlayerInput _playerInput;
    private InputAction _middleClick;
    private InputAction _mousePos;
    private InputAction _scrollWheel;

    private Vector2 _prevPos;
    private Vector2 _mouseStartPos;

    private bool _canScroll => _middleClick.ReadValue<float>() > 0.5f;
    private float _scrollDir => _scrollWheel.ReadValue<Vector2>().y;
    
    private void Start()
    {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        
        _playerInput.SwitchCurrentActionMap("Editor");
        
        _middleClick = _playerInput.actions["Editor/EnableScroll"];
        _scrollWheel = _playerInput.actions["Editor/ScrollWheel"];
        _mousePos = _playerInput.actions["Editor/MousePos"];
    }

    private void Update()
    {
        if (_middleClick.triggered) _mouseStartPos = _cam.ScreenToWorldPoint(_mousePos.ReadValue<Vector2>());

        if (_scrollDir < 0) _cam.orthographicSize = Mathf.Min(_cam.orthographicSize + 0.5f, 10f);
        else if (_scrollDir > 0) _cam.orthographicSize = Mathf.Max(_cam.orthographicSize - 0.5f, 3f);

        if (!_canScroll)
        {
            _prevPos = transform.position;
            return;
        }

        Vector2 cursorDelta = (Vector2) _cam.ScreenToWorldPoint(_mousePos.ReadValue<Vector2>()) - _mouseStartPos;
        Vector2 pos = _prevPos;

        pos += cursorDelta;
        
        transform.position =  new Vector3(pos.x, pos.y, 16);
    }
}
