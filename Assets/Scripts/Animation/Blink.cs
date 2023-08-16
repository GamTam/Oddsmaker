using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Blink : MonoBehaviour
{
    private Animator _anim;
    private float _timer;
    private bool _enabled = true;

    public string _animName = "Blink";
    
    void Awake()
    {
        _timer = Random.Range(0f, 7f);
        _anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (_anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1f)
        {
            _anim.Play("Null", 1, 0);
        }
        
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _timer = Random.Range(1f, 7f);
            if (_enabled) _anim.Play(_animName, 1, 0);
        }
    }

    public void BlinkEnabled()
    {
        _enabled = true;
    }
    
    public void BlinkDisabled()
    {
        _enabled = false;
        _anim.Play("Null", 1, 0);
    }

    public void UpdateBlinkString(string name)
    {
        bool blink = false;
        float animTime = 0f;
        
        if (_anim.HasState(1, Animator.StringToHash(name)))
        {
            if (_anim.GetCurrentAnimatorClipInfo(1).Length > 0)
            {
                string str = _anim.GetCurrentAnimatorClipInfo(1)[0].clip.name;
                if (str == _animName)
                {
                    blink = true;
                    animTime = _anim.GetCurrentAnimatorStateInfo(1).normalizedTime;
                }
            }
                
            _animName = name;
            if (blink)
            {
                _anim.Play(_animName, 1, animTime);
            }
                
            return;
        }
                
        if (_anim.GetCurrentAnimatorClipInfo(1).Length > 0)
        {
            string str = _anim.GetCurrentAnimatorClipInfo(1)[0].clip.name;
            if (str == _animName)
            {
                blink = true;
                animTime = _anim.GetCurrentAnimatorStateInfo(1).normalizedTime;
            }
        }
                
        _animName = "Blink";
        
        if (blink)
        {
            _anim.Play(_animName, 1, animTime);
        }
    }
    
    [Button]
    private void StartBlink()
    {
        _timer = -12;
    }
}
