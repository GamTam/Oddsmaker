using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CRPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CourtRecordController _cr;

    private DialogueManager _dialogueManager;
    
    private PlayerInput _playerInput;

    private InputAction _back;
    private InputAction _profiles;
    private InputAction _present;
    
    void Start()
    {
        _text.SetText("<sprite=\"Keyboard\" name=\"E\">Present   <sprite=\"Keyboard\" name=\"R\">Profiles");
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _dialogueManager = GameObject.FindWithTag("Dialogue Manager").GetComponent<DialogueManager>();
        
        _back = _playerInput.actions["Menu/Cancel"];
        _present = _playerInput.actions["Menu/Present"];
        _profiles = _playerInput.actions["Menu/Profile"];
    }
    
    void Update()
    {
        if (_present.triggered)
        {
            _playerInput.SwitchCurrentActionMap("Textbox");
            _cr.HasPresented += StartPresent;
            _cr.Present();
        }
        
        if (_profiles.triggered)
        {
            _cr.ProfileEvidenceSwap();
        }
    }
    
    public void SetControlLabel(bool evidence)
    {
        if (evidence)
        {
            _text.SetText("<sprite=\"Keyboard\" name=\"E\">Present   <sprite=\"Keyboard\" name=\"R\">Profiles");
            return;
        }
        
        _text.SetText("<sprite=\"Keyboard\" name=\"E\">Present   <sprite=\"Keyboard\" name=\"R\">Evidence");
    }

    private void StartPresent(EvidenceSO evidence)
    {
        StartCoroutine(Present(evidence));
    }
    
    private IEnumerator Present(EvidenceSO evidence)
    {
        DialogueSO dialogue = _dialogueManager._dialogue.wrongPresentSequence;
        foreach (InvestigationMenu.EvidenceTalkPair pair in _dialogueManager._dialogue.evidence)
        {
            if (pair.Evidence == evidence && Globals.CheckStoryFlags(pair.Conditions))
            {
                dialogue = pair.Dialogue;
                break;
            }
        }
        
        if (dialogue.dialogueText[0].Interjection == Interjection.NA) Globals.SoundManager.Play("confirm");

        yield return new WaitForSeconds(0.1f);
        
        if (dialogue.dialogueText[0].Interjection != Interjection.NA) _cr._base.transform.localScale = Vector3.zero;
        
        _cr.Close(false, false);

        if (dialogue.dialogueText[0].Interjection == Interjection.NA) yield return new WaitUntil(() => _cr.enabled == false);
        
        _dialogueManager.StartText(dialogue);
        Destroy(gameObject);
    }
}
