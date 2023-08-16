using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SwapCharacters : MonoBehaviour
{
    [SerializeField] private MeshRenderer _mesh;
    [SerializeField] private float _speed = 5;
    public bool _done;
    public bool _swapped;
    public Vector2 CharPos;

    public List<Character> _chars = new List<Character>();
    public List<Character> _charPrefabs = new List<Character>();

    private void Awake()
    {
        _mesh.material.color = Color.white;
    }

    private void LateUpdate()
    {
        if (_chars.Count <= 0) return;
        foreach (Character chara in _chars)
        {
            if (chara.CharOnScreen == null) continue;
            Vector3 charPos = CharPos + chara.Offset;
            charPos.z = chara.zPos;
            chara.CharOnScreen.transform.localPosition = charPos;
        }
    }

    public void UpdateCharPos(Vector2 vec)
    {
        CharPos = vec;

        if (_chars.Count <= 0) return;
        foreach (Character chara in _chars)
        {
            if (chara.CharOnScreen == null) continue;
            Vector3 charPos = CharPos + chara.Offset;
            charPos.z = chara.zPos;
            chara.CharOnScreen.transform.localPosition = charPos;
        }
    }

    public void QuickSwap(List<Character> newChar, Vector2 pos, bool killChar)
    {
        if (_chars.Count > 0 && killChar)
        {
            foreach (Character obj in _chars)
            {
                Destroy(obj.CharOnScreen);
            }
        }
        
        _chars = new List<Character>();
        _charPrefabs = new List<Character>();

        if (newChar.Count == 0)
        {
            return;
        }

        int i = 0;
        List<string> list = new List<string>();
        
        foreach (Character chara in newChar)
        {
            _charPrefabs.Add((Character) chara.Clone());
            Character charb = new Character();
            charb.CharOnScreen = Instantiate(chara.CharOnScreen, gameObject.transform, false);
            Vector3 charPos = pos + chara.Offset;
            charPos.z = i;

            _chars.Add(charb);
            
            int count = list.Where(temp => temp.Equals(chara.CharOnScreen.name))
                .Select(temp => temp)
                .Count();
            
            list.Add(chara.CharOnScreen.name);
            
            charb.AnimPlaying = chara.AnimPlaying;
            charb.InFrontOfForeground = chara.InFrontOfForeground;
            charb.Offset = chara.Offset;
            charb.Flip = chara.Flip;
            charb.DontWaitForOpeningAnimation = chara.DontWaitForOpeningAnimation;
            charb.SkipOpeningAnimation = chara.SkipOpeningAnimation;
            charb.CharOnScreen.transform.localPosition = charPos;
            charb.DontWaitForOpeningAnimationToEnd = chara.DontWaitForOpeningAnimationToEnd;
            charb.CharOnScreen.transform.localScale = Vector3.one * 0.01f;
            charb.MoveSpeed = chara.MoveSpeed;
            charb.MoveTime = chara.MoveTime;
            charb.zPos = charb.InFrontOfForeground ? -4 - (newChar.Count - i) : i + newChar.Count - 1;
            charb.CharOnScreen.name = charb.CharOnScreen.name.Replace("(Clone)", "");
            
            if (count > 0) charb.CharOnScreen.name = charb.CharOnScreen.name + " (" + count + ")";
            else charb.CharOnScreen.name = charb.CharOnScreen.name;
            
            charb.CharOnScreen.GetComponent<Animator>().Play($"{charb.AnimPlaying}_idle");
            charb.CharOnScreen.GetComponent<CharacterIdentifier>()._character = charb;
            
            Vector3 scale = charb.CharOnScreen.transform.localScale;
            if (chara.Flip)
            {
                if (charb.CharOnScreen.transform.localScale.x > 0) scale.x *= -1;
            }
            else
            {
                scale.x = Mathf.Abs(scale.x);
            }

            charb.CharOnScreen.transform.localScale = scale;
        
            _mesh.material.color = new Color(1f, 1f, 1f, 1f);
            i += 1;
        }

        CharPos = pos;
    }

    public void StartSwap(List<Character> newChar, float speed=-3, bool fadeIn=true, bool skipFade=false, Vector2? pos=null)
    {
        if (pos == null) pos = CharPos;
        
        if (speed <= 0)
        {
            speed = _speed;
        }
        StartCoroutine(Swap(newChar, speed, fadeIn, skipFade, (Vector2) pos));
    }

    private IEnumerator Swap(List<Character> newChar, float speed, bool fadeIn, bool skipFade, Vector2 pos)
    {
        _done = false;
        _swapped = false;
        
        if (_mesh.material.color.a != 0)
        {
            if (!skipFade)
            {
                float time = 0;
                float duration = speed;

                while (time < duration)
                {
                    time += Time.deltaTime;
                    float frac = 1 - time / duration;
                    _mesh.material.color = new Color(frac, frac, frac, frac);

                    yield return null;
                }
            }

            _mesh.material.color = new Color(0, 0, 0, 0);
        }
        
        CharPos = pos;
        
        if (newChar.Count > 0)
        {
            KillChars();
            
            _chars = new List<Character>();
            _charPrefabs = new List<Character>();

            int i = 0;
            foreach (Character chara in newChar)
            {
                _charPrefabs.Add((Character) chara.Clone());
                Character charb = new Character();
                charb.CharOnScreen = Instantiate(chara.CharOnScreen, gameObject.transform, false);
                Vector3 charPos = pos + chara.Offset;
                charPos.z = i;

                _chars.Add(charb);
                charb.AnimPlaying = chara.AnimPlaying;
                charb.InFrontOfForeground = chara.InFrontOfForeground;
                charb.Offset = chara.Offset;
                charb.Flip = chara.Flip;
                charb.DontWaitForOpeningAnimation = chara.DontWaitForOpeningAnimation;
                charb.SkipOpeningAnimation = chara.SkipOpeningAnimation;
                charb.DontWaitForOpeningAnimationToEnd = chara.DontWaitForOpeningAnimationToEnd;
                charb.CharOnScreen.transform.localPosition = charPos;
                charb.CharOnScreen.transform.localScale = Vector3.one * 0.01f;
                charb.MoveSpeed = chara.MoveSpeed;
                charb.MoveTime = chara.MoveTime;
                charb.CharOnScreen.name = charb.CharOnScreen.name.Replace("(Clone)", "");
                charb.CharOnScreen.GetComponent<Animator>().Play($"{charb.AnimPlaying}_idle");
                charb.zPos = charb.InFrontOfForeground ? -4 - (newChar.Count - i) : i + newChar.Count - 1;

                charb.CharOnScreen.GetComponent<CharacterIdentifier>()._character = charb;
            
                Vector3 scale = charb.CharOnScreen.transform.localScale;
                if (chara.Flip)
                {
                    if (charb.CharOnScreen.transform.localScale.x > 0) scale.x *= -1;
                }
                else
                {
                    scale.x = Mathf.Abs(scale.x);
                }

                charb.CharOnScreen.transform.localScale = scale;
        
                i += 1;
            }

            _mesh.material.color = new Color(0, 0, 0, 0);

            _swapped = true;
            if (!skipFade) yield return new WaitForSeconds(0.1f);
            
            if (fadeIn)
            {
                if (!skipFade)
                {
                    float time = 0;
                    float duration = speed;
                    
                    while (time < duration)
                    {
                        time += Time.deltaTime;
                        float frac = time / duration;
                        _mesh.material.color = new Color(frac, frac, frac, frac);
                        yield return null;
                    }
                }

                _mesh.material.color = new Color(1f, 1f, 1f, 1f);
            }
        }
        else
        {
            _mesh.material.color = new Color(0, 0, 0, 0);
            
            KillChars();
            
            _chars = new List<Character>();
            _charPrefabs = new List<Character>();
        }

        _done = true;
    }

    public void KillChars()
    {
        if (_chars.Count > 0)
        {
            foreach (Character obj in _chars)
            {
                Destroy(obj.CharOnScreen);
            }
        }
    }

    public bool CompareCharList(List<Character> chars)
    {
        for (int i = 0; i < chars.Count; i++)
        {
            try
            {
                if (_chars[i].CharOnScreen.GetComponent<SpriteObjectsSortingOrder>().ID !=
                    chars[i].CharOnScreen.GetComponent<SpriteObjectsSortingOrder>().ID) return false;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator MoveChar(Character chara, Vector2 endPos)
    {
        float time = 0;
        float duration = chara.MoveTime;
        Vector2 startPos = chara.Offset;

        switch (chara.MoveSpeed)
        {
            case ScrollSpeedOptions.Default:
                duration = 0.5f;
                break;
            case ScrollSpeedOptions.Fast:
                duration = 0.25f;
                break;
            case ScrollSpeedOptions.Slow:
                duration = 1f;
                break;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            
            chara.Offset = Vector2.Lerp(startPos, endPos, time / duration);
            yield return null;
        }
    }
}
