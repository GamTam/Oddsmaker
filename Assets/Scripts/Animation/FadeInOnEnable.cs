using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOnEnable : MonoBehaviour
{
    private const float _FadeInTime = 0.2f;
    [SerializeField] private SpriteRenderer[] _sprites;
    [SerializeField] private Image[] _spritesUI;

    private Coroutine _fadeRoutine;

    void Start()
    {
        _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        _spritesUI = GetComponentsInChildren<Image>(true);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
     
        if (_sprites.Length > 0)
        {
            foreach (SpriteRenderer spriteRenderer in _sprites)
            {
                spriteRenderer.color = Color.clear;
            }
        }
        
        if (_spritesUI.Length > 0)
        {
            foreach (Image spriteRenderer in _spritesUI)
            {
                spriteRenderer.color = Color.clear;
            }
        }
    }

    private void OnEnable()
    {
        this.EnsureCoroutineStopped(ref _fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;
        
        if (_sprites.Length > 0)
        {
            foreach (SpriteRenderer spriteRenderer in _sprites)
            {
                spriteRenderer.color = Color.clear;
            }
        }
        
        if (_spritesUI.Length > 0)
        {
            foreach (Image spriteRenderer in _spritesUI)
            {
                spriteRenderer.color = Color.clear;
            }
        }

        yield return null;
        
        while (time < _FadeInTime)
        {
            time += Time.deltaTime;
            
            if (_sprites.Length > 0)
            {
                foreach (SpriteRenderer spriteRenderer in _sprites)
                {
                    spriteRenderer.color = new Color(1, 1, 1, time / _FadeInTime);
                }
            }
            
            if (_spritesUI.Length > 0)
            {
                foreach (Image spriteRenderer in _spritesUI)
                {
                    spriteRenderer.color = new Color(1, 1, 1, time / _FadeInTime);
                }
            }

            yield return null;
        }
        
        if (_sprites.Length > 0)
        {
            foreach (SpriteRenderer spriteRenderer in _sprites)
            {
                spriteRenderer.color = new Color(1, 1, 1, 1);
            }
        }
        
        if (_spritesUI.Length > 0)
        {
            foreach (Image spriteRenderer in _spritesUI)
            {
                spriteRenderer.color = new Color(1, 1, 1, 1);
            }
        }
    }
}
