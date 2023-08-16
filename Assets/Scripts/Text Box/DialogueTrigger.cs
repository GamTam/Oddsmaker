using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected TMP_FontAsset _font;
    [SerializeField] protected bool _skipTextboxCloseAnimation;
    [TextArea(3, 4)] [SerializeField] private string[] _dialogue;
    [SerializeField] protected bool _forceBottom;

    [HideInInspector] public PlayerInput _playerInput;
    protected bool _triggeredDialogue;
    
    [SerializeField] protected bool _talkable = false;

    protected void Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
    }

    protected void Update()
    {
        if (_talkable && Globals.Player._interacting && !Globals.InBattle)
        {
            _triggeredDialogue = true;
            TriggerDialogue();
        }
    }
    
    public virtual void TriggerDialogue()
    {
        string[] dialogue = (string[]) _dialogue.Clone();
        
        FindObjectOfType<DialogueManager>().StartText(dialogue, gameObject.transform, _spriteRenderer, _skipTextboxCloseAnimation, _forceBottom);
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Debug.Log("a");
            _talkable = true;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Debug.Log("b");
            _talkable = false;
        }
    }
}
