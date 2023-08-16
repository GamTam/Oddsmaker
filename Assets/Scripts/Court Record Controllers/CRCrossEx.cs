using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CRCrossEx: MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CourtRecordController _cr;

    private InvestigationMenu _menu;
    
    private PlayerInput _playerInput;

    private InputAction _back;
    private InputAction _present;
    private InputAction _profiles;
    
    void Start()
    {
        _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back      <sprite=\"Keyboard\" name=\"E\">Present      <sprite=\"Keyboard\" name=\"R\">Profiles");
        _text.transform.parent.localPosition = new Vector3(1015, _text.transform.parent.localPosition.y);
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();

        _back = _playerInput.actions["Menu/Cancel"];
        _present = _playerInput.actions["Menu/Present"];
        _profiles = _playerInput.actions["Menu/Profile"];
    }
    
    void Update()
    {
        if (_back.triggered)
        {
            _playerInput.SwitchCurrentActionMap("Textbox");
            _cr.Close();
        }

        if (_present.triggered)
        {
            _playerInput.SwitchCurrentActionMap("Textbox");
            _cr.Present();
            _cr.Close(false);
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
}