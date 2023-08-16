using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragableNode : MonoBehaviour
{
    private Camera _cam;
    
    private PlayerInput _playerInput;
    private InputAction _click;
    private InputAction _mousePos;

    private Vector2 _prevPos;
    private Vector2 _mouseStartPos;

    private bool _startedDragging;
    private bool _clickedMouse;
    private bool _releasedMouse => _click.ReadValue<float>() < 0.5f;

    private List<RaycastResult> _raycastResults;

    private void Start()
    {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        
        _playerInput.SwitchCurrentActionMap("Editor");
        
        _click = _playerInput.actions["Editor/Click"];
        _mousePos = _playerInput.actions["Editor/MousePos"];
    }

    private void Update()
    {
        if (_click.triggered) _mouseStartPos = _cam.ScreenToWorldPoint(_mousePos.ReadValue<Vector2>());

        if (_releasedMouse) _startedDragging = false;

        if (!Dragged() && !_startedDragging)
        {
            _prevPos = transform.position;
            return;
        }

        _startedDragging = true;
        
        transform.SetSiblingIndex(transform.parent.GetSiblingIndex());

        Vector2 cursorDelta = (Vector2) _cam.ScreenToWorldPoint(_mousePos.ReadValue<Vector2>()) - _mouseStartPos;
        Vector2 pos = _prevPos;

        pos += cursorDelta;
        
        transform.position =  new Vector3(pos.x, pos.y, 16);
    }

    private bool Dragged()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = _mousePos.ReadValue<Vector2>();

        if (_releasedMouse) {
            _raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, _raycastResults);
        }

        if (_raycastResults.Count == 0) return false;

        return _raycastResults[0].gameObject == gameObject && _click.ReadValue<float>() > 0.5f;
    }
}
