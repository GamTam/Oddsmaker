using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class CrossExamination : MonoBehaviour 
{
    [SerializeField] GameObject _courtRecord;
    
    private EvidenceSO _selectedEvidence;
    private DialogueSO _dialogue;
    private bool _presenting;

    private SoundManager _soundManager;
    
    private DialogueManager dialogueManager;
    private DialogueSO currentDialogue;

    private PlayerInput playerInput;
    private InputAction present;
    private InputAction pressing;
    private InputAction previousLine;
    private InputAction nextLine;

    private void Start() {
        dialogueManager = FindObjectOfType<DialogueManager>();

        playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Textbox");
        pressing = playerInput.actions["Textbox/Press"];
        present = playerInput.actions["Textbox/Court Record"];
        previousLine = playerInput.actions["Textbox/PreviousLine"];
        nextLine = playerInput.actions["Textbox/NextLine"];
        
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

    private void Update() {
        currentDialogue = dialogueManager.ReturnCurrentDialogue();
        
        if (dialogueManager._currentLine != currentDialogue.dialogueText.Length) return;
        if (currentDialogue == null || _presenting) return;
        if (dialogueManager.dialogueVertexAnimator.textAnimating) return;

        if (pressing.triggered) {
            Press();
        }

        if (present.triggered)
        {
            StartCoroutine(Present());
        }

        if (currentDialogue.dialogueType == DialogueOptions.CrossExamination)
        {
            if (nextLine.triggered && currentDialogue.HasNextLine &&
                !dialogueManager.dialogueVertexAnimator.textAnimating)
            {
                _soundManager.Play("confirm");
                if (currentDialogue.FindNextLine().name.Contains("Loop")) Globals.ControlFlag.SetText(new string[] {});
                dialogueManager.StartText(currentDialogue.FindNextLine());
            }

            if (previousLine.triggered && currentDialogue.FindPreviousLine() != null &&
                !dialogueManager.dialogueVertexAnimator.textAnimating)
            {
                _soundManager.Play("confirm");
                dialogueManager.StartText(currentDialogue.FindPreviousLine());
            }
        }
    }

    public IEnumerator Present()
    {
        _selectedEvidence = null;
        dialogueManager._presenting = true;
        _presenting = true;
        
        List<string> controlFlagPosition = dialogueManager._controlFlag._currentButtons;
        dialogueManager._controlFlag.SetText(new string[] {});
        dialogueManager._advanceButton.gameObject.SetActive(false);
        GameObject obj = Instantiate(_courtRecord, GameObject.FindGameObjectWithTag("UI").transform, false);
        CourtRecordController cr = obj.GetComponent<CourtRecordController>();
        obj.GetComponent<CRCrossEx>().enabled = true;
        
        playerInput.SwitchCurrentActionMap("Menu");
        cr.HasPresented += UpdateEvidence;

        while (_selectedEvidence == null)
        {
            if (obj == null)
            {
                _presenting = false;
                dialogueManager._presenting = false;
                dialogueManager._controlFlag.SetText(controlFlagPosition.ToArray(), true);
                yield break;
            }

            yield return null;
        }

        bool found = false;
        foreach (InvestigationMenu.EvidenceTalkPair pair in currentDialogue.ReturnListOfEvidence())
        {
            if (pair.Evidence == _selectedEvidence && Globals.CheckStoryFlags(pair.Conditions))
            {
                found = true;
                _dialogue = pair.Dialogue;
                break;
            }
        }
        
        if (found)
        {
            CorrectEvidenceShown();
        }
        else
        {
            IncorrectEvidenceShown();
        }
        
        _presenting = false;
        dialogueManager._presenting = false;
    }

    void UpdateEvidence(EvidenceSO evidence)
    {
        _selectedEvidence = evidence;
    }

    private void CorrectEvidenceShown() {
        dialogueManager.StartText(_dialogue);
    }

    private void IncorrectEvidenceShown() {
        currentDialogue.wrongPresentSequence.nextLine = currentDialogue;
        dialogueManager.StartText(currentDialogue.wrongPresentSequence);
    }

    private void Press() {
        if (currentDialogue.HasPressingSequence) {
            dialogueManager.StartText(currentDialogue.FindPress());
        }
    }
}