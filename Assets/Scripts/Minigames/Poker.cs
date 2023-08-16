using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Poker : MonoBehaviour
{
    [SerializeField] private Intro _intro;
    [SerializeField] private PokerResults _pokerResults;
    [SerializeField] private Animator[] _dealersCardsAnimators;
    [SerializeField] public DialogueManagerDavid _dialogueManagerScript;
    [SerializeField] public GameObject _dialogueManager;
    [SerializeField] public CasinoManager _casino;

    bool _firstTime;

    int[] _dealersNumbers;

    int _playersResult;
    int _dealersResult;
    
    private InputAction _confirm;

    // Start is called before the first frame update
    void Start()
    {
        _confirm = Globals.Input.actions["Menu/Present"];
        _dealersNumbers = new int[_intro._cards.Length];
        _firstTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Change Input later
        if(_confirm.triggered && _intro._functionality)
        {  
            _intro._functionality = false;
            EventSystem.current.SetSelectedGameObject(null);
            for(int i = 0; i < _intro._numbersSelected.Length; i++)
            {
                if(_intro._numbersSelected[i])
                {
                    switch(_intro._numbers[i])
                    {
                        case 0:
                            _intro._cardsAnimators[i].Play("HeartWipe");
                            _intro._numbers[i] = UnityEngine.Random.Range(0, 4);
                            break;
                        case 1:
                            _intro._cardsAnimators[i].Play("DiamondWipe");
                            _intro._numbers[i] = UnityEngine.Random.Range(0, 4);
                            break;
                        case 2:
                            _intro._cardsAnimators[i].Play("ClubWipe");
                            _intro._numbers[i] = UnityEngine.Random.Range(0, 4);
                            break;
                        case 3:
                            _intro._cardsAnimators[i].Play("SpadeWipe");
                            _intro._numbers[i] = UnityEngine.Random.Range(0, 4);
                            break;
                    }
                }
                else
                {
                    //idk why that for loop was there. Got rid of it, but will keep incase everything breaks.

                    //for(int j = 0; j < _intro._numbersSelected.Length; j++)
                    //{
                    //    if(_intro._numbersSelected[j])
                    //    {
                            StartCoroutine(AnotherDelay(i, _intro._numbers[i]));
                    //    }
                    //}
                }
                StartCoroutine(ShowNewCards(3.25f));
            }
        }
    }

    IEnumerator AnotherDelay(int cardOrder, int cardType)
    {
        yield return new WaitForSeconds(2.25f);
        if(cardType == 0)
        {
            _intro._cardsAnimators[cardOrder].Play("HeartDown");
        }
        else if(cardType == 1)
        {
            _intro._cardsAnimators[cardOrder].Play("DiamondDown");
        }
        else if(cardType == 2)
        {
            _intro._cardsAnimators[cardOrder].Play("ClubDown");
        }
        else if(cardType == 3)
        {
            _intro._cardsAnimators[cardOrder].Play("SpadeDown");
        }
        //StartCoroutine(ShowNewCards(1.0f));
    }

    IEnumerator ShowNewCards(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(_firstTime)
        {
            _intro.sortCards(_intro._numbers);
            for(int i = 0; i < _intro._cards.Length; i++)
            {
                _intro._cardsAnimators[i].Play("PokerCardShow");
            }
            StartCoroutine(ShowNewCards(1.0f));
            yield return new WaitForSeconds(0.1f);
            _firstTime = false;
        }
        else
        {
            for(int i = 0; i < _intro._cards.Length; i++)
            {
                switch(_intro._numbers[i])
                {
                    case 0:
                        _intro._cardsAnimators[i].Play("HeartFlip");
                        break;
                    case 1:
                        _intro._cardsAnimators[i].Play("DiamondFlip");
                        break;
                    case 2:
                        _intro._cardsAnimators[i].Play("ClubFlip");
                        break;
                    case 3:
                        _intro._cardsAnimators[i].Play("SpadeFlip");
                        break;
                }
            }
            StartCoroutine(DealersCards());
        }
    }

    IEnumerator DealersCards()
    {
        yield return new WaitForSeconds(1.0f);
        for(int i = 0; i < _intro._cards.Length; i++)
        {
            //Had to use UnityEngine.Random.Range because I have using System; and using UnityEngine;
            _intro._random = UnityEngine.Random.Range(0, 4);
            _dealersNumbers[i] = _intro._random;
            _intro.sortCards(_dealersNumbers);
        }

        for(int j = 0; j < _intro._cards.Length; j++)
        {
            switch(_dealersNumbers[j]) 
            {
                case 0:
                    _dealersCardsAnimators[j].Play("HeartFlip");
                    break;
                case 1:
                    _dealersCardsAnimators[j].Play("DiamondFlip");
                    break;
                case 2:
                    _dealersCardsAnimators[j].Play("ClubFlip");
                    break;
                case 3:
                    _dealersCardsAnimators[j].Play("SpadeFlip");
                    break;
            }
        }

        yield return new WaitForSeconds(0.1f);
        _playersResult = _pokerResults.PokerCalcs(_intro._numbers);
        _dealersResult = _pokerResults.PokerCalcs(_dealersNumbers);

        if(_playersResult == _dealersResult)
        {
            StartCoroutine(Result(3));
        }
        else if(_dealersResult > _playersResult)
        {
            StartCoroutine(Result(2));
        }
        else if(_dealersResult < _playersResult)
        {
            StartCoroutine(Result(1));
        }
    }

    IEnumerator Result(int number)
    {
        yield return new WaitForSeconds(2.0f);
        _dialogueManagerScript.ForceAdvance();
        yield return new WaitForSeconds(0.01f);
        _dialogueManagerScript.counter = number;
        _dialogueManager.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        StartCoroutine(_casino.FadeOut());
    }
}
