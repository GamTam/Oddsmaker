using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerResults : MonoBehaviour
{
    [SerializeField] private Intro _intro;
    [SerializeField] private Poker _poker;

    public int PokerCalcs(int[] array)
    {
        int _heartCounter = 0;
        int _diamondCounter = 0;
        int _clubCounter = 0;
        int _spadeCounter = 0;

        int _ranking = 0;

        foreach(int x in array)
        {
            if(x == 0)
            {
                _heartCounter++;
                if(_heartCounter > 1 && _heartCounter < 3)
                {
                    Debug.Log("Heart Pair");
                    _ranking++;
                }
                else if(_heartCounter > 2 && _heartCounter < 4)
                {
                    Debug.Log("Heart Three of a kind");
                    _ranking += 2;
                }
                else if(_heartCounter > 3 && _heartCounter < 5)
                {
                    Debug.Log("Heart Four of a kind");
                    _ranking += 3;
                }
                else if(_heartCounter > 4)
                {
                    Debug.Log("Heart Five");
                    _ranking += 4;
                }
            }
            else if(x == 1)
            {
                _diamondCounter++;
                if(_diamondCounter > 1 && _diamondCounter < 3)
                {
                    Debug.Log("Diamond Pair");
                    _ranking++;
                }
                else if(_diamondCounter > 2 && _diamondCounter < 4)
                {
                    Debug.Log("Diamond Three of a kind");
                    _ranking += 2;
                }
                else if(_diamondCounter > 3 && _diamondCounter < 5)
                {
                    Debug.Log("Diamond Four of a kind");
                    _ranking += 3;
                }
                else if(_diamondCounter > 4)
                {
                    Debug.Log("Diamond Five");
                    _ranking += 4;
                }
            }
            else if(x == 2)
            {
                _clubCounter++;
                if(_clubCounter > 1 && _clubCounter < 3)
                {
                    Debug.Log("_clubCounter Pair");
                    _ranking++;
                }
                else if(_clubCounter > 2 && _clubCounter < 4)
                {
                    Debug.Log("_clubCounter Three of a kind");
                    _ranking += 2;
                }
                else if(_clubCounter > 3 && _clubCounter < 5)
                {
                    Debug.Log("_clubCounter Four of a kind");
                    _ranking += 3;
                }
                else if(_clubCounter > 4)
                {
                    Debug.Log("_clubCounter Five");
                    _ranking += 4;
                }
            }
            else if(x == 3)
            {
                _spadeCounter++;
                if(_spadeCounter > 1 && _spadeCounter < 3)
                {
                    Debug.Log("_spadeCounter Pair");
                    _ranking++;
                }
                else if(_spadeCounter > 2 && _spadeCounter < 4)
                {
                    Debug.Log("_spadeCounter Three of a kind");
                    _ranking += 2;
                }
                else if(_spadeCounter > 3 && _spadeCounter < 5)
                {
                    Debug.Log("_spadeCounter Four of a kind");
                    _ranking += 3;
                }
                else if(_spadeCounter > 4)
                {
                    Debug.Log("_spadeCounter Five");
                    _ranking += 4;
                }
            }
        }

        return _ranking;
    }
}
