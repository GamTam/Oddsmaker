using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TextSelection : MonoBehaviour
{
    public TMP_Text _buttonPrefab;
    [SerializeField] private GameObject _selectionHead;
    [SerializeField] private GameObject _grid;
    [HideInInspector] public GameObject _currentSelectedObject;
    private BattleManager _battleManager;
    
    public List<KeyValuePair<TMP_Text, bool>> _buttons = new List<KeyValuePair<TMP_Text, bool>>();

    private int _currentColum;
    private int _currentRow;

    private PlayerInput _input;
    private InputAction _cancel;
    private InputAction _confirm;
    private Vector2 _prevMoveVector;

    private bool _frozen;

    private void Start()
    {
        _input = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _battleManager = GameObject.FindWithTag("Battle Manager").GetComponent<BattleManager>();
        _cancel = _input.actions["Cancel"];
        _confirm = _input.actions["Confirm"];
    }

    public void ResetButtons()
    { 
        foreach (KeyValuePair<TMP_Text, bool> text in _buttons)
        {
            Destroy(text.Key.gameObject);
        }
        
        _currentSelectedObject = null;
        _buttons = new List<KeyValuePair<TMP_Text, bool>>();
    }

    public void AddButton(string text, bool enabled)
    {
        TMP_Text button = Instantiate(_buttonPrefab, _grid.transform);
        
        button.gameObject.SetActive(true);
        button.gameObject.name = text;
        
        _buttons.Add(new KeyValuePair<TMP_Text, bool>(button, enabled));

        button.text = enabled ? text : "<color=grey>" + text;
    }

    public void Update()
    {
        if (_frozen) return;
        
        if (_cancel.triggered)
        {
            _battleManager.Back();
            _battleManager._soundManager.Play("back");
            return;
        }

        if (_confirm.triggered)
        {
            foreach (KeyValuePair<TMP_Text, bool> pair in _buttons)
            {
                if (!(_currentSelectedObject.GetComponent<TMP_Text>() == pair.Key)) continue;
                
                if (!pair.Value)
                {
                    Globals.SoundManager.Play("error");
                    return;
                }
                
                break;
            }
            
            Globals.SoundManager.Play("confirm");
            _battleManager.Click();
            return;
        }
        
        if (_currentSelectedObject == EventSystem.current.currentSelectedGameObject) return;

        if (_buttons.All(btn => EventSystem.current.currentSelectedGameObject != btn.Key.gameObject)) return;
        
        _currentSelectedObject = EventSystem.current.currentSelectedGameObject;

        _selectionHead.transform.position =
            EventSystem.current.currentSelectedGameObject.transform.position;
    }

    public void Freeze()
    {
        _frozen = true;
        foreach (KeyValuePair<TMP_Text, bool> button in _buttons)
        {
            button.Key.GetComponent<Button>().interactable = false;
        }
    }

    public void UnFreeze()
    {
        _frozen = false;
        foreach (KeyValuePair<TMP_Text, bool> button in _buttons)
        {
            button.Key.GetComponent<Button>().interactable = true;
        }
        
        EventSystem.current.SetSelectedGameObject(_currentSelectedObject);
    }
    
    public string GetSelectedButtonText()
    {
        if (_buttons.All(btn => _currentSelectedObject != btn.Key.gameObject)) return "";
        
         return _currentSelectedObject.GetComponent<TMP_Text>().text;
    }
}
