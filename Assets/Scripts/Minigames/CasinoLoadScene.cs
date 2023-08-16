using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CasinoLoadScene : MonoBehaviour
{
    [SerializeField] private CasinoManager _battle;
    [SerializeField] private MinigameSelection _mingame;
    private Scene _scene;
    private GameState _state;
    private string _prevControlState;

    private string _prevSong;
    private float _prevSongPlace;

    public IEnumerator BattleTransition()
    {
        _scene = SceneManager.GetActiveScene();
        _state = Globals.GameState;
        string prevSong = Globals.MusicManager.GetMusicPlaying().name;
        
        Globals.MusicManager.FadeVariation("Minigame");
        StartCoroutine(Globals.LoadScene(_mingame.ToString(), true));
        Globals.BeginSceneLoad = true;
        Globals.Input.SwitchCurrentActionMap("Menu");

        while (Globals.BeginSceneLoad) yield return null;

        SceneManager.SetActiveScene(_scene);
        _battle = FindObjectOfType<CasinoManager>();

        while (!_battle._done) yield return null;

        Globals.UnloadAllScenesExcept(_scene.name);
        Globals.MusicManager.FadeVariation(prevSong);
        
        Globals.InBattle = false;
        Globals.GameState = _state;
        Globals.Input.SwitchCurrentActionMap("Overworld");
    }
}

enum MinigameSelection
{
    Matching,
    Poker,
    Wheel
}
