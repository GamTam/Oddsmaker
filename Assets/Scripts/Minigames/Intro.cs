using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Intro : MonoBehaviour
{
    [SerializeField] public GameObject[] _cards;
    [SerializeField] public Animator[] _cardsAnimators;
    [SerializeField] public DialogueManagerDavid _dialogueManagerScript;
    [SerializeField] public GameObject _dialogueManager;

    // Helpful ints
    public int _random = 0;

    public int[] _numbers;
    public bool[] _numbersSelected;
    public bool _functionality = false;

    // Start is called before the first frame update
    void Start()
    {
        _numbers = new int[_cards.Length];
        _numbersSelected = new bool[_cards.Length];
        StartCoroutine(StartDelay(3.7f));
    }

    IEnumerator StartDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _functionality = true;
        _dialogueManager.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_cards[0].gameObject);
        for(int i = 0; i < _cards.Length; i++)
        {
            //Had to use UnityEngine.Random.Range because I have using System; and using UnityEngine;
            _random = UnityEngine.Random.Range(0, 4);
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
            _numbers[i] = _random;
        }
    }

    public void SelectCard(int number)
    {
        switch(_numbers[number])
        {
            case 0:
                if(_numbersSelected[number])
                {
                    _cardsAnimators[number].Play("HeartUnselect");
                    _numbersSelected[number] = false;
                }
                else
                {
                    _cardsAnimators[number].Play("HeartSelect");
                    _numbersSelected[number] = true;
                    StartCoroutine(AnimationDelay(number, _numbers[number]));
                }
                break;
            case 1:
                if(_numbersSelected[number])
                {
                    _cardsAnimators[number].Play("DiamondUnselect");
                    _numbersSelected[number] = false;
                }
                else
                {
                    _cardsAnimators[number].Play("DiamondSelect");
                    _numbersSelected[number] = true;
                    StartCoroutine(AnimationDelay(number, _numbers[number]));
                }
                break;
            case 2:
                if(_numbersSelected[number])
                {
                    _cardsAnimators[number].Play("ClubUnselect");
                    _numbersSelected[number] = false;
                }
                else
                {
                    _cardsAnimators[number].Play("ClubSelect");
                    _numbersSelected[number] = true;
                    StartCoroutine(AnimationDelay(number, _numbers[number]));
                }
                break;
            case 3:
                if(_numbersSelected[number])
                {
                    _cardsAnimators[number].Play("SpadeUnselect");
                    _numbersSelected[number] = false;
                }
                else
                {
                    _cardsAnimators[number].Play("SpadeSelect");
                    _numbersSelected[number] = true;
                    StartCoroutine(AnimationDelay(number, _numbers[number]));
                }
                break;
        }
    }

    IEnumerator AnimationDelay(int cardOrder, int cardType)
    {
        yield return new WaitForSeconds(0.1f);
        if(cardType == 0)
        {
            _cardsAnimators[cardOrder].Play("HoverHeart");
        }
        else if(cardType == 1)
        {
            _cardsAnimators[cardOrder].Play("HoverDiamond");
        }
        else if(cardType == 2)
        {
            _cardsAnimators[cardOrder].Play("HoverClub");
        }
        else if(cardType == 3)
        {
            _cardsAnimators[cardOrder].Play("HoverSpade");
        }
    }

    public void sortCards(int[] array)
    {
        Array.Sort(array);
    }
}
