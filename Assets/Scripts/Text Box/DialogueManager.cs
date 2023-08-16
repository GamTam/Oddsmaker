using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject _textBoxPrefab;
    private GameObject _tempBox;
    private TMP_Text _advanceButton;
    private TMP_Text textBox;
    private Queue<string> lines;
    private PlayerInput _playerInput;

    [HideInInspector] public bool _done;
    private bool _skipTextBoxCloseAnimation;

    private DialogueVertexAnimator dialogueVertexAnimator;
    private bool movingText;
    private InputAction _advanceText;

    private bool _forceBottom;
    
    void Awake() {
        lines = new Queue<string>();
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _advanceText = _playerInput.actions["confirm"];
    }

    public void StartText(String[] linesIn, Transform parentPos, SpriteRenderer spriteRenderer, bool skipTextBoxCloseAnimation=false, bool forceBottom=false, TMP_FontAsset font = null)
    {
        this._skipTextBoxCloseAnimation = skipTextBoxCloseAnimation;
        
        _tempBox = Instantiate(_textBoxPrefab, GameObject.FindWithTag("UI").transform);
        TextBoxSettings tempBoxSettings = _tempBox.GetComponent<TextBoxSettings>();
        tempBoxSettings.ParentPos = parentPos;
        tempBoxSettings.SpriteRenderer = spriteRenderer;
        tempBoxSettings._forceBottom = forceBottom;
        if (font != null) tempBoxSettings._textMeshPro.font = font;
        tempBoxSettings.Open();

        TMP_Text[] texts = _tempBox.GetComponentsInChildren<TMP_Text>();

        textBox = texts[0];
        _advanceButton = texts[1];
        _advanceButton.enabled = false;
        dialogueVertexAnimator = new DialogueVertexAnimator(textBox);
        
        _playerInput.SwitchCurrentActionMap("Menu");
        lines.Clear();
        
        foreach (string line in linesIn)
        {
            lines.Enqueue(line);
        }

        NextLine();
        _done = false;
    }

    private void Update()
    {
        if (_done || dialogueVertexAnimator == null) return;
        if (_advanceText.triggered)
        {
            NextLine();
        }

        if (!dialogueVertexAnimator.textAnimating && _advanceButton)
        {
            _advanceButton.enabled = true;
        }
    }

    private Coroutine typeRoutine = null;
    public void NextLine() {
        if (dialogueVertexAnimator.textAnimating)
        {
            dialogueVertexAnimator.QuickEnd();
            return;
        }
        
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        if (movingText)
        {
            return;
        }

        this.EnsureCoroutineStopped(ref typeRoutine);
        dialogueVertexAnimator.textAnimating = false;
        List<DialogueCommand> commands =
            DialogueUtility.ProcessInputString(lines.Dequeue(), out string totalTextMessage);
        TextAlignOptions[] textAlignInfo = SeparateOutTextAlignInfo(commands);
        String nameInfo = SeparateOutNameInfo(commands);

        for (int i = 0; i < textAlignInfo.Length; i++)
        {
            TextAlignOptions info = textAlignInfo[i];
            if (info == TextAlignOptions.topCenter)
            {
                textBox.alignment = TextAlignmentOptions.Top;
            }
            else if (info == TextAlignOptions.midCenter)
            {
                textBox.alignment = TextAlignmentOptions.Center;
            }
            else if (info == TextAlignOptions.left)
            {
                textBox.alignment = TextAlignmentOptions.TopLeft;
            }
            else if (info == TextAlignOptions.right)
            {
                textBox.alignment = TextAlignmentOptions.TopRight;
            }
        }

        _advanceButton.enabled = false;
        typeRoutine =
            StartCoroutine(dialogueVertexAnimator.AnimateTextIn(commands, totalTextMessage, "typewriter", null));
    }
    
    private TextAlignOptions[] SeparateOutTextAlignInfo(List<DialogueCommand> commands) {
        List<TextAlignOptions> tempResult = new List<TextAlignOptions>();
        for (int i = 0; i < commands.Count; i++) {
            DialogueCommand command = commands[i];
            if (command.type == DialogueCommandType.Align) {
                tempResult.Add(command.textAlignOptions);
            }
        }
        return tempResult.ToArray();
    }
    
    private String SeparateOutNameInfo(List<DialogueCommand> commands) {
        for (int i = 0; i < commands.Count; i++) {
            DialogueCommand command = commands[i];
            if (command.type == DialogueCommandType.Name) {
                return command.stringValue;
            }
        }
        return null;
    }

    void EndDialogue()
    {
        _done = true;
        StartCoroutine(dialogueVertexAnimator.AnimateTextIn(new List<DialogueCommand>(), "", "typewriter", null));
        if (!_skipTextBoxCloseAnimation) StartCoroutine(_tempBox.GetComponent<TextBoxSettings>().Kill());
        else Destroy(_tempBox);
        _playerInput.SwitchCurrentActionMap("Overworld");
    }
}
