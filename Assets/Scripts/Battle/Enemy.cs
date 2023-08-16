using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Battleable
{
    [SerializeField] private EnemySO _baseEnemy;
    [SerializeField] private Image[] _image;

    public bool _selected;
    public RectTransform _rectTransform;
    
    [HideInInspector] public string[] _description;
    float fade = 1;
    [HideInInspector] public bool _killable;
    [HideInInspector] public Vector3 _slidePoint;
    
    void Awake()
    {
        _name = _baseEnemy.Name;
        _level = _baseEnemy.Level;
        _HP = _baseEnemy.HP;
        _maxHP = _baseEnemy.HP;
        _pow = _baseEnemy.Pow;
        _def = _baseEnemy.Def;
        _speed = _baseEnemy.Speed;
        _description = _baseEnemy.Description;
        _exp = _baseEnemy.EXP;

        _image = GetComponentsInChildren<Image>();

        _mat = new Material(_image[0].material);
        foreach (var image in _image)
        {
            image.material = _mat;
        }
       
        _slider.maxValue = _maxHP;
        _redSliders[0].maxValue = _maxHP;
        _redSliders[0].value = _maxHP;
        _slider.gameObject.transform.SetParent(gameObject.transform.parent.parent);
        
        StartingLocation = transform.localPosition;
        transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + 1080f);
        _slidePoint = transform.localPosition;
    }

    private void LateUpdate()
    {
        _slider.value = _HP;
        
        if (_selected) FlashWhite();
        else
        {
            _timer = 0;
            _mat.SetColor("_Color_2", Color.white);
        }
        
        if (_killable) {
            fade -= Time.deltaTime;
            if (fade <= 0f) {
                fade = 0;
                gameObject.SetActive(false);
            }

            _mat.SetFloat("_Fade", fade);
        }
    }
}
