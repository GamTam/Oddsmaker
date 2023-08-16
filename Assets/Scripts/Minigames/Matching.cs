using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Matching : MonoBehaviour
{
    //Everything that needs to be listed in the inspector
    [SerializeField] public GameObject[] _cards;
    [SerializeField] private Animator[] _cardsAnimators;
    [SerializeField] private MenuSystem _menuSystem;
    [SerializeField] public List<int> _listInt;
    [SerializeField] public DialogueManagerDavid _dialogueManagerScript;
    [SerializeField] public GameObject _dialogueManager;
    [SerializeField] public Color _color;
    [SerializeField] public CasinoManager _casino;

    // Helpful ints
    int _random = 0;
    int _firstNumber = 0;
    int _firstButton = 0;
    int _activeCardCount = 20;
    int[] _number = new int[20];

    // Bool to switch from if to else
    bool _buttonPressed;

    ColorBlock _buttonColors;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartDelay(3.7f));
    }

    public void ButtonGotPressed(int index)
    {
        if(!_buttonPressed)
        {
            _buttonColors = _menuSystem._buttons[index].colors;
            _buttonColors.normalColor = _color;
            _menuSystem._buttons[index].colors = _buttonColors;
            _buttonPressed = true;
            _firstButton = index;
            _firstNumber = _number[index];
        }
        else
        {
            _buttonColors = _menuSystem._buttons[_firstButton].colors;
            _buttonColors.normalColor = _menuSystem._unselectedColor;
            _menuSystem._buttons[_firstButton].colors = _buttonColors;
            if(_firstButton == index)
            {
                //Resets if player picks same card
                _buttonColors = _menuSystem._buttons[index].colors;
                _buttonColors.normalColor = _menuSystem._unselectedColor;
                _menuSystem._buttons[_firstButton].colors = _buttonColors;
                _buttonPressed = false;
            }
            else
            {
                _buttonPressed = false;
                if(_firstNumber == _number[index])
                {
                    _listInt.Remove(_firstNumber);
                    _listInt.Remove(_number[index]);
                    _activeCardCount -= 2;
                    _cardsAnimators[_firstButton].Play("Success");
                    _cardsAnimators[index].Play("Success");
                    EventSystem.current.SetSelectedGameObject(_dialogueManager);
                    StartCoroutine(DisappearCards(index));
                }
            }
        }

        // Check if there are only four cards left
        if (_activeCardCount == 4)
        {
            if (!AreLastCardsPairable())
            {
                //4 cards can't be paired - lose
                StartCoroutine(Result(1));
            }
        }
        else if (_activeCardCount == 2)
        {
            if (!AreLastCardsPairable())
            {
                //2 cards can't be paired - lose
                StartCoroutine(Result(1));
            }
        }
    }

    private void SetActiveCard()
    {
        // Find the first active gameobject in the _cards array
        int index = 0;
        while (index < _cards.Length && !_cards[index].activeSelf)
        {
            index++;
        }

        // Set the selected gameobject to the first active gameobject
        if (index < _cards.Length)
        {
            EventSystem.current.SetSelectedGameObject(_cards[index]);
        }
        else
        {
            StartCoroutine(Result(2));
        }
    }

    IEnumerator StartDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _dialogueManager.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_cards[0].gameObject);
        for(int i = 0; i < 20; i++)
        {
            _random = Random.Range(0, 4);
            switch(_random) 
            {
                case 0:
                    _cardsAnimators[i].Play("HeartFlip");
                    break;
                case 1:
                    _cardsAnimators[i].Play("DiamondFlip");
                    break;
                case 2:
                    _cardsAnimators[i].Play("ClubFlip");
                    break;
                case 3:
                    _cardsAnimators[i].Play("SpadeFlip");
                    break;
            }
            _number[i] = _random;
        }

        for(int i = 0; i < 20; i++)
        {
            _listInt[i] = _number[i];
        }
    }

    //Checks to see if the last cards are pairable
    private bool AreLastCardsPairable()
    {
        if (_activeCardCount == 2)
        {
            // There are only 2 cards left, so they can be paired if they have the same number
            if(_listInt[0] == _listInt[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (_activeCardCount == 4)
        {
            // There are 4 cards left, so they can be paired if any two of them have the same number
            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if(_listInt[i] == _listInt[j])
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator Result(int number)
    {
        yield return new WaitForSeconds(2.0f);
        _dialogueManagerScript.ForceAdvance();
        yield return new WaitForSeconds(0.01f);
        _dialogueManagerScript.counter = number;
        _dialogueManager.SetActive(true);
        yield return new WaitForSeconds(7.0f);
        StartCoroutine(_casino.FadeOut());
    }

    IEnumerator DisappearCards(int index)
    {
        yield return new WaitForSeconds(1.0f);
        _cards[_firstButton].SetActive(false);
        _cards[index].SetActive(false);
        SetActiveCard();
    }
}
