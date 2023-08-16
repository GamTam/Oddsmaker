using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Flash : MonoBehaviour
{
    [SerializeField] private float _flashLength;
    
    private SpriteRenderer _sprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    [Button]
    public void StartFlash()
    {
        StartCoroutine(Flashing());
    }

    private IEnumerator Flashing()
    {
        _sprite.color = Color.white;

        float time = 0;
        float duration = _flashLength;

        while (time < duration)
        {
            yield return null;
            time += Time.deltaTime;
            _sprite.color = new Color(1, 1, 1, 1 - (time / duration));
        }

        yield return null;
        _sprite.color = Color.clear;;
    }
}
