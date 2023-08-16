using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ControlFlagController : MonoBehaviour
{
    [SerializeField] private List<GameObject> _buttons;

    [SerializeField] private Color _textColor;
    
    private float _speedVector = 0.275f;
    [HideInInspector] public List<string> _currentButtons = new List<string>();
    [HideInInspector] private List<string> _targetButtons = new List<string>();

    private bool _break;
    public bool _doneRotating = true;
    public bool IsHidden;

    public void Start()
    {
        if (Globals.ControlFlag != null)
        {
            Destroy(gameObject);
            return;
        }
        Globals.ControlFlag = this;
    }

    private void LateUpdate()
    {
        if (Globals.UI != UITypes.PWAAT) Hide();
    }

    public void Hide()
    {
        _textColor.a = 0;
        foreach (GameObject obj in _buttons)
        {
            obj.SetActive(false);
        }
        IsHidden = true;
    }

    public void Show()
    {
        if (Globals.UI != UITypes.PWAAT) return;
        _textColor.a = 1;
        foreach (GameObject obj in _buttons)
        {
            if (_currentButtons.Contains(obj.name)) obj.SetActive(true);
        }
        IsHidden = false;
    }

    public void SetText(string[] activeButtons, bool skipOut=false)
    {
        StopAllCoroutines();
        
        StartCoroutine(SetTextCoroutine(activeButtons, skipOut));
    }

    private IEnumerator SetTextCoroutine(string[] activeButtons, bool skipOut)
    {
        if (_currentButtons.SequenceEqual(activeButtons))
        {
            if (_doneRotating)
            {
                transform.localScale = new Vector3(1, 1, 1);
                yield break;
            }
        }
        
        float duration = _speedVector;
        float time = (1 - transform.localScale.y) * duration;

        _doneRotating = false;
        if (!skipOut)
        {
            while (time < duration)
            {
                yield return null;
                time += Time.deltaTime;
                transform.localScale = new Vector3(1, Mathf.Lerp(1, 0, time / duration));
            }
        }

        if (!_currentButtons.SequenceEqual(activeButtons)) transform.localScale = new Vector3(1, 0, 1);

        if (!IsHidden)
            foreach (GameObject obj in _buttons) obj.SetActive(activeButtons.Contains(obj.name));

        _currentButtons = activeButtons.ToList();

        time = transform.localScale.y * duration;
        
        while (time < duration)
        {
            yield return null;
            time += Time.deltaTime;
            transform.localScale = new Vector3(1, Mathf.Lerp(0, 1, time / duration));
        }
        transform.localScale = new Vector3(1, 1, 1);
        _doneRotating = true;
    }
}
