using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = System.Object;

public class CaseSelectionMenu : MonoBehaviour
{
    [SerializeField] public GameObject _fade;
    [SerializeField] public GameObject _titleScreen;
    [SerializeField] public GameObject _titleMenu;
    [SerializeField] public GameObject _gameSelectMenu;
    [SerializeField] public GameObject _dialogueManager;
    [SerializeField] public GameObject _gameSelectStartingButton;
    [SerializeField] public GameObject _dropShadow;
    [SerializeField] public GameObject leftArrow;
    [SerializeField] public GameObject rightArrow;
    [SerializeField] public GameObject confirmSelection;
    [SerializeField] public ControlFlagController _controlFlag;
    [SerializeField] public GameObject _caseListContainer;
    [SerializeField] public CaseIconController _caseIconPrefab;
    [SerializeField] public CaseLogoPair[] _cases;
    [SerializeField] public string GameName;
    [SerializeField] public Color GameColor;
    [SerializeField] public GameSO Game;

    private PlayerInput _playerInput;
    private MusicManager _musicManager;
    private Image _sprite;
    private int _currentCase = 0;
    private SoundManager _soundManager;
    private List<CaseIconController> _caseList = new List<CaseIconController>();
    
    private InputAction _back;
    private bool _confirmSelection;
    
    IEnumerator Start()
    {
        SaveData data = FindObjectOfType<SaveData>();
        data.LoadCompletedCasesFromJson();
        
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _soundManager = Globals.SoundManager;
        _back = _playerInput.actions["Menu/Cancel"];
        _playerInput.SwitchCurrentActionMap("Null");
        _caseList = new List<CaseIconController>();
        _currentCase = 0;
        _confirmSelection = false;
        _caseListContainer.transform.localPosition = Vector2.zero;
        
        AuthorGamePair author = Globals.UnlockedCases.Find(x => x.AuthorName == Game.GameAuthor);
        GameCasesPair game = author?.Games.Find(x => x.GameName == Game.GameName);

        foreach (CaseLogoPair Case in _cases)
        {
            CaseIconController obj = Instantiate(_caseIconPrefab.gameObject, _caseListContainer.transform, true).GetComponent<CaseIconController>();
            Vector2 objPos = new Vector2(_caseList.Count * 730, 0);
            obj.transform.localPosition = objPos;
            obj.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            obj.gameObject.SetActive(true);
            _caseList.Add(obj.GetComponent<CaseIconController>());
            
            CaseCompletionPair caser = game?.Cases.Find(x => x.Case == Case.CaseName);

            bool unlocked = false;
            if (!ReferenceEquals(caser, null)) unlocked = caser.Unlocked;
            if (Case.LockedAtStart && !unlocked) obj.HideCase();
            else obj.UpdateCase(Case.CaseArt, Case.CaseNameArt, $"Episode {_caseList.Count}\n{Case.CaseName}");
        }

        leftArrow.GetComponentInChildren<Button>().interactable = false;
        rightArrow.GetComponentInChildren<Button>().interactable = false;

        StartCoroutine(_caseList[0].SetSelected(true));
        _caseList[0].gameObject.transform.localScale = Vector3.one;
        leftArrow.SetActive(false);
        
        bool isLocked = true;
        int i = 0;
        
        foreach (CaseIconController casea in _caseList)
        {
            if (!casea.CaseLocked && i > 0) isLocked = false;
            i++;
        }
        
        rightArrow.SetActive(!isLocked);

        _gameSelectStartingButton.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton);

        DiscordController.gameName = "Selecting Case";
        DiscordController.phaseName = $"({GameName})";
        
        _sprite = GetComponent<Image>();
        _sprite.enabled = true;

        _musicManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<MusicManager>();
        
        Vector3 position = _fade.transform.position;
        position = new Vector3(position.x, position.y, (int) BGFadePos.Everything);
        _fade.transform.position = position;

        SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
        Color startColor = Color.black;
        Color endColor = Color.clear;
        float time = 0;

        while (time < 0.5)
        {
            time += Time.deltaTime;
            spr.color = Color.Lerp(startColor, endColor, time / 0.5f);
            if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
            yield return null;
        }

        spr.color = endColor;
        
        _playerInput.SwitchCurrentActionMap("Menu");
        
        leftArrow.GetComponentInChildren<Button>().interactable = true;
        rightArrow.GetComponentInChildren<Button>().interactable = true;
        
        _controlFlag.SetText(new string[] {"Back"}, true);
    }

    private void OnEnable()
    {
        if (_soundManager != null) StartCoroutine(Start());
    }

    protected void Update()
    {
        if (_back.triggered)
        {
            if (_confirmSelection)
            {
                _soundManager.Play("back");
                StartCoroutine(Return(0));
            } else StartCoroutine(Leave());
        }
        
        if (_confirmSelection) return;
        if (EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton);
        if (EventSystem.current.currentSelectedGameObject == _gameSelectStartingButton) return;

        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;
        if (name == "Left")
        {
            StartCoroutine(ChangeCase(false));
        }
        else
        {
            StartCoroutine(ChangeCase(true));
        }
    }

    private IEnumerator Return(float delay=0.25f)
    {
        leftArrow.GetComponentInChildren<Button>().interactable = false;
        rightArrow.GetComponentInChildren<Button>().interactable = false;
        
        yield return new WaitForSeconds(delay);
        _confirmSelection = false;
        confirmSelection.SetActive(false);
        if (_currentCase != 0) leftArrow.SetActive(true);
        if (_currentCase != _caseList.Count - 1) rightArrow.SetActive(true);
        _gameSelectStartingButton.SetActive(true);
        _controlFlag.SetText(new string[] {"Back"}, true);
        EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton);
        
        leftArrow.GetComponentInChildren<Button>().interactable = true;
        rightArrow.GetComponentInChildren<Button>().interactable = true;
    }

    private IEnumerator ChangeCase(bool right)
    {
        _soundManager.Play("select");
        _playerInput.SwitchCurrentActionMap("null");
        _dropShadow.SetActive(false);
        
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton);

        Vector2 startPos = new Vector2(730 * _currentCase * -1, _caseListContainer.transform.localPosition.y);

        _caseList[_currentCase].transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        StartCoroutine(_caseList[_currentCase].SetSelected(false));
        
        _currentCase = FindNextAvailableCase(right);
        if (_currentCase < 0) _currentCase = _caseList.Count - 1;
        if (_currentCase >= _caseList.Count) _currentCase = 0;
        
        Vector2 endPos = new Vector2(730 * _currentCase * -1, _caseListContainer.transform.localPosition.y);
        
        StartCoroutine(_caseList[_currentCase].SetSelected(true));
        
        float time = 0;
        float duration = 0.3f;

        while (time < duration)
        {
            _caseListContainer.transform.localPosition = Vector2.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        _caseListContainer.transform.localPosition = endPos;
        _caseList[_currentCase].transform.localScale = Vector3.one;
        _dropShadow.SetActive(true);
        
        bool isLocked = true;
        int i = 0;
        
        foreach (CaseIconController casea in _caseList)
        {
            if (!casea.CaseLocked && i < _currentCase) isLocked = false;
            i++;
        }
        
        leftArrow.SetActive(!isLocked);
        
        isLocked = true;
        i = 0;
        
        foreach (CaseIconController casea in _caseList)
        {
            if (!casea.CaseLocked && i > _currentCase) isLocked = false;
            i++;
        }
        
        rightArrow.SetActive(!isLocked);
        
        _playerInput.SwitchCurrentActionMap("menu");
    }

    private int FindNextAvailableCase(bool right)
    {
        int dir = right ? 1 : -1;

        int currentCase = _currentCase;

        while (true)
        {
            currentCase += dir;
            if (!_caseList[currentCase].CaseLocked) break;
        }
        
        return currentCase;
    }

    public void Click()
    {
        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "Select":
                foreach (Button button in confirmSelection.GetComponentsInChildren<Button>())
                {
                    button.interactable = true;
                }
                
                _soundManager.Play("confirm");
                _confirmSelection = true;
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                _gameSelectStartingButton.SetActive(false);
                confirmSelection.SetActive(true);
                _controlFlag.SetText(new string[] {}, true);
                EventSystem.current.SetSelectedGameObject(confirmSelection.GetComponentsInChildren<Button>()[0].gameObject);
                confirmSelection.GetComponentInChildren<TMP_Text>().text = $"Play \"{_cases[_currentCase].CaseName}\"?";
                confirmSelection.GetComponentsInChildren<MenuCursor>()[0]._selectedButton = EventSystem.current.currentSelectedGameObject;
                break;
            case "No":
                foreach (Button button in confirmSelection.GetComponentsInChildren<Button>())
                {
                    if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
                    button.interactable = false;
                    button.animator.Play("Fade Out");
                }
                StartCoroutine(Return());
                break;
            case "Yes":
                foreach (Button button in confirmSelection.GetComponentsInChildren<Button>())
                {
                    if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
                    button.interactable = false;
                    button.animator.Play("Fade Out");
                }
                StartCoroutine(StartGame());
                break;
        }
    }

    IEnumerator Leave()
    {
        _playerInput.SwitchCurrentActionMap("Null");
        
        leftArrow.GetComponentInChildren<Button>().interactable = false;
        rightArrow.GetComponentInChildren<Button>().interactable = false;
        
        _controlFlag.SetText(new string[] {});
        _soundManager.Play("back");
        yield return new WaitForSeconds(0.25f);
        
        Vector3 position = _fade.transform.position;
        position = new Vector3(position.x, position.y, (int) BGFadePos.Everything);
        _fade.transform.position = position;

        SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
        Color startColor = spr.color;
        Color endColor = Color.black;
        float time = 0;

        while (time < 0.5)
        {
            time += Time.deltaTime;
            spr.color = Color.Lerp(startColor, endColor, time / 0.5f);
            if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
            yield return null;
        }

        foreach (CaseIconController Case in _caseList)
        {
            Destroy(Case.gameObject);
        }

        spr.color = endColor;
        _gameSelectMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    IEnumerator StartGame()
    {
        _playerInput.SwitchCurrentActionMap("Null");
        
        yield return new WaitForSeconds(0.25f);
        _musicManager.FadeOut(1.5f);
        
        Vector3 position = _fade.transform.position;
        position = new Vector3(position.x, position.y, (int) BGFadePos.Everything);
        _fade.transform.position = position;

        SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
        Color startColor = spr.color;
        Color endColor = Color.black;
        float time = 0;

        while (time < 0.5)
        {
            time += Time.deltaTime;
            spr.color = Color.Lerp(startColor, endColor, time / 0.5f);
            if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
            yield return null;
        }

        spr.color = endColor;
        yield return new WaitForSeconds(2f);
        
        foreach (CaseIconController Case in _caseList)
        {
            Destroy(Case.gameObject);
        }
        
        _gameSelectMenu.SetActive(false);
        _titleMenu.SetActive(false);
        gameObject.SetActive(false);
        _titleScreen.SetActive(true);
        _playerInput.SwitchCurrentActionMap("Menu");
        spr.color = Color.clear;
        _dialogueManager.SetActive(true);
        confirmSelection.SetActive(false);
        DialogueManager dm = _dialogueManager.GetComponent<DialogueManager>();
        Globals.GameName = GameName;
        Globals.GameAuthor = Game.GameAuthor;
        Globals.GameColor = GameColor;
        Globals.CaseName = _cases[_currentCase].CaseName;
        Globals.CaseNum = _currentCase + 1;
        dm.StartText(_cases[_currentCase].StartingDialogue);
    }
}
