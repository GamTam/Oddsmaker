using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    private SpriteRenderer _spr;
    public float _speed = 2;
    public DialogueSO _destination;
    private PlayerInput _playerInput;
    public bool _knownLocation;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Vector3 pos = Camera.main.transform.position;
        gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z + 1);
        _spr = GetComponent<SpriteRenderer>();
        try
        {
            _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();

            _playerInput.SwitchCurrentActionMap("Null");
        }
        catch
        {
            
        }

        _spr.color = new Color(_spr.color.r, _spr.color.g, _spr.color.b, 0);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        while (_spr.color.a < 1)
        {
            _spr.color = new Color(_spr.color.r, _spr.color.g, _spr.color.b, _spr.color.a + _speed * Time.deltaTime);
            yield return null;
        }
        _spr.color = Color.black;

        Globals.DialogueManager._swap.KillChars();
        Globals.DialogueManager.StartText(_destination, quickEnd:Globals.UsedDialogue.Contains(_destination));
        yield return null;
        gameObject.transform.position = Camera.main.transform.position;
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeOut()
    {
        while (_spr.color.a > 0)
        {
            _spr.color = new Color(_spr.color.r, _spr.color.g, _spr.color.b, _spr.color.a - _speed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
