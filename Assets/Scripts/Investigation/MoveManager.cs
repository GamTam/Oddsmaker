using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveManager : MonoBehaviour
{
    [SerializeField] private RectTransform responseButton;
    [SerializeField] private Transform responseButtonLocation;
    [SerializeField] private GameObject _selectionHead;
    [SerializeField] private RectTransform _window;
    [SerializeField] private Image _windowImage;
    [SerializeField] private GameObject _unknownImage;
    [SerializeField] private GameObject _sceneTransition;

    private Dictionary<RectTransform, MoveSO> _responseButtons = new Dictionary<RectTransform, MoveSO>();

    private PlayerInput _playerInput;
    private InputAction _back;
    private SoundManager _soundManager;
    private bool _revealing;
    private GameObject _selectedButton;
    
    private bool _shownResponses;
    private Image _sprite;
    private bool _killing;
    private bool _windowOpen;

    private void Awake()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        
        _back = _playerInput.actions["Menu/Cancel"];
    }

    private IEnumerator Start()
    {
        if (_sprite == null) _sprite = GetComponent<Image>();
        
        _sprite.color = new Color(0f, 0f, 0f, 0f);

        StartCoroutine(OpenWindow());
        for (int i = 0; i < 100; i += 20)
        {
            _sprite.color = new Color(0f, 0f, 0f, i/255f);
            yield return new WaitForSeconds(1 / 60f);
        }
        
        _sprite.color = new Color(0f, 0f, 0f, 100/255f);
    }

    private IEnumerator OpenWindow()
    {
        _windowOpen = false;

        for (float i = 90; i > 0; i -= 6f)
        {
            _window.transform.rotation = Quaternion.Euler(new Vector3(i, 0, 0));
            yield return new WaitForSeconds(1 / 60f);
        }
        _window.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        _windowOpen = true;
    }
    
    private IEnumerator CloseWindow()
    {
        _windowOpen = true;

        for (float i = 0; i < 90; i += 6)
        {
            _window.transform.rotation = Quaternion.Euler(new Vector3(i, 0, 0));
            yield return new WaitForSeconds(1 / 60f);
        }
        _window.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        _windowOpen = false;
    }

    private void Update()
    {
        if (_selectedButton != EventSystem.current.currentSelectedGameObject)
        {
            foreach (KeyValuePair<RectTransform, MoveSO> rect in _responseButtons)
            {
                if (rect.Key.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    _windowImage.sprite = rect.Value.Preview;
                    bool hideUnknownImage = rect.Value.KnownFromStart || Globals.KnownLocations.Contains(rect.Value);
                    _unknownImage.SetActive(!hideUnknownImage);
                }
            }

            _selectedButton = EventSystem.current.currentSelectedGameObject;
        }
        
        if (_killing) return;

        if (_back.triggered)
        {
            StartCoroutine(Kill());
        }
    }

    public void ShowOptions(List<MoveSO> responses)
    {
        _selectionHead.SetActive(true);
        int i = 0;
        foreach (MoveSO response in responses)
        {
            if (!Globals.CheckStoryFlags(response.ConditionFlags)) continue;
            
            RectTransform responseButton = Instantiate(this.responseButton, responseButtonLocation, false);
            responseButton.gameObject.SetActive(true);
            responseButton.localScale = new Vector3(1, 1, 1);
            responseButton.gameObject.GetComponentInChildren<TMP_Text>().text = response.Name;
            responseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnPickedResponse(response)));

            _responseButtons.Add(responseButton, response);
            
            if (i==0) EventSystem.current.SetSelectedGameObject(responseButton.gameObject);
            i++;
        }
    }

    private IEnumerator OnPickedResponse(MoveSO response)
    {
        _playerInput.SwitchCurrentActionMap("Null");
        StartCoroutine(CloseWindow());
        foreach (GameObject but in GameObject.FindGameObjectsWithTag("Button"))
        {
             if (but != EventSystem.current.currentSelectedGameObject) but.GetComponent<Animator>().Play("Fade Out");
        }

        for (int i = 100; i > 0; i -= 20)
        {
            _sprite.color = new Color(0f, 0f, 0f, i/255f);
            yield return new WaitForSeconds(1 / 60f);
        }
        _sprite.color = new Color(0f, 0f, 0f, 0f);
        
        yield return new WaitForSeconds(0.5f);
        
        foreach (KeyValuePair<RectTransform, MoveSO> button in _responseButtons)
        {
            Destroy(button.Key.gameObject);
        }

        _selectionHead.SetActive(false);
        
        yield return new WaitUntil(() => !_windowOpen);
        GameObject obj = Instantiate(_sceneTransition);
        obj.GetComponent<SceneTransition>()._destination = response.Scene;
        obj.GetComponent<SceneTransition>()._knownLocation = response.KnownFromStart || Globals.KnownLocations.Contains(response);
        if (!Globals.KnownLocations.Contains(response)) Globals.KnownLocations.Add(response);
        obj = GameObject.FindWithTag("Respawn").gameObject;
        obj.SetActive(false);
        obj.transform.Find("Investigation").gameObject.SetActive(true);
        Destroy(gameObject);
    }

    private IEnumerator Kill(bool destroyInstantly = true)
    {
        _killing = true;
        _soundManager.Play("back");

        StartCoroutine(CloseWindow());
        
        foreach (GameObject but in GameObject.FindGameObjectsWithTag("Button"))
        {
            but.GetComponent<Animator>().Play("Fade Out");
        }
        
        Globals.ControlFlag.SetText(new string[] {});
        
        for (int i = 100; i > 0; i -= 20)
        {
            _sprite.color = new Color(0f, 0f, 0f, i/255f);
            yield return new WaitForSeconds(1 / 60f);
        }
        _sprite.color = new Color(0f, 0f, 0f, 0f);
        Destroy(_selectionHead);
        
        yield return new WaitUntil(() => !_windowOpen);
        GameObject.FindWithTag("Respawn").transform.Find("Investigation").gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
