using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnemySelectionController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Slider _slider;
    [SerializeField] private RectTransform _transform;

    private Enemy _enemy;
    private BattleManager _battleManager;
    private GameObject _currentSelectedObject;
    
    private int _currentColum;
    private int _currentRow;

    private PlayerInput _input;
    private InputAction _cancel;
    private InputAction _confirm;
    private Vector2 _prevMoveVector;

    private void Start()
    {
        _input = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _battleManager = GameObject.FindWithTag("Battle Manager").GetComponent<BattleManager>();
        _cancel = _input.actions["Cancel"];
        _confirm = _input.actions["Confirm"];
    }

    public void Update()
    {
        if (!gameObject.activeSelf) return;
        
        if (_cancel.triggered)
        {
            _battleManager.Back();
            _battleManager._soundManager.Play("back");
            return;
        }

        if (_confirm.triggered)
        {
            _battleManager._soundManager.Play("confirm");
            _battleManager.Click();
        }
        
        if (_currentSelectedObject == EventSystem.current.currentSelectedGameObject) return;
        
        _currentSelectedObject = EventSystem.current.currentSelectedGameObject;

        try
        {
            SwitchEnemy(EventSystem.current.currentSelectedGameObject.GetComponent<Enemy>());
        }
        catch
        {
        }
    }

    public void SwitchEnemy(Enemy enemy)
    {
        if (_enemy) _enemy._selected = false;
        _currentSelectedObject = enemy.gameObject;
        gameObject.SetActive(true);
        _enemy = enemy;
        Vector2 pos = _enemy._rectTransform.anchoredPosition;
        pos.y += _enemy._rectTransform.sizeDelta.y / 2;
        pos.y += 30;
        
        _transform.anchoredPosition = pos;
        _enemy._selected = true;
        
        _text.SetText(enemy.gameObject.name);
        _slider.maxValue = _enemy._maxHP;
        _slider.value = _enemy._HP;
    }

    public void Disable()
    {
        if (!gameObject.activeSelf) return;
        
        if (_enemy) _enemy._selected = false;
        foreach (Enemy enemy in _battleManager._enemies)
        {
            enemy.GetComponent<Button>().interactable = false;
        }
        gameObject.SetActive(false);
    }
}
