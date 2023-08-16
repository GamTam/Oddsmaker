using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

[Serializable]
public class CutsceneAction
{
    [AllowNesting] public string Name;
    public bool PlayWithNext;
    [HideInInspector] public Cutscene Parent;

    public virtual IEnumerator Play()
    {
        yield break;
    }
}

[Serializable]
public class DialogueAction : CutsceneAction
{
    [SerializeField] private SpriteRenderer Speaker;
    [SerializeField] private bool SkipCloseAnimation;
    [SerializeField] private bool ForceBottom;
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] [TextArea(3, 4)] private string[] Dialogue;

    public DialogueAction()
    {
        Name = "Speech Bubble";
    }

    public override IEnumerator Play()
    {
        DialogueManager manager = Object.FindObjectOfType<DialogueManager>();
        manager.StartText(Dialogue, Speaker.gameObject.transform, Speaker, SkipCloseAnimation,
            forceBottom: ForceBottom, font:_font);

        while (!manager._done)
        {
            yield return null;
        }
    }
}

[Serializable]
public class MoveObjAction : CutsceneAction
{
    [SerializeField] private bool _relative;
    [SerializeField] private GameObject _gameObject;
    [SerializeField] private int _speed;
    [SerializeField] private Vector2[] MoveList;
    
    public MoveObjAction()
    {
        Name = "Move Object";
    }

    public override IEnumerator Play()
    {
        for (int i = 0; i < MoveList.Length; i++)
        {
            Vector3 targetPos = MoveList[i];
            if (_relative) targetPos += _gameObject.transform.position;
            targetPos.z = _gameObject.transform.position.z;

            while (_gameObject.transform.position != targetPos)
            {
                _gameObject.transform.position = Vector3.MoveTowards(_gameObject.transform.position, targetPos, _speed * Time.deltaTime);
                yield return null;
            }
        }
    }
}

[Serializable]
public class PlayAnimAction : CutsceneAction
{
    [SerializeField] private Animator _animator;
    [SerializeField] [ShowIf("HasChar")] [Dropdown("animationNameList")]  private string _animation;

    public PlayAnimAction()
    {
        Name = "Play Animation";
    }
    
    public override IEnumerator Play()
    {
        _animator.Play(_animation);
        yield return null;
    }
    
    public bool HasChar => _animator != null;
    
    List<string> animationNameList()
    {
        List<string> list = new List<string>();
        
        foreach(AnimationClip ac in _animator.runtimeAnimatorController.animationClips)
        {
            string name = ac.name.ToLower();
            list.Add(name);
        }

        return list;
    }
}

[Serializable]
public class MusicAction : CutsceneAction
{
    [SerializeField] private string _songName;
    [SerializeField] private bool _fade;
    [SerializeField] [ShowIf("_fade")] [AllowNesting]
    private float _fadeLength;
    [SerializeField] private bool _stopSong;
    [SerializeField] private bool _fadeOut;
    
    public MusicAction()
    {
        Name = "Music";
    }
    
    public override IEnumerator Play()
    { 
        if (_fadeOut) Globals.MusicManager.fadeOut();
        else if (_stopSong) Globals.MusicManager.Stop();
        else
        {
           if (_fade)
           {
               Globals.MusicManager.fadeIn(song:_songName, duration:_fadeLength);
           }
           else
           {
               Globals.MusicManager.Play(_songName);
           }
        }

        yield return null;
    }
}

[Serializable]
public class WaitAction : CutsceneAction
{
    [SerializeField] private float _waitTime;
    
    public WaitAction()
    {
        Name = "Wait";
    }
    
    public override IEnumerator Play()
    {
        yield return new WaitForSeconds(_waitTime);
    }
}

[Serializable]
public class VideoAction : CutsceneAction
{
    [SerializeField] private VideoClip _video;
    [SerializeField] private float _playbackSpeed = 1;

    private VideoPlayer _videoPlayer;

    public VideoAction()
    {
        Name = "Play Video";
    }

    public override IEnumerator Play()
    {
        _videoPlayer = Object.FindObjectOfType<VideoPlayer>();
        _videoPlayer.playbackSpeed = _playbackSpeed;
        _videoPlayer.clip = _video;
        _videoPlayer.Prepare();
        Material mat = _videoPlayer.gameObject.GetComponent<MeshRenderer>().materials[0];
        mat.color = new Color(1, 1, 1, 1);

        while (!_videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        _videoPlayer.Play();
        
        do
        {
            yield return null;
        } while (_videoPlayer.isPlaying);

        mat.color = new Color(1, 1, 1, 0);
    }
}

[Serializable]
public class ScreenFadeAction : CutsceneAction
{
    [SerializeField] private Color _screenColor;
    [SerializeField] private float _duration;

    public ScreenFadeAction()
    {
        Name = "Fade To Color";
    }

    public override IEnumerator Play()
    {
        SpriteRenderer screen = GameObject.Find("Flash").GetComponent<SpriteRenderer>();
        Color startColor = screen.color;
        
        float timeElapsed = 0f;

        while (timeElapsed < _duration)
        {
            timeElapsed += Time.deltaTime;
            screen.color = Color.Lerp(startColor, _screenColor, timeElapsed/_duration);
            yield return null;
        }

        screen.color = _screenColor;
    }
}

[Serializable]
public class BattleAction : CutsceneAction
{
    [SerializeField] private string _battleScene;
    [SerializeField] private bool _keepPlayingCurrentSong;
    [SerializeField] private bool _stopSongAfterBattle;

    public BattleAction()
    {
        Name = "Start Battle";
    }

    public override IEnumerator Play()
    {
        BattleLoadScene battle = Object.FindObjectOfType<BattleLoadScene>();

        yield return battle.StartCoroutine(battle.BattleTransition(_battleScene, continueMusic:!_stopSongAfterBattle, stopCurrentSong:_keepPlayingCurrentSong));
    }
}
