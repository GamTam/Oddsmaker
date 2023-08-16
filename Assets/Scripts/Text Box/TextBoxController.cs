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
    [SerializeField] private GameObject _nameTag;
    [SerializeField] private GameObject _advanceButton;

    private Vector3 _camOffset;

    public bool IsHidden;

    public void Start()
    {
        _camOffset = _text.transform.position - Camera.main.transform.position;
    }

    public void LateUpdate()
    {
        _text.transform.position = Camera.main.transform.position + _camOffset;
    }

    public void HideAll()
    {
        _text.alpha = 0;
        _box.enabled = false;
        _nameTag.SetActive(false);
        _advanceButton.SetActive(false);
        IsHidden = true;
    }

    public void ShowAll()
    {
        _text.alpha = 1;
        _box.enabled = true;
        _nameTag.SetActive(true);
        _advanceButton.SetActive(true);
        IsHidden = false;
    }
}
