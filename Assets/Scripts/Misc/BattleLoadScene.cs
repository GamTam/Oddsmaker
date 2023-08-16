using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class BattleLoadScene : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _whiteFlash;
    [SerializeField] private Volume _monochromeVolume;
    [SerializeField] private GameObject _fieldEffects;

    [SerializeField] private BattleManager _battle;
    private Scene _scene;
    private GameState _state;
    private string _prevControlState;

    private string _prevSong;
    private float _prevSongPlace;

    public IEnumerator BattleTransition(string scene = "Battle Scene", bool continueMusic = true, bool stopCurrentSong = true)
    {
        _scene = SceneManager.GetActiveScene();
        _state = Globals.GameState;
        
        float movementDuration = 0.1f;
        float timeElapsed = 0;

        _prevControlState = Globals.Input.currentActionMap.name;
        Globals.Input.SwitchCurrentActionMap("Null");

        if (continueMusic)
        {
            _prevSong = Globals.MusicManager.GetMusicPlaying().name;
            _prevSongPlace = Globals.MusicManager.GetMusicPlaying().source.time;
        }
        
        if (!stopCurrentSong) Globals.MusicManager.Stop();
        Globals.SoundManager.Play("battleStart");
        
        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
           _whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, timeElapsed / movementDuration));
            yield return null;
        }

        _whiteFlash.color = new Color(1, 1, 1, 1);
        _fieldEffects.SetActive(false);

        _monochromeVolume.weight = 1;
        
        movementDuration = 2f;
        timeElapsed = 0;
            
        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            _whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, timeElapsed / movementDuration));
            yield return null;
        }
        _whiteFlash.color = new Color(1, 1, 1, 0);
        
        StartCoroutine(Globals.LoadScene(scene, true));
        if (!stopCurrentSong) yield return new WaitForSeconds(0.5f);
        Globals.BeginSceneLoad = true;

        while (Globals.BeginSceneLoad) yield return null;

        SceneManager.SetActiveScene(_scene);
        _battle = FindObjectOfType<BattleManager>();
        _battle._overworldMusic = stopCurrentSong;

        while (_battle._inBattle) yield return null;

        _fieldEffects.SetActive(true);
        _monochromeVolume.weight = 0;

        while (_battle._mat.GetFloat("_alpha") > 0) yield return null;
        
        Globals.UnloadAllScenesExcept(_scene.name);
        
        Globals.InBattle = false;
        Globals.GameState = _state;
        Globals.Input.SwitchCurrentActionMap(_prevControlState);
        if (continueMusic && !stopCurrentSong) Globals.MusicManager.fadeIn(_prevSong, _prevSongPlace, 0.5f);
    }
}
