using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Battleable : MonoBehaviour
{
    public string _name;
    public int _level;
    public int _HP;
    public int _maxHP;
    public int _pow;
    public int _def;
    public int _luck;
    public int _speed;
    public int _exp;
    public Slider _slider;
    public Slider[] _redSliders;
    
    protected float _timer = 0;
    protected Material _mat;
    [HideInInspector] public Vector3 StartingLocation;

    public void InitSetRedSlider(float targetValue)
    {
        StartCoroutine(SetRedSlider(targetValue));
    }

    public IEnumerator SetRedSlider(float targetValue)
    {
        float movementDuration = 2;
        float timeElapsed = 0;

        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            foreach (Slider slider in _redSliders)
            {
                slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * 5);
            }

            yield return null;
        }

        foreach (Slider slider in _redSliders)
        {
            slider.value = targetValue;
        }
    }
    
    protected void FlashWhite()
    {
        _timer += Time.deltaTime * 5;

        float intensity = Mathf.Sin(_timer) + 1f;
        
        float factor = Mathf.Pow(2, intensity);
        Color color = new Color(factor, factor, factor);
        
        _mat.SetColor("_Color_2", color);
    }
}