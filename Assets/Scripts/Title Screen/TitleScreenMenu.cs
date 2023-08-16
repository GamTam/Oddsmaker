using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleScreenMenu : MenuCursor
{
    [SerializeField] public GameObject _fade;
    [SerializeField] public GameObject _titleMenu;
    [SerializeField] public GameObject _gameSelectMenu;
    [SerializeField] public GameObject _dialogueManager;
    [SerializeField] public GameObject confirmSelection;
    [SerializeField] public GameObject loadSave;
    [SerializeField] public Animator[] _buttonList;
    [SerializeField] public string _song;

    private SaveData _saveDataManager;
    private PlayerInput _playerInput;
    private InputAction _back;
    private MusicManager _musicManager;
    private Image _sprite;

    private bool _confirmSelection;
    public static bool FadeIn = true;
    
    new IEnumerator Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _playerInput.SwitchCurrentActionMap("Null");
        _saveDataManager = FindObjectOfType<SaveData>();
        
        _back = _playerInput.actions["Menu/Cancel"];
        
        base.Start();
        
        EventSystem.current.SetSelectedGameObject(_buttonList[1].gameObject);
        _selectedButton = _buttonList[1].gameObject;
        transform.position = _buttonList[1].gameObject.transform.position;

        DiscordController.gameName = "Title Screen";
        DiscordController.phaseName = "";
        
        _anim.Play("Null", 0, 0);
        _anim.Update(0);
        _sprite = GetComponent<Image>();
        _sprite.enabled = false;

        _musicManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<MusicManager>();
        //_musicManager.Play(_song);
        Globals.DialogueManager = _dialogueManager.GetComponent<DialogueManager>();
        _musicManager.PlayRandom();
        
        foreach (Animator button in _buttonList)
        {
            button.gameObject.SetActive(false);
        }

        if (FadeIn)
        {
            Vector3 position = _fade.transform.position;
            position = new Vector3(position.x, position.y, (int) BGFadePos.Everything);
            _fade.transform.position = position;

            SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
            Color startColor = Color.black;
            Color endColor = Color.clear;
            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime;
                spr.color = Color.Lerp(startColor, endColor, time / 1);
                if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
                yield return null;
            }

            spr.color = endColor;
        }
        else
        {
            SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
            spr.color = Color.clear;
        }

        foreach (Animator button in _buttonList)
        {
            button.gameObject.SetActive(true);
            button.GetComponent<Button>().interactable = true;
        }

        if (!File.Exists(Globals.SaveDataPath + "/Saves/FILE_0.law"))
        {
            _buttonList[1].gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(_buttonList[0].gameObject);
            transform.position = _buttonList[0].gameObject.transform.position;
            _selectedButton = _buttonList[0].gameObject;
        }
        
        _sprite.enabled = true;
        _anim.Play("Select Fade In", 0, 0);
        
        _playerInput.SwitchCurrentActionMap("Menu");
        FadeIn = true;
    }

    private void OnEnable()
    {
        if (_soundManager != null) StartCoroutine(Start());
    }

    private void Update()
    {
        if (_back.triggered)
        {
            if (_confirmSelection)
            {
                _soundManager.Play("back");
                StartCoroutine(Return(0));
            }
        }
    }

    public new void Click()
    {
        base.Click();
        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "New Game":
                StartCoroutine(Begin());
                break;
            case "Continue":
                StartCoroutine(Continue());
                break;
            case "Exit Game":
                _playerInput.SwitchCurrentActionMap("Null");
                foreach (Animator button in _buttonList)
                {
                    if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
                    button.Play("Fade Out");
                }
                StartCoroutine(LoadKillScreen());
                break;
            case "Yes":
                StartCoroutine(Kill());
                break;
            case "No":
                foreach (Button button in confirmSelection.GetComponentsInChildren<Button>())
                {
                    if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
                    button.animator.Play("Fade Out");
                }
                StartCoroutine(Return());
                break;
        }
    }

    IEnumerator Begin()
    {
        _playerInput.SwitchCurrentActionMap("Null");
        foreach (Animator button in _buttonList)
        {
            if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
            button.GetComponent<Button>().interactable = false;
            button.Play("Fade Out");
        }
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
        yield return null;
        _gameSelectMenu.SetActive(true);
        _titleMenu.SetActive(false);
    }
    
    IEnumerator Continue()
    {
        // _playerInput.SwitchCurrentActionMap("null");
        foreach (Animator button in _buttonList)
        {
            if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
            button.Play("Fade Out");
        }
        yield return new WaitForSeconds(0.25f);
        GameObject obj = Instantiate(loadSave);
        obj.GetComponent<SaveScreenController>().PrevMenu = _titleMenu.transform.parent.parent.gameObject;
        obj.SetActive(true);
        _titleMenu.transform.parent.parent.gameObject.SetActive(false);
        FadeIn = false;
    }

    private IEnumerator LoadKillScreen()
    {
        yield return new WaitForSeconds(0.25f);
        
        foreach (Animator button in _buttonList)
        {
            button.gameObject.SetActive(false);
        }
        
        _confirmSelection = true;
        confirmSelection.SetActive(true);
        _anim.Play("Select Fade In");
        _anim.Update(0);
        EventSystem.current.SetSelectedGameObject(confirmSelection.GetComponentsInChildren<Button>()[1].gameObject);
        _selectedButton = EventSystem.current.currentSelectedGameObject;
        _playerInput.SwitchCurrentActionMap("Menu");
    }
    
    private IEnumerator Return(float delay=0.25f)
    {
        yield return new WaitForSeconds(delay);
        _confirmSelection = false;
        confirmSelection.SetActive(false);
        
        foreach (Animator button in _buttonList)
        {
            button.gameObject.SetActive(true);
        }
        
        EventSystem.current.SetSelectedGameObject(_buttonList[1].gameObject);
        transform.position = _buttonList[1].gameObject.transform.position;
        _selectedButton = _buttonList[1].gameObject;

        if (!File.Exists(Globals.SaveDataPath + "/Saves/FILE_0.law"))
        {
            _buttonList[1].gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(_buttonList[0].gameObject);
            transform.position = _buttonList[0].gameObject.transform.position;
            _selectedButton = _buttonList[0].gameObject;
        }
        
        _anim.Play("Select Fade In");
        _selectedButton = EventSystem.current.currentSelectedGameObject;
    }

    IEnumerator Kill()
    {
        _playerInput.SwitchCurrentActionMap("null");
        foreach (Button button in confirmSelection.GetComponentsInChildren<Button>())
        {
            if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
            button.interactable = false;
            button.animator.Play("Fade Out");
        }
        yield return new WaitForSeconds(0.5f);
        
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
             Application.Quit();
        #endif
    }
}
