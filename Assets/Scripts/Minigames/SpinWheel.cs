using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinWheel : MonoBehaviour
{
    [SerializeField] private Image[] _options;
    [SerializeField] private float _duration;
    [SerializeField] private float _durationBetweenOptions;
    [SerializeField] public GameObject _dialogueManager;
    [SerializeField] public DialogueManagerDavid _dialogueManagerScript;
    [SerializeField] public GameObject[] _backgrounds;
    [SerializeField] public GameObject _guy;

    [SerializeField] public CasinoManager _casino;

    bool _spinning;

    int _counter;

    float _timeElapsed;

    int _finalValue;

    bool _functionality;

    public static bool _desert;
    private InputAction _confirm;

    void Start()
    {
        _confirm = Globals.Input.actions["Menu/confirm"];
        
        if(_desert)
        {
            _guy.SetActive(false);
            _backgrounds[1].SetActive(true);
            _backgrounds[0].SetActive(false);
        }
        else
        {
            _guy.SetActive(true);
            _backgrounds[1].SetActive(false);
            _backgrounds[0].SetActive(true);
        }
        StartCoroutine(StartDelay(3.7f));
        _timeElapsed = 0f;
        _counter = 0;
        for(int i = 0; i < _options.Length; i++)
        {
            _options[i].color = Color.gray;
        }
    }

    void Update()
    {
        if(_confirm.triggered && !_spinning && _functionality)
        {
            _spinning = true;
            _functionality = false;
            _finalValue = Random.Range(0, _options.Length);
            StartCoroutine(Spin());
        }
    }

    IEnumerator Spin()
    {
        while(true)
        {
            _counter++;
            if(_counter > 7)
            {
                _counter = 0;
                _options[7].color = Color.gray;
            }
            else
            {
                _options[_counter - 1].color = Color.gray;
            }
            _options[_counter].color = Color.white;
            yield return new WaitForSeconds(_durationBetweenOptions);
            _timeElapsed += _durationBetweenOptions;
            if(_timeElapsed > _duration && _counter == _finalValue)
            {
                _timeElapsed = 0f;
                Globals.SoundManager.Play("wheelSelect");
                Result();
                break;
            }
            else
            {
                Globals.SoundManager.Play("wheelSelect");
            }
        }
    }

    IEnumerator StartDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _dialogueManager.SetActive(true);
        _options[0].color = Color.white;
        _functionality = true;
    }

    void Result()
    {
        switch(_finalValue)
        {
            case 0:
                StartCoroutine(ChangeDialogue(2));
                break;
            case 1:
                StartCoroutine(ChangeDialogue(1));
                break;
            case 2:
                StartCoroutine(ChangeDialogue(2));
                break;
            case 3:
                StartCoroutine(ChangeDialogue(3));
                break;
            case 4:
                StartCoroutine(ChangeDialogue(4));
                break;
            case 5:
                StartCoroutine(ChangeDialogue(1));
                break;
            case 6:
                StartCoroutine(ChangeDialogue(2));
                break;
            case 7:
                StartCoroutine(ChangeDialogue(1));
                break;
        }
    }

    IEnumerator ChangeDialogue(int number)
    {
        yield return new WaitForSeconds(2.0f);
        _dialogueManagerScript.ForceAdvance();
        yield return new WaitForSeconds(0.01f);
        _dialogueManagerScript.counter = number;
        _dialogueManager.SetActive(true);
        StartCoroutine(End());
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(3.0f);
        StartCoroutine(_casino.FadeOut());
    }
}
