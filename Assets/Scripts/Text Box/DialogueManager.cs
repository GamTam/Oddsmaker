using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

public class DialogueManager : MonoBehaviour
{
    [Foldout("Inspector")] [SerializeField] private GameObject[] _textBoxPrefabs;
    [Foldout("Inspector")] [SerializeField] private GameObject _courtRecordPrefab;
    [Foldout("Inspector")] [SerializeField] private GameObject _courtRecord;
    [Foldout("Inspector")] public SwapCharacters _swap;
    [Foldout("Inspector")] [SerializeField] private GameObject _interjectionObj;
    [Foldout("Inspector")] [SerializeField] private GameObject _fade;
    [Foldout("Inspector")] [SerializeField] private GameObject _scrollObj;
    [Foldout("Inspector")] public SpriteRenderer _background;
    [Foldout("Inspector")] public SpriteRenderer _foreground;
    [Foldout("Inspector")] [SerializeField] private InvestigationMenu _investigationMenu;
    [Foldout("Inspector")] [SerializeField] private GameObject _investigationMenuObj;
    [Foldout("Inspector")] [SerializeField] private CrossExamination _crossExamination;
    [Foldout("Inspector")] [FormerlySerializedAs("_controlFlagController")] [SerializeField] public ControlFlagController _controlFlag;
    [Foldout("Inspector")] [SerializeField] private GameObject _testimony;
    [Foldout("Inspector")] [SerializeField] private GameObject _optionsGameObject;
    [Foldout("Inspector")] [SerializeField] private Material _invertMat;
    [Foldout("Inspector")] [SerializeField] private Volume _grayscale;
    [Foldout("Inspector")] [SerializeField] private EvidenceIconController[] _evidenceIcons;
    [Foldout("Inspector")] [SerializeField] private VideoPlayer _videoPlayer;
    [Foldout("Inspector")] [SerializeField] private GameObject _titleMenu;
    [Foldout("Inspector")] [SerializeField] private PsycheLockController _psycheLock;
    [Foldout("Inspector")] [SerializeField] private float _skipTimer;
    [Foldout("Inspector")] [SerializeField] private GameObject _savePrompt;
    
    private float _skipTimerValue;
    private bool _canSkipText;
    private GameObject _testimonyInstance;

    private bool _revealInvestigationPanel;
    [HideInInspector] public GameObject _cursorInteractables;
    
    private GameObject _tempCourtRecord;
    [HideInInspector] public TextBoxController _tempBox;
    [HideInInspector] private GameObject _tempBoxPrefab;
    [HideInInspector] public Animator _advanceButton;
    [HideInInspector] public TMP_Text textBox;
    private TMP_Text _nameBox;
    private List<TBLine> lines;
    [HideInInspector] public int _currentLine;
    private PlayerInput _playerInput;
    [HideInInspector] public string _prevActionMap;
    private string _typingClip = "blipmale";
    [HideInInspector] public bool _presenting;

    [HideInInspector] public DialogueVertexAnimator dialogueVertexAnimator;
    private bool _hideOptions;
    private InputAction _advanceText;
    private InputAction _skipText;
    private InputAction _skipTextTapped;
    private InputAction _skipTextNull;
    private InputAction _cr;
    private InputAction _options;
    private MusicManager _musicManager;
    private SoundManager _soundManager;
    private List<GameObject> _deadGalleryObjects = new List<GameObject>();
    private List<GameObject> _galleryObjects = new List<GameObject>();

    [HideInInspector] public ResponseHandler _responseHandler;
    [HideInInspector] public DialogueSO _dialogue;
    private bool _shownResponses;
    
    private GameObject _screenAnim = null;
    private bool _waitForScreenAnim;

    private bool _presentImage;
    [Foldout("Inspector")] [SerializeField] private GameObject _presentImagePrefab;

    [Foldout("Inspector")] public List<Character> _chars = new List<Character>();
    [Foldout("Inspector")] public List<CharacterIdentifier> _charsIdentifiers = new List<CharacterIdentifier>();

    [Foldout("Inspector")] [SerializeField] public HealthBarController _hpBar;
    private bool _startedText = true;
    private bool _skipFade;

    private bool _crossEx;
    private bool _autoEnd;
    private bool _mute;
    private bool _quickEnd;

    private bool _isScrolling;

    private List<string> _flagControlFlagPosition;

    private int _healthBarChangeAmount;
    private Camera _cam;
    
    [Foldout("Inspector")] public bool _doneTalking;
    
    [Foldout("Debug")] [SerializeField] private bool _debugMode;
    [Foldout("Debug")] [SerializeField] public DialogueSO _startingDialogue;
    [Foldout("Debug")] [SerializeField] private List<EvidenceSO> _evidence;
    [Foldout("Debug")] [SerializeField] private List<EvidenceSO> _profiles;

    [HideInInspector] public bool SkipTextButtonHeld;

    void Awake() {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Globals.DialogueManager = this;
        lines = new List<TBLine>();
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _advanceText = _playerInput.actions["Textbox/Advance"];
        _skipText = _playerInput.actions["Textbox/Skip"];
        _skipTextTapped = _playerInput.actions["Textbox/SkipTapped"];
        _skipTextNull = _playerInput.actions["Null/Skip"];
        _cr = _playerInput.actions["Textbox/Court Record"];
        _options = _playerInput.actions["Textbox/Options"];
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        _musicManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<MusicManager>();
    }

    IEnumerator Start()
    {
        if (Globals.Evidence.Count == 0) Globals.Evidence = _evidence;
        if (Globals.Profiles.Count == 0) Globals.Profiles = _profiles;
        
        if (_debugMode) StartText(_startingDialogue);
        
        if (Math.Abs(_cam.orthographicSize - 5.4f) > 0.001)
        {
            float timeElapsed = 0;
            float maxTime = 10f;
            
            while (timeElapsed < maxTime)
            {
                timeElapsed += Time.deltaTime;
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, 5.4f, Time.deltaTime);
                    
                yield return null;
            }

            _cam.orthographicSize = 5.4f;
        }
    }

    public void StartText(DialogueSO linesIn, bool quickEnd = false, int startingLine = 0, string prevActionMap = null,
        float waitTime = 0f)
    {
        if (_dialogue != null)
        {
            if (!Globals.UsedDialogue.Contains(_dialogue)) Globals.UsedDialogue.Add(_dialogue);
        }
        
        _revealInvestigationPanel = linesIn.showInvestigationPanel;
        _presentImage = linesIn.dialogueType == DialogueOptions.ImagePrompt;

        _flagControlFlagPosition = _controlFlag._currentButtons;

        if (_revealInvestigationPanel)
        {
            _investigationMenu._selectedButton = null;
            _investigationMenu._wrongEvidence = linesIn.wrongPresentSequence;
            Globals.NoCluesHere = linesIn.noCluesHere;
        }
        
        if (linesIn.Interactables != null)
        {
            if (_cursorInteractables != null)
            {
                Destroy(_cursorInteractables);
            }
            _cursorInteractables = Instantiate(linesIn.Interactables, _background.transform, false);
        }

        switch (linesIn.dialogueType)
        {
            case DialogueOptions.ToInvestigation:
                _investigationMenu._talkText = new List<TalkSO>();
                _investigationMenu._evidenceDialogue = new InvestigationMenu.EvidenceTalkPair[] { };
                _investigationMenu._moveablePlaces = new List<MoveSO>();
                _investigationMenu._talkText = linesIn.talkText;
                _investigationMenu._evidenceDialogue = linesIn.evidence;
                _investigationMenu._moveablePlaces = linesIn.moveablePlaces;
                _investigationMenu._interactables = linesIn.Interactables;
                break;
            case DialogueOptions.PhaseStart:
                Globals.PhaseName = linesIn.PhaseName;
                
                if (linesIn.StartingEvidence.Count > 0) Globals.Evidence = new List<EvidenceSO>(linesIn.StartingEvidence);
                if (linesIn.StartingProfiles.Count > 0) Globals.Profiles = new List<EvidenceSO>(linesIn.StartingProfiles);

                DiscordController.gameName = Globals.GameName;
                DiscordController.phaseName = $"{Globals.CaseName} ({linesIn.PhaseName})";
                break;
            case DialogueOptions.CaseStart:
                Globals.ResetProgress();

                Globals.Evidence = new List<EvidenceSO>(linesIn.StartingEvidence);
                Globals.Profiles = new List<EvidenceSO>(linesIn.StartingProfiles);
                
                Globals.PhaseName = linesIn.PhaseName;
                Globals.CaseName = linesIn.CaseName;

                DiscordController.gameName = Globals.GameName;
                DiscordController.phaseName = $"{linesIn.CaseName} ({linesIn.PhaseName})";
                break;
        }

        if (_testimonyInstance != null) Destroy(_testimonyInstance);

        if (linesIn.dialogueType == DialogueOptions.Testimony)
        {
            _testimonyInstance = Instantiate(_testimony);
        }
        
        _doneTalking = false;
        
        _cr = _playerInput.actions["Textbox/Court Record"];
        _options = _playerInput.actions["Textbox/Options"];

        _crossExamination.enabled = linesIn.dialogueType == DialogueOptions.CrossExamination;
        _crossEx = linesIn.dialogueType == DialogueOptions.CrossExamination;

        _shownResponses = false;
        _currentLine = startingLine;

        _dialogue = linesIn;
        dialogueVertexAnimator ??= new DialogueVertexAnimator(null);
        
        if (prevActionMap == null)
        {
            try
            {
                _prevActionMap = _playerInput.currentActionMap.name;
            }
            catch
            {
                _prevActionMap = "Menu";
            }
        }
        else
        {
            _prevActionMap = prevActionMap;
        }

        if (Globals.CheckStoryFlags(linesIn.skipConditions) && startingLine == 0)
        {
            _currentLine = linesIn.skipIndex;
        }
        
        if (linesIn.StoryFlag != null) if (!Globals.StoryFlags.Contains(linesIn.StoryFlag)) Globals.StoryFlags.Add(linesIn.StoryFlag);

        _playerInput.SwitchCurrentActionMap("TextBox");
        lines.Clear();

        lines = linesIn.dialogueText.ToList();

        StartCoroutine(NextLine(true, quickEnd, waitTime:waitTime));
    }

    private void Update()
    {
        SkipTextButtonHeld = _skipText.ReadValue<float>() > 0.5f || _skipTextNull.ReadValue<float>() > 0.5f;
        _skipTimerValue -= Time.deltaTime;
        _canSkipText = false;
        if (_skipTimerValue <= 0)
        {
            _skipTimerValue = _skipTimer;
            if (!_presenting) _canSkipText = true;
        }
        
        if (!_startedText) return;
        if (_doneTalking) return;
        if (_hpBar._hitAnimation) return;
        if (_screenAnim != null && _waitForScreenAnim) return;
        if (SkipTextButtonHeld) return;

        foreach (CharacterIdentifier chara in _charsIdentifiers)
        {
            if (Globals.IsAnimationPlaying(chara._anim, $"{chara._character.AnimPlaying}_opening") && !chara._character.DontWaitForOpeningAnimationToEnd) return;
        }
        
        if (_presenting || _shownResponses || _tempBox.IsHidden)
        {
            _advanceButton.gameObject.SetActive(false);
            return;
        }
        if (dialogueVertexAnimator.textAnimating || _advanceButton.gameObject.activeSelf) return;

        _advanceButton.gameObject.SetActive(true);
        if (_crossEx)
        {
            _advanceButton.Play("Idle_Cross");
            
            _advanceButton.transform.Find("Forwards").gameObject.GetComponent<Image>().enabled = !_dialogue.FindNextLine().name.Contains("Loop");

            _advanceButton.transform.Find("Backwards").gameObject.GetComponent<Image>().enabled = _dialogue.prevLine != null;
        }
        else
        {
            _advanceButton.transform.Find("Backwards").gameObject.GetComponent<Image>().enabled = false;
            _advanceButton.transform.Find("Forwards").gameObject.GetComponent<Image>().enabled = true;
                    
            _advanceButton.Play("Idle");
        }
    }
    
    private void LateUpdate()
    {
        SkipTextButtonHeld = _skipText.ReadValue<float>() > 0.5f || _skipTextNull.ReadValue<float>() > 0.5f;
        
        if (!_startedText) return;
        if (_doneTalking) return;
        if (_hpBar._hitAnimation) return;
        if (_screenAnim != null && _waitForScreenAnim) return;
        
        foreach (CharacterIdentifier chara in _charsIdentifiers)
        {
            if (Globals.IsAnimationPlaying(chara._anim, $"{chara._character.AnimPlaying}_opening") && !chara._character.DontWaitForOpeningAnimationToEnd) return;
        }

        if (_advanceText.triggered || _skipTextTapped.triggered || (SkipTextButtonHeld && _canSkipText))
        {
            if (_skipTextTapped.triggered) _skipTimerValue = _skipTimer * 2f;
            else StartCoroutine(NextLine());
        }
        else if (!dialogueVertexAnimator.textAnimating &&
                 (_currentLine == lines.Count && (_dialogue.HasResponses || _dialogue.HasPresentPrompt) && !_shownResponses) || _autoEnd)
        {
            if (!dialogueVertexAnimator.textAnimating)
            {
                
                StartCoroutine(NextLine());
            }
        }
        
        if (!dialogueVertexAnimator.textAnimating && _cr.triggered && !_hideOptions)
        {
            StartCoroutine(CourtRecord());
        } 
        else if (!dialogueVertexAnimator.textAnimating && _options.triggered && !_hideOptions && !_controlFlag._currentButtons.Equals(new string[] {"Court Record"}))
        {
            StartCoroutine(Options());
        }

        if (_advanceButton == null) return;
        if ((dialogueVertexAnimator.textAnimating && dialogueVertexAnimator.visableCharacterIndex < dialogueVertexAnimator.charCount - 1) || _advanceButton.gameObject.activeSelf) return;

        foreach (var t in _chars)
        {
            CharacterIdentifier tempID = t.CharOnScreen.GetComponent<CharacterIdentifier>();
            tempID.IsTalking = false;
        }

        if (lines[_currentLine - 1].ShowHealthBar)
        {
            switch (_healthBarChangeAmount)
            {
                case 0:
                    return;
                case < 0:
                    StartCoroutine(_hpBar.Hit(_healthBarChangeAmount));
                    break;
            }
        }

        _healthBarChangeAmount = 0;
    }

    public DialogueSO ReturnCurrentDialogue() {
        return _dialogue;
    }

    private IEnumerator CourtRecord()
    {
        if (_presenting || _crossEx) yield break;

        _presenting = true;
        
        if (_shownResponses) _responseHandler.Hide();
        else _advanceButton.gameObject.SetActive(false);
        
        List<string> controlFlagPosition = _controlFlag._currentButtons;
        _controlFlag.SetText(new string[] {});
        GameObject obj = Instantiate(_courtRecord, GameObject.FindGameObjectWithTag("UI").transform, false);
        CourtRecordController court = obj.GetComponent<CourtRecordController>();
        obj.GetComponent<CRNormal>().enabled = true;
        
        _playerInput.SwitchCurrentActionMap("Menu");

        while (!court.Closing)
        {
            yield return null;
        }
        
        if (!_shownResponses) _advanceButton.gameObject.SetActive(true);
        _controlFlag.SetText(controlFlagPosition.ToArray(), true);
        
        while (obj != null)
        {
            yield return null;
        }
        
        _presenting = false;
        if (_shownResponses) _responseHandler.Show();
        else _playerInput.SwitchCurrentActionMap("TextBox");
    }
    
    private IEnumerator Options()
    {
        if (_presenting) yield break;
        if (_shownResponses) _responseHandler.Hide();
        
        _presenting = true;

        Globals.SoundManager.Play("record flip");
        _optionsGameObject.SetActive(true);
        _playerInput.SwitchCurrentActionMap("Menu");

        do
        {
            yield return null;
        } while (Globals.InOptions);
        
        _optionsGameObject.SetActive(false);
        
        _presenting = false;
        if (_shownResponses) _responseHandler.Show();
        else _playerInput.SwitchCurrentActionMap("TextBox");
    }
    
    private Coroutine typeRoutine = null;

    private IEnumerator NextLine(bool firstTime = false, bool quickEnd = false, bool loopThroughEvents = false,
        float waitTime = 0f)
    {
        if (waitTime > 0f)
        {
            if (_tempBox != null) _tempBox.HideAll();
            yield return new WaitForSeconds(waitTime);
        }
        
        #region Variable Setup

        if (_shownResponses) yield break;

        if (_autoEnd && dialogueVertexAnimator.textAnimating)
        {
            if (lines[_currentLine].AddToPrevious && !lines[_currentLine].AutoEnd)
            {
                dialogueVertexAnimator.QuickEnd();
                yield return StartCoroutine(NextLine());
                dialogueVertexAnimator.textAnimating = true;
                dialogueVertexAnimator.QuickEnd();
            }
            
            yield break;
        }

        if (dialogueVertexAnimator.textAnimating)
        {
            dialogueVertexAnimator.QuickEnd();
            yield break;
        }

        if (_currentLine == lines.Count)
        {
            if (_dialogue.HasResponses)
            {
                _shownResponses = true;
                _cr = _playerInput.actions["Menu/Court Record"];
                _options = _playerInput.actions["Menu/Options"];
                if (!_quickEnd) yield return new WaitForSeconds(0.5f);
                else _mute = true;
                _responseHandler.ShowResponses(_dialogue.responses);
            } 
            else if (_dialogue.HasPresentPrompt)
            {
                _shownResponses = true;
                if (!_quickEnd) yield return new WaitForSeconds(0.5f);
                _mute = true;
                _controlFlag.Hide();
                GameObject cr = Instantiate(_courtRecord, GameObject.FindGameObjectWithTag("UI").transform, false);
                if (_dialogue.dialogueType == DialogueOptions.CourtRecordPrompt) cr.GetComponent<CRPrompt>().enabled = true;
                if (_dialogue.dialogueType == DialogueOptions.CourtRecordPromptLock) cr.GetComponent<CRPromptLock>().enabled = true;
            }
        }

        _charsIdentifiers = new List<CharacterIdentifier>();

        if (!firstTime && !_mute)
        {
            if (_crossEx && _currentLine == lines.Count)
            {
                _soundManager.Play("confirm");   
            }
            else
            {
                _soundManager.Play("textboxAdvance");
            }
        }
        
        _mute = false;

        if (_currentLine >= lines.Count && !(_dialogue.HasResponses || _dialogue.HasPresentPrompt))
        {
            if (_dialogue.dialogueType == DialogueOptions.CanGameOver && Globals.HP <= 0) StartText(_dialogue.GameOverDialogue);
            else if (_dialogue.HasNextLine && _dialogue.dialogueType != DialogueOptions.PhaseEnd && _dialogue.dialogueType != DialogueOptions.CaseEnd)
            {
                if (_dialogue.FindNextLine().name.Contains("Loop")) _controlFlag.SetText(new string[] {});
                StartText(_dialogue.FindNextLine());
            }
            else
            {
                _shownResponses = true;
                StartCoroutine(EndDialogue());
            }
            yield break;
        }

        _quickEnd = quickEnd;
        _skipFade = false;
        TBLine line = lines[_currentLine];
        _currentLine += 1;
        this.EnsureCoroutineStopped(ref typeRoutine);
        dialogueVertexAnimator.textAnimating = false;
        string prevText = "";
        if (line.AddToPrevious)
        {
            bool prevRecursion = true;
            int tempLine = _currentLine - 1;

            while (prevRecursion)
            {
                if (lines[tempLine].AddToPrevious)
                {
                    tempLine -= 1;
                }
                else
                {
                    prevRecursion = false;
                }
            }

            while (tempLine < _currentLine - 1)
            {
                if (!string.IsNullOrEmpty(line.Dialogue))
                {
                    if (line.DontAddSpaceToPrevious || line.Dialogue[0] == '\n')
                        prevText += RemoveRichTextDynamicTag(lines[tempLine].Dialogue);
                    else prevText += RemoveRichTextDynamicTag(lines[tempLine].Dialogue) + " ";
                }
                else
                {
                    if (line.DontAddSpaceToPrevious)
                        prevText += RemoveRichTextDynamicTag(lines[tempLine].Dialogue);
                    else prevText += RemoveRichTextDynamicTag(lines[tempLine].Dialogue) + " ";
                }
                tempLine += 1;
            }
        }
        List<DialogueCommand> commands =
            DialogueUtility.ProcessInputString(prevText + line.Dialogue, out string totalTextMessage);
        DialogueUtility.ProcessInputString(prevText, out prevText);
        
        String soundInfo = line.BlipSound == DialogueSoundTypes.Custom ? line.CustomBlipSound : null;
        
        if (soundInfo == null)
        {
            soundInfo = line.BlipSound switch
            {
                DialogueSoundTypes.Male => "blipmale",
                DialogueSoundTypes.Female => "blipfemale",
                DialogueSoundTypes.Typewriter => "typewriter",
                _ => "none"
            };
        }
        
        _chars = line.Chars.ToList();
        Interjection interjection = line.Interjection;
        BackgroundFade bg = line.FadeDetails;
        _hideOptions = line.HideOptions;
        _skipFade = line.FadeType == FadeTypes.SkipFade;
        _waitForScreenAnim = line.CannotAdvanceUntilAnimationDone;

        if (line.Chars.Length == 1) line._charTalking = line.Chars[0].CharOnScreen.gameObject.name;

        _autoEnd = line.AutoEnd;
        if (line.AutoEnd)
        {
            _mute = true;
        }
        
        if (line.CustomTextBox != null)
        {
            if (_tempBoxPrefab != line.CustomTextBox)
            {
                if (_tempBox != null) Destroy(_tempBox.gameObject);
                _tempBox = Instantiate(line.CustomTextBox).GetComponent<TextBoxController>();
                _tempBoxPrefab = line.CustomTextBox;
                _tempBox.transform.SetParent(GameObject.FindWithTag("UI").transform, false);
                _tempBox.transform.SetSiblingIndex(0);

                TMP_Text[] texts = _tempBox.GetComponentsInChildren<TMP_Text>();
                _advanceButton = _tempBox.GetComponentsInChildren<Animator>()[1];

                textBox = texts[1];
                _nameBox = texts[0];
                _advanceButton.gameObject.SetActive(false);
                dialogueVertexAnimator = new DialogueVertexAnimator(textBox);
                dialogueVertexAnimator._parent = this;
            }
        }
        else
        {
            if (_tempBoxPrefab != _textBoxPrefabs[(int) Globals.UI])
            {
                if (_tempBox != null) Destroy(_tempBox.gameObject);
                _tempBox = Instantiate(_textBoxPrefabs[(int) Globals.UI]).GetComponent<TextBoxController>();
                _tempBoxPrefab = _textBoxPrefabs[(int) Globals.UI];
                _tempBox.transform.SetParent(GameObject.FindWithTag("UI").transform, false);
                _tempBox.transform.SetSiblingIndex(0);

                TMP_Text[] texts = _tempBox.GetComponentsInChildren<TMP_Text>();
                _advanceButton = _tempBox.GetComponentInChildren<Animator>();

                textBox = texts[1];
                _nameBox = texts[0];
                _advanceButton.gameObject.SetActive(false);
                dialogueVertexAnimator = new DialogueVertexAnimator(textBox);
                dialogueVertexAnimator._parent = this;
            }
        }

        StateChange state = line.StateChange;
        if (state.EvidenceToAdd != null) if (!Globals.Evidence.Contains(state.EvidenceToAdd)) Globals.Evidence.Add(state.EvidenceToAdd);
        if (state.EvidenceToRemove != null) if (Globals.Evidence.Contains(state.EvidenceToRemove)) Globals.Evidence.Remove(state.EvidenceToRemove);
        if (state.PersonToAdd != null) if (!Globals.Profiles.Contains(state.PersonToAdd)) Globals.Profiles.Add(state.PersonToAdd);

        bool addToCourtRecord = line.AddToCourtRecord;

        TextAlignOptions options = line.Align;
        
        if (options == TextAlignOptions.topCenter)
        {
            textBox.alignment = TextAlignmentOptions.Top;
        }
        else if (options == TextAlignOptions.midCenter)
        {
            textBox.alignment = TextAlignmentOptions.Center;
        }
        else if (options == TextAlignOptions.left)
        {
            textBox.alignment = TextAlignmentOptions.TopLeft;
        }
        else if (options == TextAlignOptions.right)
        {
            textBox.alignment = TextAlignmentOptions.TopRight;
        }
        
        if (bg.BackgroundFadeType != BGFadeTypes.None) _fade.transform.position = new Vector3(_fade.transform.position.x, _fade.transform.position.y, (int) bg.BackgroundFadePos);

        if (line.StopMusic) _musicManager.FadeOut();
        
        if (soundInfo != null) _typingClip = soundInfo;

        if (_tempCourtRecord != null)
        {
            _tempCourtRecord.GetComponent<Animator>().Play("Fade Out");
        }
        while (_tempCourtRecord != null)
        {
            yield return null;
        }

        if (!line.ShowTextBox)
        {
            _controlFlag.Hide();
            _tempBox.HideAll();
        }
        else
        {
            if (line.HideOptions) _controlFlag.Hide();
            else _controlFlag.Show();
            _tempBox.ShowAll();
            if (!line.AddToPrevious) _tempBox.Text.text = "";
            _nameBox.transform.parent.gameObject.SetActive(!line.HideNameTag);
            _advanceButton.gameObject.SetActive(false);
        }
        #endregion

        #region Reset Scene

        _playerInput.SwitchCurrentActionMap("Null");
        if (line._extras.ShowPsycheLock)
        {
            _controlFlag.SetText(new string[] {"Court Record"}, !line.ShowTextBox);
        }
        else if (_dialogue.IsCrossExamination)
        {
            _controlFlag.SetText(new string[] {"Options", "Present", "Press"}, !line.ShowTextBox);
        }
        else
        {
            _controlFlag.SetText(new string[] {"Options", "Court Record"}, !line.ShowTextBox);
        }

        foreach (Character chara in _chars)
        {
            if (chara.AnimPlaying != null && chara.AnimPlaying != "Keep Previous Animation")
            {
                CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                tempID.IsTalking = false;
                tempID.UpdateAnims();
            }
        }

        _tempBox.transform.SetSiblingIndex(0);

        if (quickEnd) goto SetBackground;
        
        _startedText = false;
        #endregion
        
        #region Interjection

        string name;
        if (_chars.Count == 0 && string.IsNullOrEmpty(RemoveRichTextDynamicTag(line.Name))) name = "";
        else name = RemoveRichTextDynamicTag(line.Name);
        
        bool skip = false;
        
        string interjectionType = null;
        switch (interjection)
        {
            case Interjection.Objection:
                interjectionType = "objection";
                break;
            case Interjection.HoldIt:
                interjectionType = "holdIt";
                break;
            case Interjection.TakeThat:
                interjectionType = "takeThat";
                break;
            case Interjection.Custom:
                interjectionType = line.CustomInterjection;
                break;
            case Interjection.NA:
                skip = true;
                break;
        }

        if (!skip)
        {
            GameObject obj = Instantiate(_interjectionObj);
            RawImage img = obj.GetComponent<RawImage>();
            
            obj.transform.SetParent(GameObject.FindGameObjectWithTag("UI").transform, false);

            img.texture = Addressables.LoadAssetAsync<Texture>($"Assets/Sprites/Interjections/{interjectionType}.png").WaitForCompletion();
            img.SetNativeSize();
            
            if (_soundManager.Play($"{interjectionType}{name}", 1f) == null)
            {
                _soundManager.Play("interjection");
            }
            
            if (line.WideBackground()) yield return new WaitForSeconds(0.98f);
            else
            {
                yield return new WaitForSeconds(1f);
                Destroy(obj);
            }
        }
        #endregion

        #region Hide Evidence Icons

        SetBackground:
        if (_evidenceIcons[0].CurrentIcon != null && line.LeftEvidence == null)
        {
            if (!line.SkipIconAnimation && !quickEnd)
            {
                if (line.WaitForIconAnimation) yield return StartCoroutine(_evidenceIcons[0].Shrink());
                else StartCoroutine(_evidenceIcons[0].Shrink());
            }
            else
            {
                _evidenceIcons[0].transform.localScale = Vector3.zero;
                _evidenceIcons[0].SetImage(null);
            }
        }
        
        if (_evidenceIcons[1].CurrentIcon != null && line.RightEvidence == null)
        {
            if (!line.SkipIconAnimation && !quickEnd)
            {
                if (line.WaitForIconAnimation) yield return StartCoroutine(_evidenceIcons[1].Shrink());
                else StartCoroutine(_evidenceIcons[1].Shrink());
            }
            else
            {
                _evidenceIcons[1].transform.localScale = Vector3.zero;
                _evidenceIcons[1].SetImage(null);
            }
        }

        #endregion
        
        #region Screen Fade Out
        
        if (bg.BackgroundFadeType is BGFadeTypes.FadeOut or BGFadeTypes.FadeOutThenIn)
        {
            SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
            Color startColor = spr.color;
            if (startColor.a == 0)
            {
                startColor = Color.clear;
            }
            Color endColor = new Color(bg.Color.r, bg.Color.g, bg.Color.b, 1);
            float time = 0;

            while (time < bg.LengthInSeconds && !quickEnd)
            {
                time += Time.deltaTime;
                spr.color = Color.Lerp(startColor, endColor, time / bg.LengthInSeconds);
                if (bg.IncludeForeground) _foreground.color = Color.Lerp(Color.white, Color.clear, time / bg.LengthInSeconds);
                if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
                yield return null;
            }

            spr.color = endColor;
        }

        #endregion
        
        #region Set Background

        if (!string.IsNullOrEmpty(line.Background.AssetGUID))
        {
            AsyncOperationHandle<Sprite> bgSpriteHandler = Addressables.LoadAssetAsync<Sprite>(line.Background);
            Sprite newBg = bgSpriteHandler.WaitForCompletion();
            if (newBg != _background.sprite) _isScrolling = false;
            _background.sprite = newBg;
        }

        try
        {
            _foreground.sprite = Addressables
                .LoadAssetAsync<Sprite>("Assets/Sprites/Foregrounds/" + _background.sprite.name + "_fg.png")
                .WaitForCompletion();
        } catch {}

        if (!string.IsNullOrEmpty(line.CustomForeground.AssetGUID))
        {
            _foreground.sprite = Addressables.LoadAssetAsync<Sprite>(line.CustomForeground).WaitForCompletion();
        }

        if (line.HideForeground) _foreground.sprite = null;
        else _foreground.color = Color.white;
        
        _invertMat.SetInt("_InvertColor", bg.InvertColors ? 1 : 0);
        _grayscale.weight = bg.Grayscale ? 1 : 0;

        _background.flipX = line.FlipBackground;
        _foreground.flipX = line.FlipBackground;
        
        if (line.ShowTextBox)
        {
            CharacterIdentifier identity = null;

            if (_chars.Count > 0 && !line.Thinking &&
                !String.IsNullOrWhiteSpace(Globals.RemoveRichText(totalTextMessage)))
            {
                foreach (Character chara in _chars)
                {
                    CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                    if (chara.CharOnScreen.name == line._charTalking)
                    {
                        identity = tempID;
                        break;
                    }
                }
            }
            else if (_chars.Count > 0)
            {
                foreach (Character chara in _chars)
                {
                    CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();

                    if (chara.CharOnScreen.name == line._charTalking)
                    {
                        identity = tempID;
                        break;
                    }
                }
            }

            _nameBox.text = line.Name == line._charTalking && identity != null ? identity.Name : line.Name;
        }
        
        bool updatePeople = true;
        if (_deadGalleryObjects.Count > 0 && line.ObjectsToSpawn.Count > 0)
        {
            if (line.ObjectsToSpawn.All(_deadGalleryObjects.Contains))
            {
                updatePeople = false;
            }
        }
        
        if (updatePeople)
        {
            foreach (GameObject person in _galleryObjects)
            {
                Destroy(person);
            }

            _galleryObjects = new List<GameObject>();
            _deadGalleryObjects = new List<GameObject>(line.ObjectsToSpawn);
            foreach (GameObject person in line.ObjectsToSpawn)
            {
                _galleryObjects.Add(Instantiate(person, _scrollObj.transform, false));
            }
        }
        
        #endregion

        #region Scroll Background

        if (!_isScrolling)
        {
            if (line.DontWaitForScrolling) StartCoroutine(ScrollBackground(line, quickEnd));
            else
            {
                _isScrolling = true;
                Vector3 bgPos = new Vector3(0, 0, 0);
                
                if (!string.IsNullOrEmpty(line.Background.AssetGUID))
                {
                    float width = _background.sprite.texture.width / 100f;
                    float height = _background.sprite.texture.height / 100f;

                    float startPosH = (-1 * width * ((int) line.HorizontalScrollStartPercentage / 100f)) + (width / 2);
                    startPosH = Mathf.Clamp(startPosH, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);

                    float endPosH = (-1 * width * ((int) line.HorizontalScrollEndPercentage / 100f)) + (width / 2);
                    endPosH = Mathf.Clamp(endPosH, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);
                    
                    float startPosV = (-1 * height * ((int) line.VerticalScrollStartPercentage / 100f)) + (height / 2);
                    startPosV = Mathf.Clamp(startPosV, (-height / 2f) + 5.4f, (height / 2f) - 5.4f);

                    float endPosV = (-1 * height * ((int) line.VerticalScrollEndPercentage / 100f)) + (height / 2);
                    endPosV = Mathf.Clamp(endPosV, (-height / 2f) + 5.4f, (height / 2f) - 5.4f);

                    if ((line.WideBackground() && !quickEnd && Math.Abs(_scrollObj.transform.position.x - endPosH) > 0.1 && (Math.Abs(startPosH - endPosH) > 0.01)) ||
                        (line.TallBackground() && !quickEnd && Math.Abs(_scrollObj.transform.position.y - endPosV) > 0.1 && (Math.Abs(startPosV - endPosV) > 0.01)))
                    {
                        _scrollObj.transform.position =
                            new Vector3(startPosH, startPosV, bgPos.z);
                        
                        float timePassed = 0;
                        float totalTime = 0.5f;
                        switch (line.ScrollSpeed)
                        {
                            case ScrollSpeedOptions.Default:
                                totalTime = 0.5f;
                                break;
                            case ScrollSpeedOptions.Fast:
                                totalTime = 0.25f;
                                break;
                            case ScrollSpeedOptions.Slow:
                                totalTime = 1.5f;
                                break;
                            case ScrollSpeedOptions.Custom:
                                totalTime = line.ScrollTime;
                                break;
                        }
                        
                        _swap.UpdateCharPos(new Vector2(startPosH * -1, startPosV));
                        List<Character> chara = _swap._chars.ToList();
                        
                        _swap.QuickSwap(_chars, Vector2.zero, line.HidePreviousCharWhileSwapping);
                        _chars = _swap._chars;
                        _swap.UpdateCharPos(new Vector2(endPosH * -1, endPosV * -1));

                        foreach (Character charb in _chars)
                        {
                            CharacterIdentifier tempID = charb.CharOnScreen.GetComponent<CharacterIdentifier>();
                            tempID.IsTalking = false;
                            tempID.UpdateAnims();
                        }

                        do
                        {
                            float xPos = line.LinearScrolling ? Mathf.Lerp(startPosH, endPosH, timePassed / totalTime) : Mathf.SmoothStep(startPosH, endPosH, timePassed / totalTime);
                            float yPos = line.LinearScrolling ? Mathf.Lerp(startPosV, endPosV, timePassed / totalTime) : Mathf.SmoothStep(startPosV, endPosV, timePassed / totalTime);

                            if (totalTime == 0)
                            {
                                xPos = endPosH;
                                yPos = endPosV;
                            }

                            _scrollObj.transform.position =
                                new Vector3(xPos, yPos, bgPos.z);

                            timePassed += Time.deltaTime;

                            yield return null;
                        } while (timePassed < totalTime);

                        if (chara.Count > 0)
                        {
                            foreach (Character character in chara)
                            {
                                Destroy(character.CharOnScreen);
                            }
                        }
                        
                        _scrollObj.transform.position = new Vector3(endPosH, endPosV, _scrollObj.transform.localPosition.z);
                        if (totalTime != 0) yield return new WaitForSeconds(0.1f);
                    }
                    else if (line.WideBackground() || line.TallBackground())
                    {
                        _scrollObj.transform.position = new Vector3(endPosH, endPosV, _scrollObj.transform.localPosition.z);
                        _swap.CharPos = new Vector2(endPosH * -1, endPosV * -1);
                    }
                    else
                    {
                        _scrollObj.transform.position = bgPos;
                        _swap.UpdateCharPos(new Vector2(0, 0));
                    }
                }

                _isScrolling = false;
            }
        }

        if (quickEnd) goto CharSwap;
        #endregion

        #region Add To Court Record
        if (addToCourtRecord)
        {
            _soundManager.Play("court record");
            _tempCourtRecord = Instantiate(_courtRecordPrefab, GameObject.FindWithTag("UI").transform, false);

            _tempCourtRecord.GetComponentsInChildren<Image>()[3].sprite = state.EvidenceToAdd.Icon;
            _tempCourtRecord.GetComponentsInChildren<Image>()[3].SetNativeSize();
            
            _tempCourtRecord.gameObject.GetComponentsInChildren<TMP_Text>()[0].SetText(state.EvidenceToAdd.name);
            _tempCourtRecord.gameObject.GetComponentsInChildren<TMP_Text>()[1].SetText(state.EvidenceToAdd.Description);
            
            _tempCourtRecord.transform.SetSiblingIndex(_tempBox.transform.GetSiblingIndex() - 1);
        }
        #endregion

        #region Swap Characters

        CharSwap:
        if ((!_swap.CompareCharList(_chars) || _chars.Count == 0) && line.FadeType == FadeTypes.SkipFade || quickEnd)
        {
            _swap.QuickSwap(_chars, _swap.CharPos, true);

            _chars = _swap._chars;
        }
        else if ((!_swap.CompareCharList(_chars) || _chars.Count == 0) || line.FadeType == FadeTypes.ForceFade)
        {
            _swap.StartSwap(_chars, speed:line._extras.FadeOptions.CharFadeSpeed, fadeIn:_chars.Count > 0, skipFade:_skipFade);
            while (!_swap._done)
            {
                if (_chars != _swap._chars && _swap._swapped)
                {
                    _chars = _swap._chars;
                    foreach (Character chara in _chars)
                    {
                        CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                        tempID.IsTalking = false;
                        tempID.UpdateAnims();
                    }
                }
                
                yield return null;
            }
            
            if (!_skipFade && _chars.Count > 0)
            {
                yield return new WaitForSeconds(0.25f);
            }
        }
        else
        {
            bool needsToMove = false;
            
            for (int i=0; i<_swap._chars.Count; i++)
            {
                _swap._chars[i].AnimPlaying = _chars[i].AnimPlaying == "Keep Previous Animation" ? _swap._chars[i].AnimPlaying : _chars[i].AnimPlaying;
                _swap._chars[i].Flip = _chars[i].Flip;
                _swap._chars[i].InFrontOfForeground = _chars[i].InFrontOfForeground;
                _swap._chars[i].SkipOpeningAnimation = _chars[i].SkipOpeningAnimation;
                _swap._chars[i].DontWaitForOpeningAnimation = _chars[i].DontWaitForOpeningAnimation;
                _swap._chars[i].DontWaitForOpeningAnimationToEnd = _chars[i].DontWaitForOpeningAnimationToEnd;
                _swap._chars[i].MoveSpeed = _chars[i].MoveSpeed;
                _swap._chars[i].MoveTime = _chars[i].MoveTime;
                _swap._chars[i].zPos = _swap._chars[i].InFrontOfForeground ? -4 - (_swap._chars.Count - i) : i + _swap._chars.Count - 1;

                if (_swap._chars[i].Offset != _chars[i].Offset) needsToMove = true;
                
                _swap._charPrefabs[i].AnimPlaying = _chars[i].AnimPlaying == "Keep Previous Animation" ? _swap._chars[i].AnimPlaying : _chars[i].AnimPlaying;
                _swap._charPrefabs[i].Flip = _chars[i].Flip;
                _swap._charPrefabs[i].InFrontOfForeground = _chars[i].InFrontOfForeground;
                _swap._charPrefabs[i].SkipOpeningAnimation = _chars[i].SkipOpeningAnimation;
                _swap._charPrefabs[i].DontWaitForOpeningAnimation = _chars[i].DontWaitForOpeningAnimation;
                _swap._charPrefabs[i].DontWaitForOpeningAnimationToEnd = _chars[i].DontWaitForOpeningAnimationToEnd;
                _swap._charPrefabs[i].MoveSpeed = _chars[i].MoveSpeed;
                _swap._charPrefabs[i].MoveTime = _chars[i].MoveTime;
                
                Vector3 scale = _swap._chars[i].CharOnScreen.transform.localScale;
                if (_swap._chars[i].Flip)
                {
                    if (_swap._chars[i].CharOnScreen.transform.localScale.x > 0) scale.x *= -1;
                }
                else
                {
                    scale.x = Mathf.Abs(scale.x);
                }

                _swap._chars[i].CharOnScreen.transform.localScale = scale;

                Character chara = _swap._chars[i];
                
                CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                tempID.IsTalking = false;
                tempID.UpdateAnims();
            }

            if (needsToMove)
            {
                List<Coroutine> coroutines = new List<Coroutine>();
                for (int i = 0; i < _swap._chars.Count; i++) 
                    coroutines.Add(StartCoroutine(_swap.MoveChar(_swap._chars[i], _chars[i].Offset)));

                for (int i = 0; i < coroutines.Count; i++)
                    yield return coroutines[i];
            } 

            if (needsToMove) yield return new WaitForSeconds(0.25f);
            
            for (int i=0; i<_swap._chars.Count; i++)
            {
                _swap._chars[i].Offset = _chars[i].Offset;
                _swap._charPrefabs[i].Offset = _chars[i].Offset;
            }

            _chars = _swap._chars;
        }
        #endregion

        #region Screen Fade In
        
        if (bg.BackgroundFadeType is BGFadeTypes.FadeIn or BGFadeTypes.FadeOutThenIn)
        {
            SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
            Color startColor = spr.color;
            float time = 0;
            Color endColor = new Color(bg.Color.r, bg.Color.g, bg.Color.b, 0);
                
            while (time < bg.LengthInSeconds && !quickEnd)
            {
                time += Time.deltaTime;
                spr.color = Color.Lerp(startColor, endColor, time / bg.LengthInSeconds);
                if (bg.IncludeForeground) _foreground.color = Color.Lerp(Color.clear, Color.white, time / bg.LengthInSeconds);
                if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
                yield return null;
            }

            spr.color = endColor;
        }

        #endregion

        #region Play Video

        if (line.Video != null)
        {
            _videoPlayer.enabled = true;
            _videoPlayer.clip = line.Video;
            _videoPlayer.Prepare();

            while (!_videoPlayer.isPrepared)
            {
                yield return null;
            }

            _videoPlayer.Play();

            do
            {
                yield return null;
            } while (_videoPlayer.isPlaying);

            _videoPlayer.enabled = false;
        }

        #endregion
        
        #region Show/Hide HP Bar

        if (line.ShowHealthBar) _hpBar.Show(quickEnd);
        else _hpBar.Hide(quickEnd);
        
        if (quickEnd)
        {
            if (line.HealthBarChangeAmount == 0)
            {
                _hpBar.SetWarnAmount(line.HealthBarWarnAmount);
                _healthBarChangeAmount = line.HealthBarChangeAmount;
            }
            else
            {
                _hpBar.Hide(true);
            }
        }
        else
        {
            _hpBar.SetWarnAmount(line.HealthBarWarnAmount);
            _healthBarChangeAmount = line.HealthBarChangeAmount;
        }

        #endregion

        #region Play Opening Animation
        if (_chars.Count > 0 && !quickEnd)
        {
            bool animsPlaying = false;

            do
            {
                animsPlaying = true;
                foreach (Character chara in _chars)
                {
                    if (!chara.CharOnScreen.GetComponent<Animator>()
                            .HasState(0, Animator.StringToHash($"{chara.AnimPlaying}_opening")) ||
                        chara.SkipOpeningAnimation)
                    {
                        CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                        tempID.IsTalking = false;
                        tempID.UpdateAnims();
                    }
                    else if (!Globals.IsAnimationPlaying(chara.CharOnScreen.GetComponent<Animator>(),
                                 $"{chara.AnimPlaying}_opening") && !chara.SkipOpeningAnimation)
                    {
                        animsPlaying = false;
                        CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                        if (tempID.Is3DCharacter)
                        {
                            Animator anim = chara.CharOnScreen.GetComponent<Animator>();

                            anim.CrossFade($"{chara.AnimPlaying}_opening", 0.5f);
                            anim.Play($"{chara.AnimPlaying}_mouth_opening", 2);
                        }
                        else chara.CharOnScreen.GetComponent<Animator>().Play($"{chara.AnimPlaying}_opening");

                        chara.CharOnScreen.GetComponent<Animator>().Update(0);
                    }
                }

                if (!animsPlaying) yield return null;
            } while (!animsPlaying);

            animsPlaying = false;
            
            while (!animsPlaying)
            {
                animsPlaying = true;
                foreach (Character chara in _chars)
                {
                    if (Globals.IsAnimationPlaying(chara.CharOnScreen.GetComponent<Animator>(), $"{chara.AnimPlaying}_opening") && !chara.DontWaitForOpeningAnimation)
                        animsPlaying = false;
                }
                
                if (!animsPlaying) yield return null;
            }
            
            foreach (Character chara in _chars)
            {
                if (chara.DontWaitForOpeningAnimation) continue;
                CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                tempID.IsTalking = false;
                tempID.UpdateAnims();
            }
        }
        #endregion
        
        #region PsycheLock

        if (line._extras.ShowPsycheLock)
        {
            _psycheLock.gameObject.SetActive(true);
            if (!_psycheLock.Locked) yield return _psycheLock.StartLock(line._extras.LockCount);
            if (line._extras.BreakChain)
            {
                if (_psycheLock._maxLockCount > 0) yield return _psycheLock.RemoveChains();
            }
            else if (line._extras.BreakLock)
            {
                if (_psycheLock._lockCount > 0) yield return _psycheLock.BreakLock();
            }
        }
        else
        {
            _psycheLock.gameObject.SetActive(false);
        }

        #endregion

        #region Play On Screen Animation

        if (line.ScreenAnimation != null)
        {
            _screenAnim = Instantiate(line.ScreenAnimation);
            if (line._extras.FlipScreenAnimation)
            {
                Vector3 localScale = _screenAnim.transform.localScale;
                
                localScale = new Vector3(localScale.x * -1,
                    localScale.y, localScale.z);
                
                _screenAnim.transform.localScale = localScale;
            }

            while (_screenAnim != null && line.StartTextAfterScreenAnimation)
            {
                yield return null;
            }
        }

        #endregion

        #region Show Evidence Icons

        if (line.LeftEvidence != null)
        {
            if (!Equals(_evidenceIcons[0].CurrentIcon, line.LeftEvidence))
            {
                if (!line.SkipIconAnimation && !quickEnd)
                {
                    if (line.WaitForIconAnimation) yield return StartCoroutine(_evidenceIcons[0].Expand(line.LeftEvidence));
                    else StartCoroutine(_evidenceIcons[0].Expand(line.LeftEvidence));
                }
                else
                {
                    _evidenceIcons[0].transform.localScale = Vector3.one;
                    _evidenceIcons[0].SetImage(line.LeftEvidence);
                }
            }
        }
        
        if (line.RightEvidence != null)
        {
            if (!Equals(_evidenceIcons[1].CurrentIcon, line.RightEvidence))
            {
                if (!line.SkipIconAnimation && !quickEnd)
                {
                    if (line.WaitForIconAnimation) yield return StartCoroutine(_evidenceIcons[1].Expand(line.RightEvidence));
                    else StartCoroutine(_evidenceIcons[1].Expand(line.RightEvidence));
                }
                else
                {
                    _evidenceIcons[1].transform.localScale = Vector3.one;
                    _evidenceIcons[1].SetImage(line.RightEvidence);
                }
            }
        }

        #endregion
        
        #region Show Textbox
        if (_hideOptions) _controlFlag.Hide();
        else _controlFlag.Show();
        
        _tempBox.ShowAll();
        
        _advanceButton.gameObject.SetActive(false);
        #endregion
        
        #region Start Text

        CharacterIdentifier identifier = null;

        if (_chars.Count > 0 && !line.Thinking && !String.IsNullOrWhiteSpace(Globals.RemoveRichText(totalTextMessage)))
        {
            foreach (Character chara in _chars)
            {
                CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                _charsIdentifiers.Add(tempID);
                if (chara.CharOnScreen.name == line._charTalking)
                {
                    identifier = tempID;
                    identifier.IsTalking = true;
                    
                    if (chara.DontWaitForOpeningAnimation) continue;
                    if (SkipTextButtonHeld) continue;
                    
                    tempID.IsTalking = true;
                    tempID.UpdateAnims();
                }
                else
                {
                    tempID.IsTalking = false;
                    tempID.UpdateAnims();
                }
            }
        }
        else if (_chars.Count > 0)
        {
            foreach (Character chara in _chars)
            {
                CharacterIdentifier tempID = chara.CharOnScreen.GetComponent<CharacterIdentifier>();
                _charsIdentifiers.Add(tempID);

                if (chara.CharOnScreen.name == line._charTalking)
                    identifier = tempID;

                if (chara.DontWaitForOpeningAnimation) continue;
                tempID.IsTalking = false;
                tempID.UpdateAnims();
            }
        }
        
        if ((String.Concat((totalTextMessage + prevText).Where(c => !Char.IsWhiteSpace(c))) == "" || (totalTextMessage + prevText) == null) && !line.ShowTextBox)
        {
            _controlFlag.Hide();
            _tempBox.HideAll();
            _mute = true;
        }
        else
        {
            _nameBox.text = line.Name == line._charTalking && identifier != null ? identifier.Name : line.Name;
            _nameBox.transform.parent.gameObject.SetActive(!line.HideNameTag);
        }

        Vector2 pitch = line.BlipSound == DialogueSoundTypes.Custom ? line._pitchVariationRange : Vector2.zero;
        float blip = line.BlipSound == DialogueSoundTypes.Custom && line._blipFrequency != 0 ? line._blipFrequency : 4f / 60f;

        if (line.BlipSound == DialogueSoundTypes.Default)
        {
            _typingClip = identifier != null ? identifier.DefaultBlipSound : "none";
            blip = identifier != null ? identifier.DefaultBlipFrequency : 4f / 60f;
            pitch = identifier != null ? identifier.DefaultPitchVariationRange : Vector2.zero;
        }
        
        
        _startedText = true;
        if (quickEnd)
        {
            typeRoutine =
                StartCoroutine(dialogueVertexAnimator.AnimateTextIn(commands, totalTextMessage, null, null, Globals.RemoveRichText(prevText).Length, pitchRange:pitch, blipFrequency:blip));
            dialogueVertexAnimator.QuickEnd(loopThroughEvents);
        } 
        else
        {
            typeRoutine =
                StartCoroutine(dialogueVertexAnimator.AnimateTextIn(commands, totalTextMessage, _typingClip, null, Globals.RemoveRichText(prevText).Length, pitchRange:pitch, blipFrequency:blip));
        }
        
        if (SkipTextButtonHeld)
        {
            if (_autoEnd)
            {
                if (lines[_currentLine].AddToPrevious && !lines[_currentLine].AutoEnd)
                {
                    dialogueVertexAnimator.QuickEnd();
                    yield return StartCoroutine(NextLine());
                    dialogueVertexAnimator.textAnimating = true;
                    dialogueVertexAnimator.QuickEnd();
                }
            }
            else
            {
                dialogueVertexAnimator.QuickEnd();
            }
        }

        _playerInput.SwitchCurrentActionMap("TextBox");
        #endregion
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

    IEnumerator EndDialogue()
    {
        _startedText = false;
        if (_testimonyInstance != null) Destroy(_testimonyInstance);
        if (_tempCourtRecord != null) _tempCourtRecord.GetComponent<Animator>().Play("Fade Out");
        while (_tempCourtRecord != null)
        {
            yield return null;
        }

        CourtRecordController courtRecordController = GameObject.FindObjectOfType<CourtRecordController>();
        Destroy(_tempBox.gameObject);
        _tempBoxPrefab = null;
        _playerInput.SwitchCurrentActionMap(_prevActionMap);
        
        if (_screenAnim != null)
        {
            Destroy(_screenAnim);
        }

        if (_dialogue.GoToTitleScreen)
        {
            SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
            spr.color = Color.black;
            _fade.transform.position = new Vector3(_fade.transform.position.x, _fade.transform.position.y, (int) BGFadePos.Everything);
            _fade.GetComponent<SpriteObjectsSortingOrder>().UpdateSortingOrders();
            
            _controlFlag.Show();
            _swap.KillChars();
            _swap._chars = new List<Character>();
            _chars = new List<Character>();
            _titleMenu.SetActive(true);
            gameObject.SetActive(false);
            _hpBar.ChangeHP(10);
            
            _evidenceIcons[0].transform.localScale = Vector3.zero;
            _evidenceIcons[0].SetImage(null);
            _evidenceIcons[1].transform.localScale = Vector3.zero;
            _evidenceIcons[1].SetImage(null);
            
            Globals.ResetProgress();
            Globals.ControlFlag.SetText(new string[] {}, true);
            _doneTalking = true;
            yield break;
        }

        if (_dialogue.dialogueType == DialogueOptions.PhaseEnd)
        {
            _dialogue = _dialogue.FindNextLine();
            Globals.ControlFlag.SetText(new string[] {}, true);
            _currentLine = 0;
            _savePrompt.SetActive(true);

            _background.sprite = null;
            _foreground.sprite = null;
            
            _controlFlag.Show();
            _swap.KillChars();
            _swap._chars = new List<Character>();
            _chars = new List<Character>();
            _hpBar.ChangeHP(10);
            _fade.transform.position = new Vector3(_fade.transform.position.x, _fade.transform.position.y, (int) BGFadePos.BackgroundGone);
            Globals.PhaseName = _dialogue.PhaseName;
            Globals.MusicManager.Stop();
        }
        else if (_dialogue.dialogueType == DialogueOptions.CaseEnd)
        {
            _dialogue = _dialogue.FindNextLine();
            Globals.ControlFlag.SetText(new string[] {}, true);
            _currentLine = 0;
            _savePrompt.SetActive(true);

            _background.sprite = null;
            _foreground.sprite = null;
            
            _controlFlag.Show();
            _swap.KillChars();
            _swap._chars = new List<Character>();
            _chars = new List<Character>();
            _hpBar.ChangeHP(10);
            _fade.transform.position = new Vector3(_fade.transform.position.x, _fade.transform.position.y, (int) BGFadePos.BackgroundGone);
            Globals.CaseNum += 1;
            Globals.CaseName = _dialogue.CaseName;
            Globals.PhaseName = _dialogue.PhaseName;
            Globals.MusicManager.Stop();
            
            GameSO gameSO = Resources.LoadAll<GameSO>($"Scriptable Objects/Titles").ToList().Find(x => x.GameName.Equals(Globals.GameName));
            
            AuthorGamePair author = Globals.UnlockedCases.Find(x => x.AuthorName == gameSO.GameAuthor);

            if (author == null)
            {
                author = new AuthorGamePair()
                {
                    AuthorName = Globals.GameAuthor,
                    Games = new List<GameCasesPair>()
                };
                
                Globals.UnlockedCases.Add(author);
            }
            
            GameCasesPair game = author.Games.Find(x => x.GameName == gameSO.GameName);

            if (game == null)
            {
                game = new GameCasesPair()
                {
                    GameName = Globals.GameName,
                    Cases = new List<CaseCompletionPair>()
                };
                
                author.Games.Add(game);
            }
            
            CaseCompletionPair caser = game.Cases.Find(x => x.Case == Globals.CaseName);

            if (caser == null)
            {
                caser = new CaseCompletionPair()
                {
                    Case = Globals.CaseName
                };
                
                game.Cases.Add(caser);
            }

            caser.Unlocked = true;

            SaveData data = FindObjectOfType<SaveData>();
            data.SaveCompletedCasesIntoJson();
        }
        else if (_revealInvestigationPanel)
        {
            _controlFlag.transform.localScale = new Vector3(1, 0, 1);
            _investigationMenuObj.SetActive(true);
            _controlFlag.Show();
        }
        else if (_presentImage)
        {
            _soundManager.Play("record flip");
            _controlFlag.SetText(new string[] {"PresentImage"}, true);
            GameObject obj = Instantiate(_presentImagePrefab);
            obj.GetComponentsInChildren<DialogueTrigger>()[0]._dialogue = _dialogue.wrongPresentSequence;
            _controlFlag.Show();
            yield break;
        }
        else if (!_dialogue.HasNextLine && courtRecordController == null)
        {
            _controlFlag.Hide();
        }
        else
        {
            if (_controlFlag.IsHidden) _controlFlag.transform.localScale = new Vector3(1, 0, 1);
            _controlFlag.Show();
            _controlFlag.SetText(new string[] {});
        }

        _doneTalking = true;
    }
    
    private IEnumerator ScrollBackground(TBLine line, bool quickEnd)
    {
        _isScrolling = true;
            Vector3 bgPos = new Vector3(0, 0, 0);
            
            if (!string.IsNullOrEmpty(line.Background.AssetGUID))
            {
                float width = _background.sprite.texture.width / 100f;
                float height = _background.sprite.texture.height / 100f;

                float startPosH = (-1 * width * ((int) line.HorizontalScrollStartPercentage / 100f)) + (width / 2);
                startPosH = Mathf.Clamp(startPosH, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);

                float endPosH = (-1 * width * ((int) line.HorizontalScrollEndPercentage / 100f)) + (width / 2);
                endPosH = Mathf.Clamp(endPosH, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);
                
                float startPosV = (-1 * height * ((int) line.VerticalScrollStartPercentage / 100f)) + (height / 2);
                startPosV = Mathf.Clamp(startPosV, (-height / 2f) + 5.4f, (height / 2f) - 5.4f);

                float endPosV = (-1 * height * ((int) line.VerticalScrollEndPercentage / 100f)) + (height / 2);
                endPosV = Mathf.Clamp(endPosV, (-height / 2f) + 5.4f, (height / 2f) - 5.4f);

                if ((line.WideBackground() && !quickEnd && Math.Abs(_scrollObj.transform.position.x - endPosH) > 0.1 && (Math.Abs(startPosH - endPosH) > 0.01)) ||
                    (line.TallBackground() && !quickEnd && Math.Abs(_scrollObj.transform.position.y - endPosV) > 0.1 && (Math.Abs(startPosV - endPosV) > 0.01)))
                {
                    _scrollObj.transform.position =
                        new Vector3(startPosH, startPosV, bgPos.z);
                    
                    float timePassed = 0;
                    float totalTime = 0.5f;
                    switch (line.ScrollSpeed)
                    {
                        case ScrollSpeedOptions.Default:
                            totalTime = 0.5f;
                            break;
                        case ScrollSpeedOptions.Fast:
                            totalTime = 0.25f;
                            break;
                        case ScrollSpeedOptions.Slow:
                            totalTime = 1.5f;
                            break;
                        case ScrollSpeedOptions.Custom:
                            totalTime = line.ScrollTime;
                            break;
                    }
                    
                    _swap.UpdateCharPos(new Vector2(startPosH * -1, startPosV));
                    List<Character> chara = _swap._chars.ToList();
                    
                    _swap.QuickSwap(_chars, Vector2.zero, line.HidePreviousCharWhileSwapping);
                    _chars = _swap._chars;
                    _swap.UpdateCharPos(new Vector2(endPosH * -1, endPosV * -1));

                    foreach (Character charb in _chars)
                    {
                        if (charb.CharOnScreen.GetComponent<CharacterIdentifier>().Is3DCharacter)
                        {
                            Animator anim = charb.CharOnScreen.GetComponent<Animator>();
                    
                            anim.CrossFade($"{charb.AnimPlaying}_idle", 0.5f);
                            anim.Play($"{charb.AnimPlaying}_mouth_idle", 2);
                        }
                        else charb.CharOnScreen.GetComponent<Animator>().Play($"{charb.AnimPlaying}_idle");
                        charb.CharOnScreen.GetComponent<Animator>().Update(0);
                    }

                    do
                    {
                        if (!_isScrolling) yield break;
                        float xPos = line.LinearScrolling ? Mathf.Lerp(startPosH, endPosH, timePassed / totalTime) : Mathf.SmoothStep(startPosH, endPosH, timePassed / totalTime);
                        float yPos = line.LinearScrolling ? Mathf.Lerp(startPosV, endPosV, timePassed / totalTime) : Mathf.SmoothStep(startPosV, endPosV, timePassed / totalTime);

                        if (totalTime == 0)
                        {
                            xPos = endPosH;
                            yPos = endPosV;
                        }

                        _scrollObj.transform.position =
                            new Vector3(xPos, yPos, bgPos.z);

                        timePassed += Time.deltaTime;

                        yield return null;
                    } while (timePassed < totalTime);

                    if (chara.Count > 0)
                    {
                        foreach (Character character in chara)
                        {
                            Destroy(character.CharOnScreen);
                        }
                    }
                    
                    _scrollObj.transform.position = new Vector3(endPosH, endPosV, _scrollObj.transform.localPosition.z);
                    if (totalTime != 0) yield return new WaitForSeconds(0.1f);
                }
                else if (line.WideBackground() || line.TallBackground())
                {
                    _scrollObj.transform.position = new Vector3(endPosH, endPosV, _scrollObj.transform.localPosition.z);
                    _swap.CharPos = new Vector2(endPosH * -1, endPosV * -1);
                }
                else
                {
                    _scrollObj.transform.position = bgPos;
                    _swap.UpdateCharPos(new Vector2(0, 0));
                }
            }

            _isScrolling = false;
    }

    private void OnDisable()
    {
        _background.sprite = null;
        _foreground.sprite = null;
        _swap.KillChars();
        _hpBar.ChangeHP(10);
        Globals.ResetProgress();
    }

    private bool WideBackground()
    {
        if (_background.sprite == null) return false;
        return _background.sprite.texture.width > 1920;
    }
    
    private static string RemoveRichTextDynamicTag (string str)
    {
        int index = -1;
        while (true)
        {
            index = str.IndexOf('(');
            if (index != -1)
            {
                int endIndex = str.Substring(index, str.Length - index).IndexOf(')');
                if (endIndex > 0)
                    str = str.Remove(index, endIndex + 1);
                continue;
            }
            str = RemoveRichTextTag(str);
            return str;
        }
    }
    private static string RemoveRichTextTag (string str)
    {
        while (true)
        {
            int index = str.IndexOf("()");
            if (index != -1)
            {
                str = str.Remove(index, 2);
                continue;
            }
            return str;
        }
    }
}
