using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PresentCursor : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _moveSpeed = 3;
    [SerializeField] private float _arriveSpeed = 10;

    [SerializeField] private DialogueTrigger _noClues;

    private Camera _cam;
    private DialogueTrigger _selectedObj;
    private SoundManager _soundManager;

    private SwapCharacters _swap;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private BoxCollider2D _boxCollider;

    private PlayerInput _playerInput;
    private InputAction _mousePos;
    private InputAction _vCursor;
    private InputAction _select;

    private List<Character> _char;

    private float _timer = 1 / 3f;
    private float _timerMax = 1 / 3f;
    private float _zPos = -12.6f;

    private bool _spawned;

    IEnumerator Start()
    {
        _spawned = false;
        _boxCollider.enabled = false;
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        _swap = GameObject.FindWithTag("CharacterPlane").GetComponent<SwapCharacters>();
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        _char = _swap._chars;
        
        transform.position = new Vector3(-11, 0, _zPos);
        _playerInput.SwitchCurrentActionMap("Null");

        _mousePos = _playerInput.actions["Investigation/MousePos"];
        _vCursor = _playerInput.actions["Investigation/MoveVector"];
        _select = _playerInput.actions["Investigation/Present"];

        _selectedObj = _noClues;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();

        while (transform.position != new Vector3(0, 0, _zPos))
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(0, 0, _zPos), _arriveSpeed * Time.deltaTime);
            yield return null;
        }
        
        _playerInput.SwitchCurrentActionMap("Investigation");
        _boxCollider.enabled = true;
        _spawned = true;
    }

    void Update()
    {
        if (!_spawned) return;

        if (_playerInput.currentActionMap.name == "Investigation" && !_spriteRenderer.enabled)
        {
            if (Math.Abs(_timer - _timerMax) < 0.01f) _swap.StartSwap(_char, fadeIn: false);
            
            _timer -= Time.deltaTime;
            if (_timer >= 0)
            {
                _spriteRenderer.enabled = true;
                _timer = _timerMax;
            }
            else
            {
                return;
            }
        }
        
        Cursor.visible = false;
        Vector2 cursorPos;

        if (_playerInput.currentControlScheme == "Mouse")
        {
            cursorPos = _cam.ScreenToWorldPoint(_mousePos.ReadValue<Vector2>());
            transform.position = cursorPos;
        }
        else
        {
            cursorPos = _vCursor.ReadValue<Vector2>();
            _rb.velocity = cursorPos * _moveSpeed;
        }

        if (_select.triggered)
        {
            _selectedObj.TriggerDialogue();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Examinable")
        {
            DialogueTrigger trigger = other.GetComponent<DialogueTrigger>();
            
            _selectedObj = trigger;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Examinable")
        {
            if (_selectedObj == other.GetComponent<DialogueTrigger>())
            {
                _selectedObj = _noClues;
            }
        }
    }
}
