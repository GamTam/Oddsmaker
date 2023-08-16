using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameTrigger : DialogueTrigger
{
    private CasinoLoadScene _sceneLoader;

    private void Start()
    {
        base.Start();
        _sceneLoader = GetComponent<CasinoLoadScene>();
    }
    
    private void Update()
    {
        if (_talkable && Globals.Player._interacting && !Globals.InBattle)
        {
            _triggeredDialogue = true;
            TriggerDialogue();
        }

        if (_sceneLoader == null) return;
        if (_triggeredDialogue && _playerInput.currentActionMap.name == "Overworld")
        {
            _triggeredDialogue = false;
            _playerInput.SwitchCurrentActionMap("Null");
            StartCoroutine(_sceneLoader.BattleTransition());
        }
    }
}
