using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CRPresent : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CourtRecordController _cr;

    private InvestigationMenu _menu;
    private DialogueManager _dialogueManager;
    
    private PlayerInput _playerInput;

    private InputAction _back;
    private InputAction _present;
    private InputAction _profiles;
    
    void Start()
    {
        _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back      <sprite=\"Keyboard\" name=\"E\">Present      <sprite=\"Keyboard\" name=\"R\">Profiles");
        _text.transform.parent.localPosition = new Vector3(980, _text.transform.parent.localPosition.y);
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _menu = GameObject.FindWithTag("Respawn").transform.Find("Investigation/Select").GetComponent<InvestigationMenu>();
        _dialogueManager = Globals.DialogueManager;

        _back = _playerInput.actions["Menu/Cancel"];
        _present = _playerInput.actions["Menu/Present"];
        _profiles = _playerInput.actions["Menu/Profile"];
        _cr.HasPresented += Present;
    }
    
    void Update()
    {
        if (!_dialogueManager._doneTalking) return;

        if (_back.triggered)
        {
            _cr.Close();
        }
        
        if (_profiles.triggered)
        {
            _cr.ProfileEvidenceSwap();
        }

        if (_present.triggered)
        {
            _cr.Present();
        }
    }

    void Present(EvidenceSO evidence)
    {
        Globals.SoundManager.Play("textboxAdvance");
        StartCoroutine(_cr.WaitThenLoop(evidence, _menu._evidenceDialogue, _menu._wrongEvidence));
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
}