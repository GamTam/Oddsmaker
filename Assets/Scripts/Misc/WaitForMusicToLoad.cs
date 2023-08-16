using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WaitForMusicToLoad : MonoBehaviour
{
    [SerializeField] private GameObject _titleScreen;
    [SerializeField] private GameObject _editorMenu;
    [SerializeField] private Image _splash;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _fadeTime;
    [SerializeField] private float _waitTime;
    
    [Space]
    [SerializeField] private bool _editor;
    
    private PlayerInput _playerInput;
    private InputAction _confirm;
    
    [RuntimeInitializeOnLoadMethod]
    static void ResetResolution()
    {
        Screen.SetResolution(1280, 720, false);
    }

    private IEnumerator Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _playerInput.SwitchCurrentActionMap("Menu");
        _confirm = _playerInput.actions["Menu/Submit"];

        // Disable everything but Capcom logo
        int i = 0;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (i != 0) child.gameObject.SetActive(false);
            i++;
        }
        
        _splash.gameObject.SetActive(true);
        
        _splash.color = Color.clear;


        // Fade Capcom logo in and out
        float time = 0;
        float duration = _fadeTime;

        while (time < duration)
        {
            time += Time.deltaTime;
            _splash.color = Color.Lerp(Color.clear, Color.white, time / duration);
            yield return null;
        }

        _splash.color = Color.white;
        time = _waitTime;

        while (time > 0)
        {
            time -= Time.deltaTime;
            if (Keyboard.current[Key.Enter].isPressed || Mouse.current.leftButton.isPressed || (_playerInput.currentControlScheme != "Keyboard" && _confirm.triggered))
            {
                break;
            }

            yield return null;
        }

        time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            _splash.color = Color.Lerp(Color.white, Color.clear, time / duration);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);


        // Flash press enter text until enter is pressed
        _text.gameObject.SetActive(true);
        time = 0;
        
        while (!Keyboard.current[Key.Enter].isPressed && !Mouse.current.leftButton.isPressed && !(_playerInput.currentControlScheme != "Keyboard" && _confirm.triggered))
        {
            time += Time.deltaTime;
            _text.color = new Color(1, 1, 1, -Mathf.Cos(time * 3f) / 2 + 0.5f);
            yield return null;
        }
        
        StartCoroutine(Globals.SoundManager.LoadSounds());
        StartCoroutine(Globals.MusicManager.LoadSongs());

        // Load waiting screen
        i = 0;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (i != 0) child.gameObject.SetActive(true);
            i++;
        }
        
        _splash.gameObject.SetActive(false);
        _text.gameObject.SetActive(false);
        
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        // Make sure that the Music Manager and Sound Manager have initialized 
        while (Globals.MusicManager == null || Globals.SoundManager == null)
        {
            yield return null;
        }

        // Wait for Music Manager and Sound Manager to load all songs and sound effects
        while (!Globals.MusicManager.DoneLoading || !Globals.SoundManager.DoneLoading)
        {
            yield return null;
        }


        // Disable object and either load editor or title screen depending on settings
        int i = 0;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (i != 0) child.gameObject.SetActive(false);
            i++;
        }

        yield return new WaitForSeconds(0.2f);
        
        if (!_editor) _titleScreen.SetActive(true);
        else _editorMenu.SetActive(true);
        
        SaveData data = FindObjectOfType<SaveData>();
        data.LoadSettingsFromJson();
    }

    // Reset window when the game closes
    public void OnApplicationQuit()
    { 
        Screen.SetResolution(1280, 720, false);
        PlayerPrefs.DeleteAll();
    }
}
