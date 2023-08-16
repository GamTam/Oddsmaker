using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneInstance : MonoBehaviour
{
    [SerializeField] private ChangeScenes _changeScenes;

    private void OnTriggerEnter2D(Collider2D other)
    {
        _changeScenes.StartCoroutine(_changeScenes.FadeIn());
        //_changeScenes.PlayerLocation();
    }
}
