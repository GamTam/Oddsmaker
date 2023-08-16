using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _box;
    [SerializeField] private Image[] _spritesForTransparency;
    [SerializeField] private Image _outlineSprite;
    [SerializeField] private GameObject _nameTag;
    [SerializeField] private Image _nameTagOutline;
    [SerializeField] private GameObject[] _boxCorner;
    [SerializeField] private GameObject _advanceButton;
    [SerializeField] private bool _ignoreTransparency;
    
    private Vector3 _camOffset;
    
    public int MaxLines;
    public bool IsHidden;
    public TMP_Text Text => _text;

    public void Start()
    {
        _camOffset = _text.transform.position - Camera.main.transform.position;
        SetTextBoxTransparency();
    }

    public void SetTextBoxTransparency()
    {
        if (_ignoreTransparency) return;
        
        float transparency = 1f;

        switch (Globals.TextboxTransparency)
        {
            case TextBoxTransparency.High:
                transparency = 200f / 255f;
                break;
            case TextBoxTransparency.Med:
                transparency = 230f / 255f;
                break;
        }

        _box.color = new Color(1, 1, 1, transparency);
        foreach (Image img in _spritesForTransparency)
        {
            img.color = new Color(1, 1, 1, transparency);
        }
    }

    public void LateUpdate()
    {
        _text.transform.position = Camera.main.transform.position + _camOffset;
        
        if (_boxCorner == null) return;
        if (!_nameTag.activeSelf && !IsHidden)
        {
            foreach (GameObject corner in _boxCorner)
            {
                corner.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject corner in _boxCorner)
            {
                corner.SetActive(false);
            }
        }
    }

    public void HideAll()
    {
        _text.alpha = 0;
        _box.enabled = false;
        if (_outlineSprite != null) _outlineSprite.enabled = false;
        if (_nameTagOutline != null) _nameTagOutline.enabled = false;
        _nameTag.SetActive(false);
        _advanceButton.SetActive(false);
        IsHidden = true;
    }

    public void ShowAll()
    {
        _text.alpha = 1;
        _box.enabled = true;
        if (_outlineSprite != null) _outlineSprite.enabled = true;
        if (_nameTagOutline != null) _nameTagOutline.enabled = true;
        _nameTag.SetActive(true);
        _advanceButton.SetActive(true);
        IsHidden = false;
    }
}

public enum TextBoxTransparency
{
    None,
    Med,
    High
}
