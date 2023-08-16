using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SaveData : MonoBehaviour
{
    [SerializeField] private SaveDataFormatter _save;
    [SerializeField] private SettingsDataFormatter _settings;
    [SerializeField] private UnlockedCasesformatter UnlockedCases;
    [SerializeField] private AudioMixer _mixer;
    
    private PlayerInput _playerInput;
    private InputAction _saveSettings;
    private InputAction _loadSettings;

    private void Awake()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _saveSettings = _playerInput.actions["Textbox/SaveSettings"];
        _loadSettings = _playerInput.actions["Textbox/LoadSettings"];
    }

    public void Update()
    {
        Globals.PlayTime += Time.deltaTime;

        if (_saveSettings.triggered)
        {
            SaveSettingsIntoJson();
        }

        if (_loadSettings.triggered)
        {
            LoadSettingsFromJson();
        } 
    }

    public void SaveGameIntoJson()
    {
        _save = new SaveDataFormatter();
        
        _save.PlayTime = Globals.PlayTime;
        _save.SaveDate = System.DateTime.UtcNow.ToLocalTime().ToString("yyyy/MM/dd\nHH:mm:ss");
        _save.SongPlaying = Globals.SongPlaying;
        _save.HP = Globals.HP;
        _save.DialogueIndex = Mathf.Max(Globals.DialogueManager._currentLine - 1, 0);
        _save.Dialogue = Globals.DialogueManager._dialogue.name;
        _save.GameName = Globals.GameName;
        _save.GameAuthor = Globals.GameAuthor;
        _save.CaseName = Globals.CaseName;
        _save.CaseNum = Globals.CaseNum;
        _save.PhaseName = Globals.PhaseName;
        _save.GameColor = Globals.GameColor;
        
        foreach (EvidenceSO evidence in Globals.Evidence)
        {
            _save.Evidence.Add(evidence.name);
        }
        
        foreach (EvidenceSO profile in Globals.Profiles)
        {
            _save.Profiles.Add(profile.name);
        }
        
        foreach (DialogueSO dialogue in Globals.UsedDialogue)
        {
            _save.UsedDialogue.Add(dialogue.name);
        }
        
        foreach (TalkSO talks in Globals.UsedTalks)
        {
            _save.UsedTalks.Add(talks.name);
        }
        
        _save.StoryFlags = Globals.StoryFlags;
        
        string jason = JsonUtility.ToJson(_save, true);
        if (!Directory.Exists(Globals.SaveDataPath + "/Saves"))
        {
            Directory.CreateDirectory(Globals.SaveDataPath + "/Saves");
        }
        
        //File.WriteAllText(Globals.SaveDataPath + $"/Saves/FILE_{Globals.SaveFile}.law",  Globals.EncryptString(jason, "phoenixw"));
        File.WriteAllText(Globals.SaveDataPath + $"/Saves/FILE_{Globals.SaveFile}.law",  jason);
    }

    public void LoadGameFromJson()
    {
        if (!File.Exists(Globals.SaveDataPath + $"/Saves/FILE_{Globals.SaveFile}.law")) return;
        
        string jason = File.ReadAllText(Globals.SaveDataPath + $"/Saves/FILE_{Globals.SaveFile}.law");
        //_save = JsonUtility.FromJson<SaveDataFormatter>(Globals.DecryptString(jason, "phoenixw"));
        _save = JsonUtility.FromJson<SaveDataFormatter>(jason);
        
        Globals.PlayTime = _save.PlayTime;
        Globals.DialogueManager._hpBar.SetHP(_save.HP);
        
        Globals.GameAuthor = _save.GameAuthor;
        Globals.GameName = _save.GameName;
        Globals.CaseName = _save.CaseName;
        Globals.CaseNum = _save.CaseNum;
        Globals.PhaseName = _save.PhaseName;
        Globals.GameColor = _save.GameColor;
        
        DialogueSO[] allDialogue = Resources.LoadAll<DialogueSO>($"Scriptable Objects/{RemoveSpecialCharacters(Globals.GameName)}/{RemoveSpecialCharacters(Globals.CaseName)}/{RemoveSpecialCharacters(Globals.PhaseName)}");
        EvidenceSO[] allEvidence = Resources.LoadAll<EvidenceSO>($"Scriptable Objects/{RemoveSpecialCharacters(Globals.GameName)}/{RemoveSpecialCharacters(Globals.CaseName)}/Evidence");
        TalkSO[] allTalks = Resources.LoadAll<TalkSO>($"Scriptable Objects/{RemoveSpecialCharacters(Globals.GameName)}/{RemoveSpecialCharacters(Globals.CaseName)}/{RemoveSpecialCharacters(Globals.PhaseName)}");
        
        Globals.Evidence = new List<EvidenceSO>();
        foreach (string evidence in _save.Evidence)
        {
            foreach (EvidenceSO evidenceSo in allEvidence)
            {
                if (evidenceSo.name == evidence)
                {
                    Globals.Evidence.Add(evidenceSo);
                    break;
                }
            }
        }
        
        Globals.Profiles = new List<EvidenceSO>();
        foreach (string evidence in _save.Profiles)
        {
            foreach (EvidenceSO evidenceSo in allEvidence)
            {
                if (evidenceSo.name == evidence)
                {
                    Globals.Profiles.Add(evidenceSo);
                    break;
                }
            }
        }
        
        Globals.UsedDialogue = new List<DialogueSO>();
        foreach (string evidence in _save.UsedDialogue)
        {
            foreach (DialogueSO evidenceSo in allDialogue)
            {
                if (evidenceSo.name == evidence)
                {
                    Globals.UsedDialogue.Add(evidenceSo);
                    break;
                }
            }
        }
        
        Globals.UsedTalks = new List<TalkSO>();
        foreach (string evidence in _save.UsedTalks)
        {
            foreach (TalkSO evidenceSo in allTalks)
            {
                if (evidenceSo.name == evidence)
                {
                    Globals.UsedTalks.Add(evidenceSo);
                    break;
                }
            }
        }
        
        Globals.StoryFlags = _save.StoryFlags;
        
        Globals.MusicManager.Stop();
        Globals.MusicManager.Play(_save.SongPlaying);
        DialogueSO dialogue = null;

        foreach (DialogueSO dialogueSo in allDialogue)
        {
            if (dialogueSo.name == _save.Dialogue)
            {
                dialogue = dialogueSo;
                break;
            }
        }
        
        DiscordController.gameName = Globals.GameName;
        DiscordController.phaseName = $"{Globals.CaseName} ({Globals.PhaseName})";

        TitleScreenMenu.FadeIn = true;
        Globals.DialogueManager.StartText(dialogue, quickEnd:true, startingLine:_save.DialogueIndex);
    }

    public SaveIconData GenerateSaveIcon(int saveNum)
    {
        if (!File.Exists(Globals.SaveDataPath + $"/Saves/FILE_{saveNum}.law")) return null;
        
        string jason = File.ReadAllText(Globals.SaveDataPath + $"/Saves/FILE_{saveNum}.law");
        //_save = JsonUtility.FromJson<SaveDataFormatter>(Globals.DecryptString(jason, "phoenixw"));
        SaveDataFormatter data = JsonUtility.FromJson<SaveDataFormatter>(jason);
        
        SaveIconData icon = new SaveIconData
        {
            SaveNum = saveNum + 1,
            GameName = data.GameName,
            CaseName = data.CaseName,
            CaseNum = data.CaseNum,
            PhaseName = data.PhaseName,
            SaveDate = data.SaveDate,
            PlayTime = data.PlayTime,
            IconColor = data.GameColor
        };

        return icon;
    }

    public void SaveSettingsIntoJson()
    {
        _settings = new SettingsDataFormatter();

        _settings.SaveFile = Globals.SaveFile;
        _settings.Soundtrack = Globals.Soundtrack;
        _settings.UI = Globals.UI;
        _settings.TextBoxTransparency = Globals.TextboxTransparency;
        _settings.MusicVolume = Globals.MusicVolume;
        _settings.SoundVolume = Globals.SoundVolume;
        _settings.ScreenShake = Globals.ScreenShake;
        _settings.FullScreen = Globals.FullScreen;
        _settings.ScreenWidth = Globals.ScreenWidth;
        _settings.ScreenHeight = Globals.ScreenHeight;

        string jason = JsonUtility.ToJson(_settings, true);
        
        if (!Directory.Exists(Globals.SaveDataPath))
        {
            Directory.CreateDirectory(Globals.SaveDataPath);
        }
        
        //File.WriteAllText(Globals.SaveDataPath + "/OPTIONS.law", Globals.EncryptString(jason, "vonkarma"));
        File.WriteAllText(Globals.SaveDataPath + "/OPTIONS.law", jason);
    }

    public void LoadSettingsFromJson()
    {
        if (!File.Exists(Globals.SaveDataPath + "/OPTIONS.law"))
        {
            SaveSettingsIntoJson();
        }
        
        string jason = File.ReadAllText(Globals.SaveDataPath + $"/OPTIONS.law");
        //_settings = JsonUtility.FromJson<SettingsDataFormatter>(Globals.DecryptString(jason, "vonkarma"));
        _settings = JsonUtility.FromJson<SettingsDataFormatter>(jason);

        Globals.SaveFile = _settings.SaveFile;
        Globals.Soundtrack = _settings.Soundtrack;
        Globals.UI = _settings.UI;
        Globals.TextboxTransparency = _settings.TextBoxTransparency;
        Globals.ScreenShake = _settings.ScreenShake;
        Globals.FullScreen = _settings.FullScreen;
        Globals.ScreenWidth = _settings.ScreenWidth;
        Globals.ScreenHeight = _settings.ScreenHeight;
        Globals.MusicVolume = _settings.MusicVolume;
        Globals.SoundVolume = _settings.SoundVolume;
        
        Screen.SetResolution(_settings.ScreenWidth, _settings.ScreenHeight, _settings.FullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        _mixer.SetFloat("Music Volume", Mathf.Log10(Mathf.Max(Globals.MusicVolume / 4f, 0.0001f)) * 20);
        _mixer.SetFloat("Sound Volume", Mathf.Log10(Mathf.Max(Globals.SoundVolume / 4f, 0.0001f)) * 20);

        if (Globals.MusicManager.GetMusicPlaying() != null)
        {
            string songName = Globals.MusicManager.GetMusicPlaying().fileName;
            
            foreach (SoundtrackTypes sound in Enum.GetValues(typeof(SoundtrackTypes)))
            {
                songName = songName.Replace("_" + sound, "");
            }
            
            Globals.MusicManager.Stop();
            Globals.MusicManager.Play(songName);
        }
        
        if (Globals.DialogueManager != null)
            if (Globals.DialogueManager._tempBox != null) 
                Globals.DialogueManager._tempBox.SetTextBoxTransparency();
    }

    public void SaveCompletedCasesIntoJson()
    {
        if (!Directory.Exists(Globals.SaveDataPath))
        {
            Directory.CreateDirectory(Globals.SaveDataPath);
        }

        UnlockedCases = new UnlockedCasesformatter();
        UnlockedCases.Authors = Globals.UnlockedCases;
        string jason = JsonUtility.ToJson(UnlockedCases, true);
        File.WriteAllText(Globals.SaveDataPath + "/COMPLETION.law", jason);
    }

    public void LoadCompletedCasesFromJson()
    {
        if (!File.Exists(Globals.SaveDataPath + "/COMPLETION.law"))
        {
            return;
        }
        
        string jason = File.ReadAllText(Globals.SaveDataPath + $"/COMPLETION.law");
        Globals.UnlockedCases = JsonUtility.FromJson<UnlockedCasesformatter>(jason).Authors;
    }
    
    public string RemoveSpecialCharacters(string str) {
        StringBuilder sb = new StringBuilder();
        foreach (char c in str) {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '-' || c == ' ' || c == ',') {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}

[Serializable]
public class SaveDataFormatter
{
    public double PlayTime;
    public string SaveDate;
    public string SongPlaying;
    public int HP;
    public string GameName;
    public string GameAuthor;
    public int CaseNum;
    public string CaseName;
    public string PhaseName;
    public Color GameColor;
    public string Dialogue;
    public int DialogueIndex;
    public List<string> Evidence = new List<string>();
    public List<string> Profiles = new List<string>();
    public List<string> UsedDialogue = new List<string>();
    public List<string> UsedTalks = new List<string>();
    public List<string> StoryFlags = new List<string>();
}

[Serializable]
public class SettingsDataFormatter
{
    public int SaveFile;
    public SoundtrackTypes Soundtrack;
    public UITypes UI;
    public TextBoxTransparency TextBoxTransparency;
    public int MusicVolume;
    public int SoundVolume;
    public bool ScreenShake;
    public bool FullScreen;
    public int ScreenWidth = 1280;
    public int ScreenHeight = 720;
}

[Serializable]
public class UnlockedCasesformatter
{
    public List<AuthorGamePair> Authors;
}

[Serializable]
public class AuthorGamePair
{
    public string AuthorName;
    public List<GameCasesPair> Games;
}

[Serializable]
public class GameCasesPair
{
    public string GameName;
    public List<CaseCompletionPair> Cases;
}

[Serializable]
public class CaseCompletionPair
{
    public string Case;
    public bool Unlocked;
}
