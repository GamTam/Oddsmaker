using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CRPromptLock : MonoBehaviour
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
        _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back      <sprite=\"Keyboard\" name=\"E\">Present      <sprite=\"Keyboard\" name=\"R\">Profiles");
        _text.transform.parent.localPosition = new Vector3(980, _text.transform.parent.localPosition.y);
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
        
        if (_back.triggered)
        {
            StartPresent(backOut:true);
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
            _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back      <sprite=\"Keyboard\" name=\"E\">Present      <sprite=\"Keyboard\" name=\"R\">Profiles");
            return;
        }
        
        _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back      <sprite=\"Keyboard\" name=\"E\">Present      <sprite=\"Keyboard\" name=\"R\">Evidence");
    }

    private void StartPresent(EvidenceSO evidence=null, bool backOut=false)
    {
        StartCoroutine(Present(evidence, backOut));
    }
    
    private void StartPresent(EvidenceSO evidence)
    {
        StartCoroutine(Present(evidence, false));
    }
    
    private IEnumerator Present(EvidenceSO evidence, bool backOut)
    {
        DialogueSO dialogue = _dialogueManager._dialogue.wrongPresentSequence;
        if (backOut) dialogue = _dialogueManager._dialogue.GiveUp;
        
        if (evidence != null)
        {
            foreach (InvestigationMenu.EvidenceTalkPair pair in _dialogueManager._dialogue.evidence)
            {
                if (pair.Evidence == evidence && Globals.CheckStoryFlags(pair.Conditions))
                {
                    dialogue = pair.Dialogue;
                    break;
                }
            }
        }

        if (dialogue.dialogueText[0].Interjection == Interjection.NA && !backOut) Globals.SoundManager.Play("confirm");
        else if (backOut) Globals.SoundManager.Play("back");

        yield return new WaitForSeconds(0.1f);
        
        if (dialogue.dialogueText[0].Interjection != Interjection.NA) _cr._base.transform.localScale = Vector3.zero;
        
        _cr.Close(false, false);

        if (dialogue.dialogueText[0].Interjection == Interjection.NA) yield return new WaitUntil(() => _cr.enabled == false);
        
        _dialogueManager.StartText(dialogue);
        Destroy(gameObject);
    }
}
