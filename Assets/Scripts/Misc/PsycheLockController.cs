using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsycheLockController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _lock;

    private Flash _flash;

    private Vector2[][] _lockPositions = new[]
    {
        new Vector2[]
        {
            new Vector2(0, -1.5f)
        },
        
        new Vector2[]
        {
            new Vector2(-6.5f, 0),
            new Vector2(6.5f, 0),
        },
        
        new Vector2[]
        {
            new Vector2(0, -1.35f),
            new Vector2(-6.5f, 0),
            new Vector2(6.5f, 0),
        },
        
        new Vector2[]
        {
            new Vector2(-4.6f, 3f),
            new Vector2(4.6f, 3f),
            new Vector2(-6.5f, -0.5f),
            new Vector2(6.5f, -0.5f),
        },
        
        new Vector2[]
        {
            new Vector2(0, -1.35f),
            new Vector2(-4.6f, 3f),
            new Vector2(4.6f, 3f),
            new Vector2(-6.5f, -0.6f),
            new Vector2(6.5f, -0.6f),
        }
    };
    
    private List<Animator> _activeLocks = new List<Animator>();
    
    [HideInInspector] public bool Locked;
    [HideInInspector] public int _lockCount;
    [HideInInspector] public int _maxLockCount;

    private void Start()
    {
        _flash = FindObjectOfType<Flash>();
    }

    public IEnumerator StartLock(int lockCount)
    {
        _lockCount = lockCount;
        _maxLockCount = lockCount;
        Locked = true;
        _activeLocks = new List<Animator>();
        
        _animator.Play($"Chains Appear {lockCount}");
        Sound chains = Globals.SoundManager.Play($"lockChain{lockCount}", 1f);

        while (!Globals.IsAnimationPlaying(_animator, $"Chains Appear {lockCount}", 0))
        {
            yield return null;
        }

        float shakeLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        
        Shake obj = GameObject.FindWithTag("MainCamera").GetComponent<Shake>();
        obj.maxShakeDuration = shakeLength;
        obj.multiplier = 1.25f;
        obj.enabled = true;
        
        while (Globals.IsAnimationPlaying(_animator, $"Chains Appear {lockCount}", 0))
        {
            yield return null;
        }

        Globals.SoundManager.FadeOut(chains, 0.5f);
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < lockCount; i++)
        {
            Animator anim = Instantiate(_lock, gameObject.transform, false).GetComponent<Animator>();
            anim.gameObject.SetActive(true);

            Vector3 pos = _lockPositions[lockCount - 1][i];
            pos.z = -12.5f;
            
            anim.gameObject.transform.localPosition = pos;
            
            if (i == 0) anim.Play("Lock Appear First");
            
            _activeLocks.Add(anim);
            yield return new WaitForSeconds(0.13f);
        }
        
        
        obj.maxShakeDuration = 22f/60f;
        obj.multiplier = 2;
        obj.enabled = true;
        yield return new WaitForSeconds(22 / 60f);
        _flash.StartFlash();
        
        yield return new WaitForSeconds(0.75f);
    }

    private void OnDisable()
    {
        Locked = false;
        foreach (Animator pLock in _activeLocks)
        {
            Destroy(pLock.gameObject);
        }
    }

    public IEnumerator BreakLock()
    {
        if (_lockCount <= 0) yield break;

        _lockCount -= 1;
        GameObject pLock = _activeLocks[^1].gameObject;
        
        pLock.transform.GetChild(0).gameObject.SetActive(false);
        pLock.transform.GetChild(1).gameObject.SetActive(true);
        Globals.SoundManager.Play("lockBreak");

        yield return new WaitForSeconds(1.1f);
        _activeLocks.RemoveAt(_activeLocks.Count - 1);
        Destroy(pLock);
    }

    public IEnumerator RemoveChains()
    {
        while (_lockCount > 0)
        {
            yield return BreakLock();
            yield return new WaitForSeconds(0.25f);
        }
        
        _animator.Play($"Chains Leave {_maxLockCount}");
        Sound chains = Globals.SoundManager.Play($"lockChain{_maxLockCount}", 1f);

        while (!Globals.IsAnimationPlaying(_animator, $"Chains Leave {_maxLockCount}", 0))
        {
            yield return null;
        }

        float shakeLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        
        Shake obj = GameObject.FindWithTag("MainCamera").GetComponent<Shake>();
        obj.maxShakeDuration = shakeLength;
        obj.multiplier = 1.25f;
        obj.enabled = true;
        
        while (Globals.IsAnimationPlaying(_animator, $"Chains Leave {_maxLockCount}", 0))
        {
            yield return null;
        }

        Globals.SoundManager.FadeOut(chains, 0.5f);
        yield return new WaitForSeconds(0.4f);
        _maxLockCount = 0;
    }
}
