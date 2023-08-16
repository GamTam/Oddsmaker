using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InvestigationMenu : MenuCursor
{
    [SerializeField] private GameObject _investigateCursor;
    [SerializeField] private GameObject _talkPrefab;
    [SerializeField] private GameObject _movePrefab;
    [SerializeField] private GameObject _presentPrefab;
    [SerializeField] private Button[] _buttons;
    public ControlFlagController _controlFlag;
    public List<TalkSO> _talkText = new List<TalkSO>();
    public List<MoveSO> _moveablePlaces = new List<MoveSO>();
    public EvidenceTalkPair[] _evidenceDialogue;
    public DialogueSO _wrongEvidence;
    public GameObject _interactables;
    
    [Header("Misc.")] [SerializeField] public string _song;
    [SerializeField] SwapCharacters _swap;
    [SerializeField] private GameObject _background;
    [SerializeField] private SpriteRenderer _map;

    private DialogueManager _dialogueManager;

    private PlayerInput _playerInput;
    
    private InputAction _slide;

    private MusicManager _musicManager;
    private Navigation _nav = new Navigation();

    private bool _sliding;

    [Serializable]
    public struct EvidenceTalkPair {
        public EvidenceSO Evidence;
        public DialogueSO Dialogue;
        public string[] Conditions;
    }
    
    [Serializable]
    public struct FlagTalkPair {
        public string[] Flags;
        public DialogueSO Dialogue;
    }

    void Awake()
    {
        base.Start();
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();

        _dialogueManager = GameObject.FindGameObjectWithTag("Dialogue Manager").GetComponent<DialogueManager>();

        _musicManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<MusicManager>();

        _slide = _playerInput.actions["Menu/Slide"];
    }

    private void OnEnable()
    {
        _controlFlag.SetText(WideBackground() ? new string[] {"Options", "Slide", "Court Record"} : new string[] {"Options", "Court Record", "John"});
        _nav.wrapAround = true;
        _nav.mode = Navigation.Mode.Horizontal;

        foreach (Button button in _buttons)
        {
            button.navigation = _nav;
            button.gameObject.SetActive(true);
        }
        
        _playerInput.SwitchCurrentActionMap("Menu");
        _background.transform.localScale = new Vector3(1, 0, 1);
        StartCoroutine(BackgroundAnimIn());

        if (_talkText.Count == 0) _buttons[2].gameObject.SetActive(false);
        
        if (_evidenceDialogue.Length == 0 && _wrongEvidence == null) _buttons[3].gameObject.SetActive(false);

        if (_moveablePlaces.Count == 0) _buttons[1].gameObject.SetActive(false);
        
        if (_interactables == null) _buttons[0].gameObject.SetActive(false);
        
        if (_selectedButton == null)
        {
            if (_buttons[0].gameObject.activeSelf) _selectedButton = _buttons[0].gameObject;
            else if (_buttons[1].gameObject.activeSelf) _selectedButton = _buttons[1].gameObject;
            else if (_buttons[2].gameObject.activeSelf) _selectedButton = _buttons[2].gameObject;
            else if (_buttons[3].gameObject.activeSelf) _selectedButton = _buttons[3].gameObject;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_selectedButton);
    }

    private IEnumerator BackgroundAnimIn()
    {
        _background.transform.localScale = new Vector2(1, 0);
        for(float i = 0f; i < 1f; i += 0.1f)
        {
            _background.transform.localScale = new Vector2(1, i);
            yield return new WaitForSeconds(1/60f);
        }
        _background.transform.localScale = new Vector2(1, 1);
    }

    private IEnumerator BackgroundAnimOut()
    {
        _background.transform.localScale = new Vector2(1, 1);
        for(float i = 1f; i > 0f; i -= 0.1f)
        {
            _background.transform.localScale = new Vector2(1, i);
            yield return new WaitForSeconds(1/60f);
        }
        _background.transform.localScale = new Vector2(1, 0);
    }

    public new void Update()
    {
        if (_slide.triggered && WideBackground() && !_sliding)
        {
            Globals.SoundManager.Play("confirm");
            _playerInput.SwitchCurrentActionMap("Null");
            StartCoroutine(Slide());
        }
    }

    public new void Click()
    {
        base.Click();
        _playerInput.SwitchCurrentActionMap("Null");
        StartCoroutine(BackgroundAnimOut());
        
        foreach (GameObject but in GameObject.FindGameObjectsWithTag("Button"))
        {
             if (but != EventSystem.current.currentSelectedGameObject) but.GetComponent<Animator>().Play("Fade Out");
        }

        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "Examine":
                Globals.ControlFlag.SetText(WideBackground()
                    ? new string[] {"Options", "Slide", "Court Record", "Back"}
                    : new string[] {"Options", "Back", "Court Record"});
                StartCoroutine(Examine());
                break;
            case "Move":
                Globals.ControlFlag.SetText(new string[] {"Options", "Back", "Court Record"});
                StartCoroutine(Move());
                break;
            case "Talk":
                Globals.ControlFlag.SetText(new string[] {"Options", "Back", "Court Record"});
                StartCoroutine(Talk());
                break;
            case "Present":
                Globals.ControlFlag.SetText(new string[] {});
                StartCoroutine(Present());
                break;
        }
    }

    private IEnumerator Examine()
    {
        _swap.StartSwap(_swap._charPrefabs, fadeIn:false);
        yield return new WaitForSeconds(0.25f);
        _playerInput.SwitchCurrentActionMap("Investigation");
        Instantiate(_investigateCursor, (Vector2) Camera.main.transform.position, Quaternion.identity);
        GameObject.FindWithTag("Respawn").SetActive(false);
        _controlFlag.transform.SetSiblingIndex(_controlFlag.transform.parent.childCount);
    }
    
    private IEnumerator Talk()
    {
        yield return new WaitForSeconds(0.25f);
        
        GameObject obj = Instantiate(_talkPrefab);
        obj.transform.SetParent(GameObject.FindWithTag("Respawn").transform, false);
        TalkManager talk = obj.GetComponent<TalkManager>();
        talk._investigation = this;
        talk.ShowOptions(_talkText);
        
        GameObject.FindWithTag("Investigation").SetActive(false);
        _playerInput.SwitchCurrentActionMap("Menu");
        _controlFlag.transform.SetSiblingIndex(_controlFlag.transform.parent.childCount);
    }
    
    private IEnumerator Move()
    {
        yield return new WaitForSeconds(0.25f);
        
        GameObject obj = Instantiate(_movePrefab);
        obj.transform.SetParent(GameObject.FindWithTag("Respawn").transform, false);
        MoveManager move = obj.GetComponent<MoveManager>();
        move.ShowOptions(_moveablePlaces);
        
        GameObject.FindWithTag("Investigation").SetActive(false);
        _playerInput.SwitchCurrentActionMap("Menu");
        _controlFlag.transform.SetSiblingIndex(_controlFlag.transform.parent.childCount);
    }
    
    private IEnumerator Present()
    {
        yield return new WaitForSeconds(0.25f);
        
        GameObject obj = Instantiate(_presentPrefab);
        obj.transform.SetParent(GameObject.FindGameObjectWithTag("UI").transform, false);
        CRPresent pres = obj.GetComponent<CRPresent>();
        pres.enabled = true;
        
        GameObject.FindWithTag("Investigation").SetActive(false);
        _playerInput.SwitchCurrentActionMap("Menu");
        _controlFlag.transform.SetSiblingIndex(_controlFlag.transform.parent.childCount);
    }
    
    private IEnumerator Slide()
    {
        _sliding = true;
        Vector3 bgPos = new Vector3(0, 0, 0);
        float width = _map.sprite.texture.width / 100f;

        float leftSide = (-1 * width * ((int) 0 / 100f)) + (width / 2);
        leftSide = Mathf.Clamp(leftSide, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);

        float rightSide = (-1 * width * (100 / 100f)) + (width / 2);
        rightSide = Mathf.Clamp(rightSide, (-width / 2f) + 9.6f, (width / 2f) - 9.6f);

        GameObject scrollObj = _map.transform.parent.gameObject;

        float startPos = scrollObj.transform.position.x;
        float endPos;

        if (Math.Abs(startPos - leftSide) < 0.1)
        {
            endPos = rightSide;
        }
        else
        {
            endPos = leftSide;
        }
        
        float timePassed = 0;
        float totalTime = 1f;

        do
        {
            float xPos = Mathf.Lerp(startPos, endPos, timePassed / totalTime);

            scrollObj.transform.position =
                new Vector3(xPos, bgPos.y, bgPos.z);

            timePassed += Time.deltaTime;

            yield return null;
        } while (timePassed < totalTime);

        scrollObj.transform.position = new Vector3(endPos, 0, scrollObj.transform.localPosition.z);
        _sliding = false;
        
        _playerInput.SwitchCurrentActionMap("Menu");
    }
    
    private bool WideBackground()
    {
        if (_map.sprite == null) return false;
        return _map.sprite.texture.width > 1920;
    }
}