using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class CharacterIdentifier : SpriteObjectsSortingOrder
{
    public string Name;
    public string DefaultBlipSound;
    public float DefaultBlipFrequency = 4f / 60f;
    [MinMaxSlider(-100f, 100f)] [AllowNesting] public Vector2 DefaultPitchVariationRange;
    public bool Is3DCharacter;
    public bool IsTalking;
    public Animator _anim;
    public Character _character;

    private Blink _blink;
    private string _prevAnim;

    private void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
        base.Awake();
        _blink = GetComponent<Blink>();
    }

    public void LateUpdate()
    {
        base.LateUpdate();
        if (_character == null)
        {
            Debug.LogError($"Character Identifier {gameObject.name} is missing Character");
            return;
        }

        UpdateAnims();
    }

    public void UpdateAnims()
    {
        if (!_anim.isActiveAndEnabled) return;
        if (_anim.GetCurrentAnimatorClipInfo(0).Length > 0)
        {
            string str = _anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (_prevAnim != str && _blink != null)
            {
                _blink.UpdateBlinkString(str + "_blink");
            }

            _prevAnim = str;
        }
        
        if (Globals.IsAnimationPlaying(_anim, $"{_character.AnimPlaying}_opening")) return;
        
        if (IsTalking)
        {
            if (Is3DCharacter)
            {
                _anim.CrossFade($"{_character.AnimPlaying}_talk", 0.5f);
                
            }
            else
            {
                _anim.Play($"{_character.AnimPlaying}_talk");
            }

            if (_anim.layerCount >= 3)
                _anim.Play(
                    _anim.HasState(2, Animator.StringToHash($"{_character.AnimPlaying}_mouth_talk"))
                        ? $"{_character.AnimPlaying}_mouth_talk"
                        : "Null", 2);
        }
        else
        {
            if (Is3DCharacter)
            {
                _anim.CrossFade($"{_character.AnimPlaying}_idle", 0.5f);
            }
            else
            {
                _anim.Play($"{_character.AnimPlaying}_idle");
            }
            
            if (_anim.layerCount >= 3)
                _anim.Play(
                    _anim.HasState(2, Animator.StringToHash($"{_character.AnimPlaying}_mouth_idle"))
                        ? $"{_character.AnimPlaying}_mouth_idle"
                        : "Null", 2);
        }
        
        _anim.Update(0);
    }
}
