using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuCursor : MonoBehaviour
{
    [HideInInspector] public GameObject _selectedButton;
    [HideInInspector] public bool StopUpdating = false;
    protected SoundManager _soundManager;
    protected Animator _anim;
    
    protected void Start()
    {
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        _anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_anim != null)
        {
            _anim.Play("Select Fade In", 0, 0);
        }
    }

    private void OnDisable()
    {
        _selectedButton = null;
    }

    protected void LateUpdate()
    {
        if (StopUpdating) return;
        
        if (EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(_selectedButton);
        transform.position = EventSystem.current.currentSelectedGameObject.transform.position;

        if (_selectedButton != EventSystem.current.currentSelectedGameObject)
        {
            if (_selectedButton != null) _soundManager.Play("select");
            _selectedButton = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void Click()
    {
        _soundManager.Play("confirm");
        _anim.Play("Select Fade Out", 0, 0);
        _selectedButton = EventSystem.current.currentSelectedGameObject;
        _selectedButton.GetComponent<Animator>().Play("Pressed");
    }
}
