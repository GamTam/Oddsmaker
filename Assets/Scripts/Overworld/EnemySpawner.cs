using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private BattleLoadScene _load;
    [SerializeField] private float _stepInterval;
    [SerializeField] private BattleChance[] _chance;
    [SerializeField] private bool _continueOverworldMusic = true;

    [SerializeField] [ReadOnly] [AllowNesting] private float _timer = 0;
    private float _totalOdds;
    private float _randomOffset;

    private void Start()
    {
        for (int i = 0; i < _chance.Length; i++)
        {
            _totalOdds += _chance[i].Chance;
        }

        _randomOffset = Random.Range(0.5f, 2f);
        Debug.Log(_randomOffset);
    }

    private void Update()
    {
        if (!_player.IsMoving) return;

        _timer += Time.deltaTime;

        if (_timer >= _stepInterval * _randomOffset)
        {
            _timer = 0;
            _randomOffset = Random.Range(0.5f, 2f);

            int random = Random.Range(0, _chance.Length);

            StartCoroutine(_load.BattleTransition(_chance[random].Scene, stopCurrentSong:_continueOverworldMusic));
        }
    }
}

[Serializable]
public struct BattleChance
{
    public string Scene;
    public float Chance;
}