using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TalkManager : MonoBehaviour
{
    [SerializeField] private RectTransform responseButton;
    [SerializeField] private Transform responseButtonLocation;
    [SerializeField] private GameObject _selectionHead;
    [SerializeField] private Sprite[] _selectedSprites;

    private List<RectTransform> _responseButtons = new List<RectTransform>();
    private List<TalkSO> _dialogue = new List<TalkSO>();
    public InvestigationMenu _investigation;

    private PlayerInput _playerInput;
    private InputAction _back;
    private SoundManager _soundManager;
    private bool _revealing;
    
    private DialogueManager _dialogueManager;
    private bool _shownResponses;
    private Image _sprite;
    private bool _killing;

    private void Awake()
    {
        _dialogueManager = GameObject.FindWithTag("Dialogue Manager").GetComponent<DialogueManager>();
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        _investigation = GameObject.FindWithTag("UI").GetComponent<InvestigationMenu>();
        
        _back = _playerInput.actions["Menu/Cancel"];
    }

    private IEnumerator Start()
    {
        if (!Globals.ControlFlag._currentButtons.Equals(new string[] {"Back"}))
        {
            Globals.ControlFlag.Show();
            Globals.ControlFlag.SetText(new string[] {"Options", "Back", "Court Record"}, skipOut:true);
        }
        if (_sprite == null) _sprite = GetComponent<Image>();
        
        _sprite.color = new Color(0f, 0f, 0f, 0f);

        for (int i = 0; i < 100; i += 20)
        {
            _sprite.color = new Color(0f, 0f, 0f, i/255f);
            yield return new WaitForSeconds(1 / 60f);
        }
        
        _sprite.color = new Color(0f, 0f, 0f, 100/255f);
    }

    private void Update()
    {
        if (_killing || !_dialogueManager._doneTalking) return;
        
        if (_back.triggered)
        {
            StartCoroutine(Kill());
        }

        if (_responseButtons.Count == 0 && !_revealing)
        {
            _revealing = true;
            StartCoroutine(RevealOptions());
        }
    }

    private IEnumerator RevealOptions()
    {
        yield return new WaitForSeconds(0.25f);
        
        StartCoroutine(Start());
        _playerInput.SwitchCurrentActionMap("Menu");
        _dialogue = _investigation._talkText.ToList();
        ShowOptions(_dialogue);
    }

    public void ShowOptions(List<TalkSO> responses)
    {
        if (_dialogue.Count == 0)
        {
            _dialogue = responses.ToList();
        }
        
        _selectionHead.SetActive(true);
        int i = 0;
        foreach (TalkSO response in responses)
        {
            if (!Globals.CheckStoryFlags(response.ConditionFlags)) continue;

            RectTransform responseButton = Instantiate(this.responseButton, responseButtonLocation, false);
            responseButton.gameObject.SetActive(true);
            responseButton.localScale = new Vector3(1, 1, 1);
            responseButton.gameObject.GetComponentInChildren<TMP_Text>().text = response.Name;
            responseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnPickedResponse(response)));

            _responseButtons.Add(responseButton);

            if (Globals.UsedTalks.Contains(response) && response.Locked)
            {
                responseButton.gameObject.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (Globals.UsedTalks.Contains(response))
            {
                responseButton.gameObject.GetComponent<Image>().sprite = _selectedSprites[0];
                responseButton.gameObject.GetComponentsInChildren<Image>()[1].sprite = _selectedSprites[1];
            }
            
            if (i==0) EventSystem.current.SetSelectedGameObject(responseButton.gameObject);
            i++;
        }
    }

    private IEnumerator OnPickedResponse(TalkSO response)
    {
        _playerInput.SwitchCurrentActionMap("TextBox");
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
        Globals.ControlFlag.SetText(new string[] {});
        
        yield return new WaitForSeconds(0.5f);
        Globals.UsedTalks.Add(response);
        _playerInput.SwitchCurrentActionMap("Menu");
        _dialogueManager.StartText(response.DialogueSO);
        
        foreach (RectTransform button in _responseButtons)
        {
            Destroy(button.gameObject);
        }

        _responseButtons = new List<RectTransform>();
        _revealing = false;
        _selectionHead.SetActive(false);
    }

    private IEnumerator Kill()
    {
        _killing = true;
        _soundManager.Play("back");
        
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
        
        yield return new WaitForSeconds(10/60f);
        GameObject.FindWithTag("Respawn").transform.Find("Investigation").gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
