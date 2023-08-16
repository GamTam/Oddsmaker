using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueFlags[] _dialogues;
    public DialogueSO _dialogue;
    public bool _inspected;

    public void Start()
    {
        UpdateDialogue();
    }

    private void UpdateDialogue()
    {
        foreach (DialogueFlags talk in _dialogues)
        {
            if (Globals.CheckStoryFlags(talk.Flags))
            {
                _dialogue = talk.Dialogue;
                break;
            }
        }

        if (Globals.UsedDialogue.Contains(_dialogue)) _inspected = true;
        else _inspected = false;
    }

    public void TriggerDialogue()
    {
        Globals.DialogueManager.StartText(_dialogue);
        Globals.UsedDialogue.Add(_dialogue);
        UpdateDialogue();
    }
}

[Serializable]
public struct DialogueFlags
{
    public DialogueSO Dialogue;
    public string[] Flags;
}