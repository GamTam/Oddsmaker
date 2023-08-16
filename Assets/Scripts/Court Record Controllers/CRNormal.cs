using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CRNormal : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CourtRecordController _cr;
    private PlayerInput _playerInput;

    private InputAction _back;
    private InputAction _profiles;
    
    void Start()
    {
        _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back         <sprite=\"Keyboard\" name=\"R\">Profiles");
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        
        _back = _playerInput.actions["Menu/Cancel"];
        _profiles = _playerInput.actions["Menu/Profile"];
    }
    
    void Update()
    {
        if (_back.triggered)
        {
            _cr.Close();
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
            _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back         <sprite=\"Keyboard\" name=\"R\">Profiles");
            return;
        }
        
        _text.SetText("<sprite=\"Keyboard\" name=\"backspace\">Back         <sprite=\"Keyboard\" name=\"R\">Evidence");
    }
}
