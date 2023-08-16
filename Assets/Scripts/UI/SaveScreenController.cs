using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SaveScreenController : MonoBehaviour
{
    [SerializeField] private GameObject _saveIconBase;
    [SerializeField] private GameObject _saveList;
    [SerializeField] private GameObject _confirmMenu;
    [SerializeField] private GameObject _confirmMenuButtons;
    [SerializeField] private GameObject _confirmMenuLoading;
    [SerializeField] private MenuCursor _mainMenuCursor;
    [SerializeField] private TMP_Text _messageText;
    [Space] 
    public GameObject PrevMenu;
    [SerializeField] [ReadOnly] public bool _saveMode = false;
    [SerializeField] [ReadOnly] public bool _startDialogueOnLeave = false;
    [SerializeField] [ReadOnly] private SaveData _saveLoader;
    [SerializeField] [ReadOnly] private List<SaveIconController> _saves = new List<SaveIconController>();
    
    private PlayerInput _playerInput;
    private InputAction _back;
    private int saveNum = 0;

    private void Awake()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _back = _playerInput.actions["Menu/Cancel"];
        GetComponent<Canvas>().worldCamera = Camera.main;

        _saveLoader = FindObjectOfType<SaveData>();
    }
    
    private void OnEnable()
    {
        foreach (SaveIconController save in _saves)
        {
            Destroy(save.gameObject);
        }
        _saves = new List<SaveIconController>();
        
        if (!Directory.Exists(Globals.SaveDataPath + "/Saves")) Directory.CreateDirectory(Globals.SaveDataPath + "/Saves");

        int saveCount = Directory.GetFiles(Globals.SaveDataPath + "/Saves", "*", SearchOption.AllDirectories).Length;

        for (int i = 0; i < Mathf.Max(saveCount, 10); i++)
        {
            GameObject obj = Instantiate(_saveIconBase, _saveList.transform);
            obj.SetActive(true);
            _saves.Add(obj.GetComponent<SaveIconController>());
            
            _saves[^1].UpdateIcon(_saveLoader.GenerateSaveIcon(i), i + 1, _saveMode);
        }

        EventSystem.current.SetSelectedGameObject(_saves[Globals.SaveFile].gameObject);
    }

    private void Start()
    {
        if (_saveMode) _messageText.text = "Select a slot to save in.";
    }

    private void Update()
    {
        if (_back.triggered)
        {
            if (!_mainMenuCursor.StopUpdating)
            {
                Globals.SoundManager.Play("back");
                if (PrevMenu != null) PrevMenu.SetActive(true);
                if (_startDialogueOnLeave) Globals.DialogueManager.StartText(Globals.DialogueManager._dialogue, waitTime:1f);
                Destroy(gameObject);
            }
            else
            {
                Globals.SoundManager.Play("back");
                FinalCloseConfirmMenu();
            }
        }
    }

    public void OpenConfirmMenu()
    {
        Globals.SoundManager.Play("confirm");
        _mainMenuCursor.StopUpdating = true;
        foreach (SaveIconController icon in _saves)
        {
            Button iconButton = icon.GetComponent<Button>();
            iconButton.interactable = false;
            if (icon.gameObject == _mainMenuCursor._selectedButton)
            {
                saveNum = icon.FindSaveNumber();
            }
        }
        _confirmMenu.SetActive(true);
        _confirmMenu.GetComponentInChildren<MenuCursor>()._selectedButton =
            _confirmMenu.GetComponentsInChildren<Button>()[1].gameObject;
        if (_saveMode) _confirmMenuButtons.GetComponent<TMP_Text>().text = "Overwrite the selected saved data?";
        SaveIconData data = _saveLoader.GenerateSaveIcon(saveNum);
        _confirmMenu.GetComponentInChildren<SaveIconController>().UpdateIcon(data, saveNum + 1, true);
        if (data == null) StartLoad(false);
    }

    public void CloseConfirmMenu()
    {
        foreach (Button button in _confirmMenu.GetComponentsInChildren<Button>())
        {
            if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
            button.animator.Play("Fade Out");
        }
        StartCoroutine(CloseMenuEnum());
    }

    private IEnumerator CloseMenuEnum()
    {
        yield return new WaitForSeconds(0.25f);
        FinalCloseConfirmMenu();
    }

    private void FinalCloseConfirmMenu()
    {
        EventSystem.current.SetSelectedGameObject(_mainMenuCursor._selectedButton);
        _mainMenuCursor.StopUpdating = false;
        
        foreach (SaveIconController icon in _saves)
        {
            Button iconButton = icon.GetComponent<Button>();
            iconButton.interactable = true;
            if (icon.gameObject == _mainMenuCursor._selectedButton)
            {
                icon.UpdateIcon(_saveLoader.GenerateSaveIcon(icon.FindSaveNumber()));
            }
        }
        _confirmMenu.SetActive(false);
    }

    public void StartLoad(bool wait = true)
    {
        foreach (Button button in _confirmMenu.GetComponentsInChildren<Button>())
        {
            if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
            button.animator.Play("Fade Out");
        }
        StartCoroutine(LoadGame(wait));
    }

    private IEnumerator LoadGame(bool wait = true)
    {
        Globals.SaveFile = saveNum;
        if (wait) yield return new WaitForSeconds(0.25f);
        _confirmMenuButtons.SetActive(false);
        _confirmMenuLoading.SetActive(true);
        if (_saveMode)
        {
            _confirmMenuLoading.GetComponent<Animator>().Play("Book Animating");
            _confirmMenuLoading.GetComponentInChildren<TMP_Text>().text =
                "Saving content.\nPlease do not turn off your PC.";
        }
        yield return new WaitForSeconds(1.5f);
        if (_saveMode)
        {
            _saveLoader.SaveGameIntoJson();
            FinalCloseConfirmMenu();
            _confirmMenuButtons.SetActive(true);
            _confirmMenuLoading.SetActive(false);
        }
        else
        {
            Globals.MusicManager.Stop();
            _confirmMenuLoading.GetComponentInChildren<Animator>().speed = 0f;
            yield return new WaitForSeconds(0.25f);
            Globals.DialogueManager.gameObject.SetActive(true);
            _saveLoader.SaveSettingsIntoJson();
            _saveLoader.LoadGameFromJson();
            Globals.InOptions = false;
            Destroy(gameObject);
        }
    }
}
