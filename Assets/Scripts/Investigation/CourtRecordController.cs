using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CourtRecordController : MonoBehaviour
{
    [SerializeField] public GameObject _base;
    [SerializeField] private GameObject _icon;
    [SerializeField] private GameObject _evidenceRow;
    [SerializeField] private GameObject _backupRow;

    [Header("Arrows")] 
    [SerializeField] private GameObject[] _arrows;
    [SerializeField] private RectTransform _targetPoint;

    [Header("Evidence Details")] 
    [SerializeField] private Image _checkSprite;
    [SerializeField] private Image _bigEvidenceIcon;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;

    [Header("Top Banner")] 
    [SerializeField] private Image _banner;
    [SerializeField] private List<Sprite> _bannerImages;

    [Header("Check Evidence For More Details")] 
    [SerializeField] private Image _moreInfo;
    [SerializeField] private Image _fade;
    [SerializeField] private GameObject _moreInfoAdvanceButton;

    [Header("Row Indicator")] 
    [SerializeField] private GameObject _iconRow;
    [SerializeField] private GameObject _iconRowIndicator;
    [SerializeField] private Sprite[] _icons;
    
    public event Action<EvidenceSO> HasPresented;
    [HideInInspector] public bool Closing;

    private List<Image> _oddEvidence = new List<Image>();
    private List<Image> _evenEvidence = new List<Image>();
    private GameObject _openPage;

    private bool _sound = true;
    private bool _evidence = true;
    private int _page = 1;
    private int _evidencePage = 1;
    private int _evidenceIndex = 0;
    private int _profilePage = 1;
    private int _profileIndex = 0;

    private SoundManager _soundManager;
    private GameObject _selectedButton;
    private DialogueManager _dialogueManager;
    
    private PlayerInput _playerInput;
    private InputAction _nextEvidence;
    private InputAction _prevEvidence;
    private InputAction _backButton;
    private ControlFlagController _controlFlag;
    
    private CRNormal _normal;
    private CRPrompt _prompt;
    private CRPresent _present;
    private CRCrossEx _crossEx;
    private CRPromptLock _psycheLock;

    [SerializeField] private Vector3 _speedVector = new Vector3(10f, 10f, 1);
    
    private Navigation _nav = new Navigation();
    
    IEnumerator Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _soundManager = GameObject.FindWithTag("Audio").GetComponent<SoundManager>();
        _dialogueManager = GameObject.FindWithTag("Dialogue Manager").GetComponent<DialogueManager>();
        _controlFlag = FindObjectOfType<ControlFlagController>();
        _base.transform.localScale = new Vector3(0, 0, 0);

        _nextEvidence = _playerInput.actions["CheckEvidence/Next"];
        _prevEvidence = _playerInput.actions["CheckEvidence/Previous"];
        _backButton = _playerInput.actions["CheckEvidence/Back"];
        
        _playerInput.SwitchCurrentActionMap("Null");
        _soundManager.Play("record flip");

        Transform hpBar = transform.parent.Find("HP Bar").transform;
        transform.SetSiblingIndex(hpBar.GetSiblingIndex());

        _normal = GetComponent<CRNormal>();
        _prompt = GetComponent<CRPrompt>();
        _present = GetComponent<CRPresent>();
        _crossEx = GetComponent<CRCrossEx>();
        _psycheLock = GetComponent<CRPromptLock>();
        
        _nav.wrapAround = true;
        _nav.mode = Navigation.Mode.Horizontal;

        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(_icon, _evidenceRow.transform, false);
            obj.gameObject.SetActive(true);
            
            if (Globals.Evidence.Count > i)
            {
                obj.GetComponentsInChildren<Image>()[1].sprite = Globals.Evidence[i].Icon;
                obj.GetComponentsInChildren<Image>()[1].SetNativeSize();
                obj.GetComponent<Button>().navigation = _nav;
                _oddEvidence.Add(obj.GetComponentsInChildren<Image>()[1]);
            }
            else
            {
                obj.GetComponent<Button>().interactable = false;
                obj.GetComponentsInChildren<Image>()[1].enabled = false;
            }

            if (i == 0)
            {
                EventSystem.current.SetSelectedGameObject(obj.gameObject);
                _bigEvidenceIcon.sprite = Globals.Evidence[0].Icon;
                _bigEvidenceIcon.SetNativeSize();

                _title.SetText(Globals.Evidence[0].Name);
                _description.SetText(Globals.Evidence[0].Description);

                _checkSprite.enabled = true;
                if (Globals.Evidence[0].CheckImages.Length == 0)
                {
                    _checkSprite.enabled = false;
                }
            }
        }
        
        KillChildren(_iconRow.transform);
        if (Globals.Evidence.Count > 10)
        {
            _arrows[0].SetActive(true);
            _arrows[1].SetActive(true);
            
            _iconRow.SetActive(true);
            for (int i = Globals.Evidence.Count; i > 0; i -= 10)
            {
                GameObject obj = Instantiate(_iconRowIndicator, _iconRow.transform, false);
                obj.SetActive(true);
            }

            _iconRow.GetComponentsInChildren<Image>()[0].sprite = _icons[1];
        }
        else
        {
            _iconRow.SetActive(false);
        }

        _selectedButton = EventSystem.current.currentSelectedGameObject;
        
        while (_base.transform.localScale.x < 1)
        {
            yield return null;
            _base.transform.localScale += _speedVector * Time.deltaTime;
        }
        
        _playerInput.SwitchCurrentActionMap("Menu");
        _base.transform.localScale = new Vector3(1, 1, 1);
        _openPage = _evidenceRow;
    }

    void Update()
    {
        if (_base.transform.localScale.x < 1) return;
        if (EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(_selectedButton);
        
        if (_selectedButton != EventSystem.current.currentSelectedGameObject)
        {
            if (_sound) _soundManager.Play("select");
            _selectedButton = EventSystem.current.currentSelectedGameObject;

            if (_selectedButton == _arrows[0])
            {
                StartCoroutine(PageTurn(true));
                return;
            }

            if (_selectedButton == _arrows[1])
            {
                StartCoroutine(PageTurn(false));
                return;
            }

            AssignDescription();
        }
    }

    public void StartMoreInfo()
    {
        StartCoroutine(MoreInfo());
    }

    private IEnumerator MoreInfo()
    {
        List<Image> images;
        List<EvidenceSO> masterList;
        List<string> position = _controlFlag._currentButtons;
        int flagSiblingIndex = _controlFlag.transform.GetSiblingIndex();
        int selfSiblingIndex = transform.GetSiblingIndex();
        bool flagHidden = _controlFlag.IsHidden;
            
        if (_openPage == _backupRow)
        {
            images = _evenEvidence;
        }
        else
        {
            images = _oddEvidence;
        }

        masterList = _evidence ? Globals.Evidence : Globals.Profiles;
            
        int index = images.IndexOf(
            EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]) + (_page - 1) * 10;

        if (masterList[index].CheckImages.Length == 0) yield break;
        
        _soundManager.Play("confirm");
        _playerInput.SwitchCurrentActionMap("Null");
        
        while (_base.transform.localScale.x > 0)
        {
            yield return null;
            _base.transform.localScale -= _speedVector * Time.deltaTime;
        }

        _base.transform.localScale = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(0.1f);
        
        transform.SetSiblingIndex(transform.parent.childCount - 1);

        Sprite[] checkImages = masterList[index].CheckImages;
        int checkIndex = 0;
        
        _moreInfo.sprite = checkImages[0];
        _moreInfo.gameObject.SetActive(true);
        _fade.gameObject.SetActive(true);
        if (checkImages.Length > 1) _moreInfoAdvanceButton.SetActive(true);
        _fade.color = Color.black;

        float duration = 0.5f;
        float time = 0;
        
        while (time < duration)
        {
            _fade.color = Color.Lerp(Color.black, Color.clear, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _fade.color = Color.clear;
        
        _controlFlag.transform.SetSiblingIndex(_controlFlag.transform.parent.childCount - 1);
        _controlFlag.Show();
        _controlFlag.SetText(new string[] {"Back"}, true);
        while (_controlFlag.gameObject.transform.localScale.y < 1) yield return null;

        _playerInput.SwitchCurrentActionMap("CheckEvidence");

        while (true)
        {
            if (checkImages.Length > 1)
            {
                if (_nextEvidence.triggered)
                {
                    checkIndex += 1;
                    if (checkIndex >= checkImages.Length) checkIndex = 0;
                    _soundManager.Play("confirm");
                }
                
                if (_prevEvidence.triggered)
                {
                    checkIndex -= 1;
                    if (checkIndex < 0) checkIndex = checkImages.Length - 1;
                    _soundManager.Play("confirm");
                }

                _moreInfo.sprite = checkImages[checkIndex];
            }
            if (_backButton.triggered) break;
            yield return null;
        }
        
        _playerInput.SwitchCurrentActionMap("Null");

        _soundManager.Play("back");
        
        _controlFlag.SetText(position.ToArray());
        _moreInfoAdvanceButton.SetActive(false);

        while (_controlFlag.gameObject.transform.localScale.y > 0) yield return null;
        if (flagHidden)
        {
            _controlFlag.gameObject.transform.localScale = Vector3.one;
            _controlFlag.Hide();
        }
        _controlFlag.transform.SetSiblingIndex(flagSiblingIndex);

        time = 0;
        
        while (time < duration)
        {
            _fade.color = Color.Lerp(Color.clear, Color.black, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _fade.color = Color.black;

        yield return new WaitForSeconds(0.1f);
        
        _moreInfo.gameObject.SetActive(false);
        _fade.gameObject.SetActive(false);
        
        transform.SetSiblingIndex(selfSiblingIndex);
        
        _soundManager.Play("record flip");
        while (_base.transform.localScale.x < 1)
        {
            yield return null;
            _base.transform.localScale += _speedVector * Time.deltaTime;
        }
        
        _playerInput.SwitchCurrentActionMap("Menu");
        _base.transform.localScale = new Vector3(1, 1, 1);
    }

    public void Close(bool sound=true, bool kill=true)
    {
        if (sound) _soundManager.Play("back");
        Closing = true;
        StartCoroutine(CloseRecord(kill));
    }

    public void ProfileEvidenceSwap()
    {
        _soundManager.Play("record flip");
        StartCoroutine(SwapProfileAndEvidence());
    }

    public void Present()
    {
        List<Image> images;
            
        if (_openPage == _backupRow)
        {
            images = _evenEvidence;
        }
        else
        {
            images = _oddEvidence;
        }
        
        int index = images.IndexOf(
            EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]) + (_page - 1) * 10;
        
        if (_evidence)
        {
            HasPresented?.Invoke(Globals.Evidence[index]);
        }
        else
        {
            HasPresented?.Invoke(Globals.Profiles[index]);
        }
    }

    public IEnumerator WaitThenLoop(EvidenceSO evidence, InvestigationMenu.EvidenceTalkPair[] pairs, DialogueSO wrongEvidence, bool comeBack=true)
    {
        _playerInput.SwitchCurrentActionMap("Null");
        DialogueSO dialogue = wrongEvidence;
        
        foreach (InvestigationMenu.EvidenceTalkPair pair in pairs)
        {
            if (pair.Evidence == evidence && Globals.CheckStoryFlags(pair.Conditions))
            {
                dialogue = pair.Dialogue;
                break;
            }
        }
        
        if (dialogue.dialogueText[0].Interjection == Interjection.NA) Globals.SoundManager.Play("confirm");

        yield return new WaitForSeconds(0.1f);
        
        if (dialogue.dialogueText[0].Interjection != Interjection.NA) _base.transform.localScale = Vector3.zero;

        while (_base.transform.localScale.x > 0)
        {
            yield return null;
            _base.transform.localScale -= _speedVector * Time.deltaTime;
        }

        _base.transform.localScale = new Vector3(0, 0, 0);

        GetComponent<Image>().enabled = false;
        if (dialogue.dialogueText[0].Interjection == Interjection.NA) yield return new WaitForSeconds(0.1f);

        _dialogueManager.StartText(dialogue);

        if (!comeBack)
        {
            StartCoroutine(CloseRecord());
            yield break;
        }
        
        int index;
        if (_openPage == _evidenceRow)
        {
            index = _oddEvidence.IndexOf(
                EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]);
            _openPage = _backupRow;
        }
        else 
        { 
            _openPage = _evidenceRow; 
            index = _evenEvidence.IndexOf(
                EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]);
        }
        
        _oddEvidence = new List<Image>();
        _evenEvidence = new List<Image>();
        
        KillChildren(_evidenceRow.transform);
        KillChildren(_backupRow.transform);
        
        yield return new WaitUntil(() => _dialogueManager._doneTalking);

        AddItemsToRow();
        
        if (_openPage == _evidenceRow)
        {
            EventSystem.current.SetSelectedGameObject(_evenEvidence[Mathf.Min(index, _evenEvidence.Count - 1)].transform.parent.gameObject);
            _openPage = _backupRow;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_oddEvidence[Mathf.Min(index, _oddEvidence.Count - 1)].transform.parent.gameObject);
            _openPage = _evidenceRow;
        }
        
        _selectedButton = EventSystem.current.currentSelectedGameObject;
        
        AssignDescription();
        yield return new WaitForSeconds(0.1f);
        GetComponent<Image>().enabled = false;

        Globals.SoundManager.Play("record flip");
        while (_base.transform.localScale.x < 1)
        {
            yield return null;
            _base.transform.localScale += _speedVector * Time.deltaTime;
        }

        _base.transform.localScale = new Vector3(1, 1, 1);
        GetComponent<Image>().enabled = true;
        
        yield return new WaitForSeconds(0.1f);
        _playerInput.SwitchCurrentActionMap("Menu");
    }

    private IEnumerator CloseRecord(bool kill=true)
    {
        while (_base.transform.localScale.x > 0)
        {
            yield return null;
            _base.transform.localScale -= _speedVector * Time.deltaTime;
        }

        _base.transform.localScale = new Vector3(0, 0, 0);

        GetComponent<Image>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        try
        {
            if (_present.enabled) GameObject.FindWithTag("Respawn").transform.Find("Investigation").gameObject.SetActive(true);
        } catch {}
        if (kill) Destroy(gameObject);
        enabled = false;
    }

    public IEnumerator SwapProfileAndEvidence()
    {
        _sound = false;
        int index = 0;

        if (_openPage == _evidenceRow)
        {
            index = _oddEvidence.IndexOf(
                EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]);
            _openPage = _backupRow;
        }
        else 
        { 
            _openPage = _evidenceRow; 
            index = _evenEvidence.IndexOf(
            EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]);
        }

        if (_evidence)
        {
            _evidencePage = _page;
            _evidenceIndex = index;
        }
        else
        {
            _profilePage = _page;
            _profileIndex = index;
        }
        
        _evidence = !_evidence;
        _playerInput.SwitchCurrentActionMap("Null");
        while (_base.transform.localScale.y > 0)
        {
            yield return null;
            _base.transform.localScale -= new Vector3(0, _speedVector.y) * Time.deltaTime;
        }

        if (_normal.enabled) _normal.SetControlLabel(_evidence);
        else if (_prompt.enabled) _prompt.SetControlLabel(_evidence);
        else if (_present.enabled) _present.SetControlLabel(_evidence);
        else if (_crossEx.enabled) _crossEx.SetControlLabel(_evidence);
        else if (_psycheLock.enabled) _psycheLock.SetControlLabel(_evidence);

        _base.transform.localScale = new Vector3(_base.transform.localScale.x, 0, _base.transform.localScale.y);
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        
        KillChildren(_evidenceRow.transform);
        KillChildren(_backupRow.transform);

        _arrows[0].SetActive(false);
        _arrows[1].SetActive(false);

        _banner.sprite = _evidence ? _bannerImages[0] : _bannerImages[1];
        _page = _evidence ? _evidencePage : _profilePage;
        index = _evidence ? _evidenceIndex : _profileIndex;
        
        _oddEvidence = new List<Image>();
        _evenEvidence = new List<Image>();

        AddItemsToRow();

        if (_openPage == _evidenceRow)
        {
            EventSystem.current.SetSelectedGameObject(_evenEvidence[index].transform.parent.gameObject);
            _openPage = _backupRow;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_oddEvidence[index].transform.parent.gameObject);
            _openPage = _evidenceRow;
        }
        
        KillChildren(_iconRow.transform);
        if (_evidence)
        {
            if (Globals.Evidence.Count > 10)
            {
                _arrows[0].SetActive(true);
                _arrows[1].SetActive(true);
                
                _iconRow.SetActive(true);
                for (int i = Globals.Evidence.Count; i > 0; i -= 10)
                {
                    GameObject obj = Instantiate(_iconRowIndicator, _iconRow.transform, false);
                    obj.SetActive(true);
                }

                _iconRow.GetComponentsInChildren<Image>()[_page - 1].sprite = _icons[1];
            }
            else
            {
                _iconRow.SetActive(false);
            }
        }
        else
        {
            if (Globals.Profiles.Count > 10)
            {
                _arrows[0].SetActive(true);
                _arrows[1].SetActive(true);
                
                _iconRow.SetActive(true);
                for (int i = Globals.Profiles.Count; i > 0; i -= 10)
                {
                    GameObject obj = Instantiate(_iconRowIndicator, _iconRow.transform, false);
                    obj.SetActive(true);
                }

                _iconRow.GetComponentsInChildren<Image>()[_page - 1].sprite = _icons[1];
            }
            else
            {
                _iconRow.SetActive(false);
            }
        }

        while (_base.transform.localScale.y < 1)
        {
            yield return null;
            _base.transform.localScale += new Vector3(0, _speedVector.y) * Time.deltaTime;
        }

        _base.transform.localScale = new Vector3(_base.transform.localScale.x, 1, _base.transform.localScale.y);
        _sound = true;
        _playerInput.SwitchCurrentActionMap("Menu");
    }

    public IEnumerator PageTurn(bool left)
    {
        RectTransform currentRect;
        RectTransform hiddenRect;
        int count = _evidence ? Globals.Evidence.Count : Globals.Profiles.Count;

        if (_openPage == _backupRow)
        {
            currentRect = _backupRow.GetComponent<RectTransform>();
            hiddenRect = _evidenceRow.GetComponent<RectTransform>();
        }
        else
        {
            currentRect = _evidenceRow.GetComponent<RectTransform>();
            hiddenRect = _backupRow.GetComponent<RectTransform>();
        }
        
        _playerInput.SwitchCurrentActionMap("Null");
        if (left)
        {
            _page -= 1;
            if (_page <= 0)
            {
                _page = (int) Math.Ceiling(count / 10f);
            }
        }
        else
        {
            _page += 1;
            if (_page > (int) Math.Ceiling(count / 10f))
            {
                _page = 1;
            }
        }

        AddItemsToRow();
        
        _arrows[0].SetActive(false);
        _arrows[1].SetActive(false);
        List<Image> images;
        
        if (left)
        {
            hiddenRect.localPosition = new Vector2(-_targetPoint.localPosition.x, hiddenRect.localPosition.y);

            while (currentRect.transform.position.x < _targetPoint.position.x)
            {
                currentRect.localPosition = Vector3.MoveTowards(currentRect.localPosition, _targetPoint.localPosition, 20 * Time.deltaTime * 200);
                hiddenRect.localPosition = Vector3.MoveTowards(hiddenRect.localPosition, new Vector2(0, hiddenRect.localPosition.y), 20 * Time.deltaTime * 200);
                yield return null;
            }
            
            if (_openPage == _evidenceRow)
            {
                KillChildren(_evidenceRow.transform);
                _oddEvidence = new List<Image>();
                images = _evenEvidence;
                EventSystem.current.SetSelectedGameObject(_evenEvidence[^1].transform.parent.gameObject);
                _openPage = _backupRow;
            }
            else
            {
                KillChildren(_backupRow.transform);
                _evenEvidence = new List<Image>();
                images = _oddEvidence;
                EventSystem.current.SetSelectedGameObject(_oddEvidence[^1].transform.parent.gameObject);
                _openPage = _evidenceRow;
            }
        }
        else
        {
            hiddenRect.localPosition = new Vector2(_targetPoint.localPosition.x, hiddenRect.localPosition.y);

            while (currentRect.transform.position.x > -_targetPoint.position.x)
            {
                currentRect.localPosition = Vector3.MoveTowards(currentRect.localPosition, new Vector2(-_targetPoint.localPosition.x, _targetPoint.localPosition.y), 20 * Time.deltaTime * 200);
                hiddenRect.localPosition = Vector3.MoveTowards(hiddenRect.localPosition, new Vector2(0, hiddenRect.localPosition.y), 20 * Time.deltaTime * 200);
                yield return null;
            }
            
            if (_openPage == _evidenceRow)
            {
                KillChildren(_evidenceRow.transform);
                _oddEvidence = new List<Image>();
                images = _evenEvidence;
                EventSystem.current.SetSelectedGameObject(_evenEvidence[0].transform.parent.gameObject);
                _openPage = _backupRow;
            }
            else
            {
                KillChildren(_backupRow.transform);
                _evenEvidence = new List<Image>();
                images = _oddEvidence;
                EventSystem.current.SetSelectedGameObject(_oddEvidence[0].transform.parent.gameObject);
                _openPage = _evidenceRow;
            }
        }
        
        AssignDescription();
        if ((Globals.Profiles.Count > 10 && !_evidence) || (Globals.Evidence.Count > 10 && _evidence))
        {
            foreach (Image img in _iconRow.GetComponentsInChildren<Image>())
            {
                img.sprite = _icons[0];
            }
            _iconRow.GetComponentsInChildren<Image>()[_page - 1].sprite = _icons[1];
        }
        
        _selectedButton = EventSystem.current.currentSelectedGameObject;
        _playerInput.SwitchCurrentActionMap("Menu");
        
        _arrows[0].SetActive(true);
        _arrows[1].SetActive(true);
    }

    private void AssignDescription()
    {
        List<Image> images;
        List<EvidenceSO> masterList;
            
        if (_openPage == _backupRow)
        {
            images = _evenEvidence;
        }
        else
        {
            images = _oddEvidence;
        }

        masterList = _evidence ? Globals.Evidence : Globals.Profiles;
            
        int index = images.IndexOf(
            EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Image>()[1]) + (_page - 1) * 10;

        _bigEvidenceIcon.sprite = masterList[index].Icon;
        _bigEvidenceIcon.SetNativeSize();

        _title.SetText(masterList[index].Name);
        _description.SetText(masterList[index].Description);

        _checkSprite.enabled = true;
        if (masterList[index].CheckImages.Length == 0)
        {
            _checkSprite.enabled = false;
        }
    }

    private void AddItemsToRow()
    {
        List<EvidenceSO> items = new List<EvidenceSO>();

        if (_evidence)
        {
            items = Globals.Evidence;
        }
        else
        {
            items = Globals.Profiles;
        }
        
        for (int i = 0; i < 10; i++)
        {
            GameObject obj;
            if (_openPage == _evidenceRow)
            {
                obj = Instantiate(_icon, _backupRow.transform, false);
            }
            else
            {
                obj = Instantiate(_icon, _evidenceRow.transform, false);
            } 
            
            obj.gameObject.SetActive(true);
            
            if (items.Count > i + (_page - 1) * 10)
            {
                obj.GetComponentsInChildren<Image>()[1].sprite = items[i + (_page - 1) * 10].Icon;
                obj.GetComponentsInChildren<Image>()[1].SetNativeSize();
                if (_openPage == _evidenceRow)
                {
                    _evenEvidence.Add(obj.GetComponentsInChildren<Image>()[1]);
                }
                else
                {
                    _oddEvidence.Add(obj.GetComponentsInChildren<Image>()[1]);
                }
                obj.GetComponent<Button>().navigation = _nav;
            }
            else
            {
                obj.GetComponent<Button>().interactable = false;
                obj.GetComponentsInChildren<Image>()[1].enabled = false;
            }
        }
    }
    
    public void KillChildren(Transform transform)
    {
        int i = 0;

        //Array to hold all child obj
        GameObject[] allChildren = new GameObject[transform.childCount];

        //Find all child obj and store to that array
        foreach (Transform child in transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        //Now destroy them
        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child.gameObject);
        }
    }
}
