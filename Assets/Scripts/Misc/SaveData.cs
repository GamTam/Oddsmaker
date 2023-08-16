using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveData : MonoBehaviour
{
    [SerializeField] private SaveDataFormatter _save;

    private void Awake()
    {
        // LoadFromJson(false);
        // if (SceneManager.GetActiveScene().name != _save.CurrentScene) SceneManager.LoadScene(_save.CurrentScene);
    }

    public void Update()
    {
        Globals.PlayTime += Time.deltaTime;
    }

    public void SaveIntoJson()
    {
        _save = new SaveDataFormatter();
        
        _save.CurrentScene = SceneManager.GetActiveScene().name;
        _save.PlayerPos = Globals.Player.transform.position;
        _save.Items = Globals.Items;
        _save.PlayerStats = Globals.PlayerStatsList;
        _save.PlayedCutscenes = Globals.PlayedCutscenes;
        _save.OpenedChests = Globals.OpenedChests;
        _save.PlayTime = Globals.PlayTime;
        _save.PlayerDir = Globals.PlayerDir; 
        _save.SongPlaying = Globals.MusicManager.GetMusicPlaying() == null ? "" : Globals.MusicManager.GetMusicPlaying().name;
        
        string jason = JsonUtility.ToJson(_save, true);
        
        //File.WriteAllText(Application.persistentDataPath + $"/FILE_{Globals.SaveFile}.oddsmaker",  Globals.EncryptString(jason, "vonkarma"));
        File.WriteAllText(Application.persistentDataPath + $"/FILE_{Globals.SaveFile}.oddsmaker", jason);
    }

    public void LoadFromJson()
    {
        if (!File.Exists(Application.persistentDataPath + $"/FILE_{Globals.SaveFile}.oddsmaker")) return;
        
        string jason = File.ReadAllText(Application.persistentDataPath + $"/FILE_{Globals.SaveFile}.oddsmaker");
        
        //_save = JsonUtility.FromJson<SaveDataFormatter>(Globals.DecryptString(jason, "vonkarma"));
        _save = JsonUtility.FromJson<SaveDataFormatter>(jason);
        
        Globals.PlayerPos = _save.PlayerPos;
        Globals.Items = _save.Items;
        Globals.PlayerStatsList = _save.PlayerStats;
        Globals.PlayedCutscenes = _save.PlayedCutscenes;
        Globals.OpenedChests = _save.OpenedChests;
        Globals.PlayTime = _save.PlayTime;
        Globals.PlayerDir = _save.PlayerDir;
        Globals.Player.SetFacing(_save.PlayerDir);
        Globals.MusicManager.Play(_save.SongPlaying);
        
        SceneManager.LoadScene(_save.CurrentScene);
    }
}

[Serializable]
public class SaveDataFormatter
{
    public double PlayTime;
    public string CurrentScene;
    public Vector3 PlayerPos;
    public List<sItem> Items;
    public PlayerStats[] PlayerStats;
    public List<string> PlayedCutscenes;
    public List<string> OpenedChests;
    public PlayerDir PlayerDir;
    public string SongPlaying;
}