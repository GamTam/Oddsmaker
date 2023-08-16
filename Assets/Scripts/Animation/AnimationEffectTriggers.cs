using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffectTriggers : MonoBehaviour
{
    private SoundManager _soundManager;
    private Flash _flash;

    protected void Awake()
    {
        _flash = FindObjectOfType<Flash>();
    }

    public void PlaySound(string sound)
    {
        Globals.SoundManager.Play(sound);
    }

    public void Flash()
    {
        if (_flash == null) _flash = FindObjectOfType<Flash>();
        
        _flash.StartFlash();
    }

    public void ScreenShake(float duration = 0.25f)
    {
        Shake obj = GameObject.FindWithTag("MainCamera").GetComponent<Shake>();
        obj.maxShakeDuration = duration;
        obj.enabled = true;
    }
}
