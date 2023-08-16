using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScenes : MonoBehaviour
{
    [SerializeField] private Color _screenColor;
    [SerializeField] private Color _alphaColor;
    [SerializeField] private float _duration;
    [SerializeField] private string _scene;
    [SerializeField] private string _gameObjectName;

    bool _entered;    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!_entered)
        {
            _entered = true;
            StartCoroutine(FadeOut());
        }
    }

    public IEnumerator FadeOut(string scene = "")
    {
        if (string.IsNullOrEmpty(scene)) scene = _scene;
        SpriteRenderer screen = GameObject.Find("Black").GetComponent<SpriteRenderer>();
        Color startColor = screen.color;

        float timeElapsed = 0f;

        while (timeElapsed < _duration)
        {
            timeElapsed += Time.deltaTime;
            screen.color = Color.Lerp(startColor, _screenColor, timeElapsed/_duration);
            yield return null;
        }

        screen.color = _screenColor;
        SwitchScene(scene);
    }

    public IEnumerator FadeIn()
    {
        SpriteRenderer screen = GameObject.Find("Black").GetComponent<SpriteRenderer>();
        Color endColor = screen.color;
        endColor.a = 0;
        
        float timeElapsed = 0f;

        while (timeElapsed < _duration)
        {
            timeElapsed += Time.deltaTime;
            screen.color = Color.Lerp(_screenColor, endColor, timeElapsed/_duration);
            yield return null;
        }

        screen.color = endColor;
    }

    public void SwitchScene(string scene)
    {
        PlayerPrefs.SetString("MyStringKey", _gameObjectName);
        Globals.TurnOnWarpFlag();
        SceneManager.LoadScene(sceneName:scene);
    }
}
