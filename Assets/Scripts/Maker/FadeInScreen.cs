using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInScreen : MonoBehaviour
{
    [SerializeField] public GameObject _fade;
    
    IEnumerator Start()
    {
        Vector3 position = _fade.transform.position;
        position = new Vector3(position.x, position.y, (int) BGFadePos.Everything);
        _fade.transform.position = position;

        SpriteRenderer spr = _fade.GetComponent<SpriteRenderer>();
        Color startColor = Color.black;
        Color endColor = Color.clear;
        float time = 0;

        Globals.MusicManager.Play("Explore2_AAI2");
        
        while (time < 1)
        {
            time += Time.deltaTime;
            spr.color = Color.Lerp(startColor, endColor, time / 1);
            if (Math.Abs(spr.color.a - endColor.a) < 0.0001) break;
            yield return null;
        }
    }
}
