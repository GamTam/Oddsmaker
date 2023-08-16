using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameSelectMenu : MenuCursor
{
    [SerializeField] public GameObject _fade;
    [SerializeField] public GameObject _titleScreen;
    [SerializeField] public CaseSelectionMenu _caseSelectionMenu;
    [SerializeField] public GameObject _gameSelectMenu;
    [SerializeField] public Button _gameSelectStartingButton;
    [SerializeField] public Image[] _logo;
    [SerializeField] public TMP_Text[] _altLogo;
    [SerializeField] public ControlFlagController _controlFlag;
    [SerializeField] public Button[] _buttonList;
    [SerializeField] private GameSO[] _gameList;

    private PlayerInput _playerInput;
    private MusicManager _musicManager;
    private Image _sprite;
    private int _currentGame = 0;

    private InputAction _back;
    
    new IEnumerator Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _back = _playerInput.actions["Menu/Cancel"];
        _playerInput.SwitchCurrentActionMap("Null");
        _gameList = Resources.LoadAll<GameSO>($"Scriptable Objects/Titles");
        if (_gameList[0].Logo != null) _logo[0].sprite = _gameList[0].Logo;
        else
        {
            _logo[0].sprite = null;
            _altLogo[0].enabled = true;
            _logo[0].enabled = false;
            _altLogo[0].text = _gameList[0].GameName;
        }
        
        foreach (Button button in _buttonList)
        {
            button.interactable = false;
        }

        _gameSelectStartingButton.interactable = false;
        
        EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton.gameObject);

        base.Start();

        DiscordController.gameName = "Selecting Game";
        DiscordController.phaseName = "";
        
        _anim.Play("Select Fade In");
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

        foreach (Button button in _buttonList)
        {
            button.interactable = true;
        }

        _gameSelectStartingButton.interactable = true;
        
        spr.color = endColor;
        
        _playerInput.SwitchCurrentActionMap("Menu");
        _controlFlag.SetText(new string[] {"Back"}, true);
    }

    private void OnEnable()
    {
        if (_soundManager != null) StartCoroutine(Start());
        
    }

    protected new void Update()
    {
        if (_back.triggered) StartCoroutine(Leave());
        if (EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton.gameObject);
        if (EventSystem.current.currentSelectedGameObject == _gameSelectStartingButton.gameObject) return;

        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;
        if (name == "Left")
        {
            StartCoroutine(ChangeTitle(false));
        }
        else
        {
            StartCoroutine(ChangeTitle(true));
        }
    }

    private IEnumerator ChangeTitle(bool right)
    {
        _soundManager.Play("select");
        int dir = right ? -1 : 1;

        EventSystem.current.SetSelectedGameObject(_gameSelectStartingButton.gameObject);
        foreach (Button button in _buttonList)
        {
            button.gameObject.SetActive(false);
        }

        _sprite.enabled = false;
        _anim.Play("Null");
        _gameSelectStartingButton.gameObject.SetActive(false);
        
        if (_gameList[_currentGame].Logo != null)
        {
            _logo[0].sprite = _gameList[_currentGame].Logo;
            _altLogo[0].enabled = false;
            _logo[0].enabled = true;
        }
        else
        {
            _logo[0].sprite = null;
            _altLogo[0].enabled = true;
            _logo[0].enabled = false;
            _altLogo[0].text = _gameList[_currentGame].GameName;
        }
        
        
        _currentGame += dir * -1;
        if (_currentGame < 0) _currentGame = _gameList.Length - 1;
        if (_currentGame >= _gameList.Length) _currentGame = 0;
        
        if (_gameList[_currentGame].Logo != null)
        {
            _logo[1].sprite = _gameList[_currentGame].Logo;
            _altLogo[1].enabled = false;
            _logo[1].enabled = true;
        }
        else
        {
            _logo[1].sprite = null;
            _altLogo[1].enabled = true;
            _logo[1].enabled = false;
            _altLogo[1].text = _gameList[_currentGame].GameName;
        }

        Vector2 startPos = new Vector2(0, _logo[0].gameObject.transform.localPosition.y);
        Vector2 endPos = new Vector2(1920 * dir, startPos.y);
        Vector2 startPosAlt = new Vector2(1920 * dir * -1, startPos.y);

        _logo[0].gameObject.transform.localPosition = startPos;
        _logo[1].gameObject.transform.localPosition = startPosAlt;

        float time = 0;
        float duration = 0.375f;

        while (time < duration)
        {
            _logo[0].transform.localPosition = Vector2.Lerp(startPos, endPos, time / duration);
            _logo[1].transform.localPosition = Vector2.Lerp(startPosAlt, startPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        _logo[0].transform.localPosition = endPos;
        _logo[1].transform.localPosition = startPos;
        
        foreach (Button button in _buttonList)
        {
            button.gameObject.SetActive(true);
        }

        _sprite.enabled = true;
        _anim.Play("Select Fade In");
        _gameSelectStartingButton.gameObject.SetActive(true);
    }

    public new void Click()
    {
        base.Click();
        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "Play This Game":
                StartCoroutine(StartGame());
                break;
        }
    }

    IEnumerator Leave()
    {
        _playerInput.SwitchCurrentActionMap("Null");

        foreach (Button button in _buttonList)
        {
            button.interactable = false;
        }

        _gameSelectStartingButton.interactable = false;
        
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

        spr.color = endColor;
        _gameSelectMenu.SetActive(false);
        _titleScreen.SetActive(true);
    }
    
    IEnumerator StartGame()
    {
        _playerInput.SwitchCurrentActionMap("Null");
        
        foreach (Button button in _buttonList)
        {
            button.interactable = false;
        }
        
        _controlFlag.SetText(new string[] {});
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

        spr.color = endColor;
        _gameSelectMenu.SetActive(false);
        _caseSelectionMenu.Game = _gameList[_currentGame];
        _caseSelectionMenu.GameName = _gameList[_currentGame].GameName;
        _caseSelectionMenu.GameColor = _gameList[_currentGame].GameColor;
        _caseSelectionMenu._cases = _gameList[_currentGame].Cases;
        _caseSelectionMenu.gameObject.SetActive(true);
    }
}
