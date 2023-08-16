using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCursor : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _baseSprite;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _selectedAgainSprite;
    [SerializeField] private float _moveSpeed = 3;

    [SerializeField] private DialogueTrigger _noClues;

    private Camera _cam;
    private DialogueTrigger _selectedObj;

    private SwapCharacters _swap;
    [SerializeField] private Rigidbody2D _rb;

    private PlayerInput _playerInput;
    private InputAction _mousePos;
    private InputAction _vCursor;
    private InputAction _select;
    private InputAction _back;
    private InputAction _slide;
    
    [SerializeField] private Character[] _char;
    [SerializeField] private SpriteRenderer _background;

    [SerializeField] private Vector2 _swapPosition;

    private float _timer = 1 / 3f;
    private float _timerMax = 1 / 3f;
    private float _zPos = -12.6f;

    private bool _sliding;
    
    void Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        _swap = GameObject.FindWithTag("CharacterPlane").GetComponent<SwapCharacters>();
        _background = GameObject.FindWithTag("Background").GetComponent<SpriteRenderer>();

        if (_swap._chars.Count > 0)
        {
            _char = _swap._charPrefabs.ToArray();
            _swapPosition = new Vector2(_swap.CharPos.x, _swap.CharPos.y);
            _swap._chars[0].CharOnScreen.transform.position = Vector2.zero;
            _swap.CharPos = _swap._chars[0].CharOnScreen.transform.localPosition;
        }

        transform.position = new Vector3(_cam.transform.position.x, _cam.transform.position.y, _zPos);
        _playerInput.SwitchCurrentActionMap("Investigation");

        _mousePos = _playerInput.actions["Investigation/MousePos"];
        _vCursor = _playerInput.actions["Investigation/MoveVector"];
        _select = _playerInput.actions["Investigation/Select"];
        _back = _playerInput.actions["Investigation/Back"];
        _slide = _playerInput.actions["Investigation/Slide"];

        _selectedObj = _noClues;
        _noClues._dialogue = Globals.NoCluesHere;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_playerInput.currentActionMap.name == "Investigation" && !_spriteRenderer.enabled)
        {
            Globals.ControlFlag.Show();
            Globals.ControlFlag.SetText(WideBackground()
                ? new string[] {"Options", "Slide", "Court Record", "Back"}
                : new string[] {"Options", "Back", "Court Record"}, true);
            
            if (Math.Abs(_timer - _timerMax) < 0.01f) _swap.StartSwap(_char.ToList(), fadeIn: false);
            
            _timer -= Time.deltaTime;
            if (_timer >= 0)
            {
                if (_selectedObj == _noClues)
                {
                    _spriteRenderer.sprite = _baseSprite;
                }
                
                _spriteRenderer.enabled = true;
                _timer = _timerMax;
            }
            else
            {
                return;
            }
        }

        if (_slide.triggered && WideBackground())
        {
            Globals.SoundManager.Play("confirm");
            _playerInput.SwitchCurrentActionMap("Null");
            StartCoroutine(Slide());
            return;
        }
        
        Vector2 cursorPos;

        if (_playerInput.currentControlScheme == "Mouse")
        {
            cursorPos = _cam.ScreenToWorldPoint(_mousePos.ReadValue<Vector2>());
            transform.position =  new Vector3(cursorPos.x, cursorPos.y, _zPos);
        }
        else
        {
            cursorPos = _vCursor.ReadValue<Vector2>();
            _rb.velocity = cursorPos * _moveSpeed;
        }

        if (_select.triggered)
        {
            Globals.SoundManager.Play("confirm");
            Globals.ControlFlag.SetText(new string[] {});

            StartCoroutine(WaitToStartTextbox());
        }

        if (_back.triggered)
        {
            _swap.StartSwap(_char.ToList(), fadeIn:true, pos:_swapPosition);
            _playerInput.SwitchCurrentActionMap("Menu");
            Destroy(gameObject);
            Globals.SoundManager.Play("back");
            Cursor.visible = true;
            GameObject.FindWithTag("UI").transform.Find("InvestigationMasterContainer").gameObject.SetActive(true);
        }
    }

    private IEnumerator Slide()
    {
        _sliding = true;
        _spriteRenderer.sprite = _baseSprite;
        Vector3 bgPos = new Vector3(0, 0, 0);
        float width = _background.sprite.texture.width / 100f;

        float leftSide = (-1 * width * ((int) 0 / 100f)) + (width / 2);
        leftSide = Mathf.Clamp(leftSide, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);

        float rightSide = (-1 * width * (100 / 100f)) + (width / 2);
        rightSide = Mathf.Clamp(rightSide, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);

        GameObject scrollObj = _background.transform.parent.gameObject;

        float startPos = scrollObj.transform.position.x;
        float endPos;

        if (Math.Abs(startPos - leftSide) < 0.1)
        {
            endPos = rightSide;
        }
        else
        {
            endPos = leftSide;
        }

        bool swapped = false;
        
        float timePassed = 0;
        float totalTime = 1f;

        do
        {
            float xPos = Mathf.Lerp(startPos, endPos, timePassed / totalTime);
            
            if (!swapped && timePassed / totalTime > 0.5f)
            {
                _swap.CharPos = new Vector3(endPos * -1, 0);
                swapped = true;
            }

            scrollObj.transform.position =
                new Vector3(xPos, bgPos.y, bgPos.z);

            timePassed += Time.deltaTime;

            yield return null;
        } while (timePassed < totalTime);

        scrollObj.transform.position = new Vector3(endPos, 0, scrollObj.transform.localPosition.z);
        _sliding = false;
        
        if (_selectedObj != _noClues)
        {
            if (_selectedObj._inspected)
            {
                _spriteRenderer.sprite = _selectedAgainSprite;
            }
            else
            {
                _spriteRenderer.sprite = _selectedSprite;
            }
        }
        
        _playerInput.SwitchCurrentActionMap("Investigation");
    }

    private IEnumerator WaitToStartTextbox()
    {
        _playerInput.SwitchCurrentActionMap("Null");
        yield return new WaitForSeconds(0.25f);
        _playerInput.SwitchCurrentActionMap("Investigation");
        _selectedObj.TriggerDialogue();
        _spriteRenderer.enabled = false;
        if (_selectedObj._inspected) _spriteRenderer.sprite = _selectedAgainSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Examinable")
        {
            DialogueTrigger trigger = other.GetComponent<DialogueTrigger>();
            
            _selectedObj = trigger;
            
            if (_sliding) return;
            
            if (trigger._inspected)
            {
                _spriteRenderer.sprite = _selectedAgainSprite;
            }
            else
            {
                _spriteRenderer.sprite = _selectedSprite;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Examinable")
        {
            if (_selectedObj == other.GetComponent<DialogueTrigger>())
            {
                _spriteRenderer.sprite = _baseSprite;
                _selectedObj = _noClues;
            }
        }
    }
    
    private bool WideBackground()
    {
        if (_background.sprite == null) return false;
        return _background.sprite.texture.width > 1920;
    }
}
