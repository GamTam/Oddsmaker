using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffectTriggersLayton : AnimationEffectTriggers
{
    [SerializeField] private SkinnedMeshRenderer _eyes;
    [SerializeField] private SkinnedMeshRenderer _mouth;

    private void Awake()
    {
        base.Awake();
        
        _eyes.material.mainTextureOffset = Vector2.zero;
        _mouth.material.mainTextureOffset = Vector2.zero;
    }

    public void SetEyeOffset(Vector2 offset)
    {
        _eyes.material.mainTextureOffset = offset;
    }
    
    public void SetMouthOffset(Vector2 offset)
    {
        _mouth.material.mainTextureOffset = offset;
    }

    public void Blink(float blinkTime)
    {
        StartCoroutine(BlinkRoutine(blinkTime));
    }

    private IEnumerator BlinkRoutine(float blinkTime)
    {
        Vector2 prevOffset = _eyes.material.mainTextureOffset;

        _eyes.material.mainTextureOffset = new Vector2(0.5f, 0f);
        yield return new WaitForSeconds(blinkTime);

        _eyes.material.mainTextureOffset = prevOffset;
    }
}
