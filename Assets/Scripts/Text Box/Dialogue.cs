using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

[Serializable]
public class TBLine
{
    [SerializeField] public Character[] _charsOnScreen = new Character[0];
    [SerializeField] private DialogueSoundTypes _blipSound = DialogueSoundTypes.Default;
    [SerializeField] [ShowIf("_blipSound", DialogueSoundTypes.Custom)] [AllowNesting] private string _customSound = String.Empty;
    [SerializeField] [ShowIf("_blipSound", DialogueSoundTypes.Custom)] [MinMaxSlider(-100f, 100f)] [AllowNesting] public Vector2 _pitchVariationRange = Vector2.zero;
    [SerializeField] [ShowIf("_blipSound", DialogueSoundTypes.Custom)] [AllowNesting] public float _blipFrequency = 0;
    [SerializeField] [Dropdown("CharNamesList")] [AllowNesting] public string _charTalking = String.Empty;
    [SerializeField] public AssetReferenceT<Sprite> _background = new AssetReferenceT<Sprite>("");
    [SerializeField] [ShowIf("WideBackground")] [AllowNesting] public BackgroundPositions HorizontalScrollStartPercentage = BackgroundPositions.Center;
    [SerializeField] [ShowIf("WideBackground")] [AllowNesting] public BackgroundPositions HorizontalScrollEndPercentage = BackgroundPositions.Center;
    [SerializeField] [ShowIf("TallBackground")] [AllowNesting] public BackgroundPositionsVertical VerticalScrollStartPercentage = BackgroundPositionsVertical.Center;
    [SerializeField] [ShowIf("TallBackground")] [AllowNesting] public BackgroundPositionsVertical VerticalScrollEndPercentage = BackgroundPositionsVertical.Center;
    [SerializeField] [ShowIf(EConditionOperator.Or, "WideBackground", "TallBackground")] [AllowNesting] public ScrollSpeedOptions ScrollSpeed = ScrollSpeedOptions.Default;
    [SerializeField] [ShowIf("ScrollSpeed", ScrollSpeedOptions.Custom)] [AllowNesting] public float ScrollTime = 0.5f;
    [SerializeField] [ShowIf(EConditionOperator.Or, "WideBackground", "TallBackground")] [Label("Don't Wait For Scroll")] [AllowNesting] public bool DontWaitForScrolling = false;
    [SerializeField] [ShowIf(EConditionOperator.Or, "WideBackground", "TallBackground")] [AllowNesting] public bool LinearScrolling = false;
    [SerializeField] [ShowIf(EConditionOperator.Or, "WideBackground", "TallBackground")] [AllowNesting] public bool HidePreviousCharWhileSwapping = false;
    [SerializeField] [ShowIf("HasForeground")]  [AllowNesting] public bool HideForeground = false;
    [Space] [SerializeField] [TextArea(3, 4)] private string _dialogue = String.Empty;
    [SerializeField] public Metadata _extras = new Metadata();
    [SerializeField] private StateChange _stateChange = new StateChange();

    public TBLine()
    {
        ScrollTime = 0.5f;
    }
    
    List<string> CharNamesList()
    {
        List<string> list = new List<string>
        {
            "No Characters On Screen"
        };
        
        if (_charsOnScreen.Length <= 0) return list;
        
        list = new List<string>();
        
        foreach(Character ac in _charsOnScreen)
        {
            if (ac.CharOnScreen == null) continue;
            
            int count = list.Where(temp => temp.Equals(ac.CharOnScreen.name))
                .Select(temp => temp)
                .Count();

            string str;
            
            if (count > 0) str = ac.CharOnScreen.name + " (" + count + ")";
            else str = ac.CharOnScreen.name;
            
            list.Add(str);
        }

        list.Add("Keep Previous Character");

        if (list.Count == 1)
        {
            list = new List<string>
            {
                "No Characters On Screen"
            };
        }
        return list;
    }

    public bool HasChar => _charsOnScreen.Length > 0;
    public bool WideBackground()
    {
        if (string.IsNullOrEmpty(_background.AssetGUID)) return false;
        Sprite bg = Addressables.LoadAssetAsync<Sprite>(_background).WaitForCompletion();
        if (bg == null) return false;
        
        string path = "Assets/Sprites/Backgrounds/" + bg.name + ".png";
        Sprite sprite = Addressables.LoadAssetAsync<Sprite>(path).WaitForCompletion();
        
        return sprite.texture.width > 1920;
    }
    
    public bool TallBackground()
    {
        if (string.IsNullOrEmpty(_background.AssetGUID)) return false;
        Sprite bg = Addressables.LoadAssetAsync<Sprite>(_background).WaitForCompletion();
        if (bg == null) return false;
        
        string path = "Assets/Sprites/Backgrounds/" + bg.name + ".png";
        Sprite sprite = Addressables.LoadAssetAsync<Sprite>(path).WaitForCompletion();
        
        return sprite.texture.height > 1080;
    }

    public bool HasForeground()
    {
        if (string.IsNullOrEmpty(_background.AssetGUID)) return true;
        Sprite bg = Addressables.LoadAssetAsync<Sprite>(_background).WaitForCompletion();

        if (bg == null) return true;
        
        Sprite sprite = null;

        AsyncOperationHandle<Sprite> spriteHandler;
        try
        {
            string path = "Assets/Sprites/Foregrounds/" +
                          bg.name + "_fg.png";
            sprite = Addressables.LoadAssetAsync<Sprite>(path).WaitForCompletion();
        } catch (InvalidKeyException) {}

        if (!string.IsNullOrEmpty(CustomForeground.AssetGUID))
        {
            spriteHandler = Addressables.LoadAssetAsync<Sprite>(CustomForeground);
            sprite = spriteHandler.WaitForCompletion();
        }

        return sprite != null;
    }

    public void SetDefaultValues()
    {
        _extras.Thinking = false;
        _extras.FlipBackground = false;
        _extras.AutoEnd = false;
        _extras.StopMusic = false;
        _extras.AddToCourtRecord = false;
        _extras.HideNameTag = false;
        _extras.HideOptions = false;
        _extras.ShowTextBox = false;
        _extras.AddToPrevious = false;
        _extras.DontAddSpaceToPrevious = false;
        _extras.CustomName = string.Empty;
        _extras.CustomForeground = new AssetReferenceT<Sprite>("");
        _extras.CustomTextBox = null;
        _extras.Interjection = Interjection.NA;
        _extras.CustomInterjection = "";
        _extras.Align = TextAlignOptions.left;
        _extras.ShowHealthBar = false;
        _extras.HealthBarChangeAmount = 0;
        _extras.HealthBarWarnAmount = 0;
        _extras.ShowPsycheLock = false;
        _extras.LockCount = 0;
        _extras.BreakLock = false;
        _extras.BreakChain = false;
        _extras.ObjectsToSpawn = new List<GameObject>();
        _extras.ScreenAnimation = null;
        _extras.StartTextAfterAnimation = false;
        _extras.CannotAdvanceUntilAnimationDone = false;
        _extras.FlipScreenAnimation = false;
        _extras.LeftEvidence = null;
        _extras.RightEvidence = null;
        _extras.SkipIconAnimation = false;
        _extras.WaitForIconAnimation = false;
        _extras.Video = null;
        
        _extras.FadeOptions.CharFade = FadeTypes.Auto;
        _extras.FadeOptions.CharFadeSpeed = 0.2f;
        _extras.FadeOptions.BackgroundFadeType = BGFadeTypes.None;
        _extras.FadeOptions.BackgroundFadePos = BGFadePos.BackgroundGone;
        _extras.FadeOptions.LengthInSeconds = 0;
        _extras.FadeOptions.Color = Color.black;
        _extras.FadeOptions.IncludeForeground = false;
        _extras.FadeOptions.InvertColors = false;
        _extras.FadeOptions.Grayscale = false;
    }

    public string Name => _extras.CustomName == "" && HasChar ? _charTalking : _extras.CustomName;
    public Character[] Chars => _charsOnScreen;
    public bool Thinking => _extras.Thinking;
    public bool AutoEnd => _extras.AutoEnd;
    public bool FlipBackground => _extras.FlipBackground;
    public bool AddToCourtRecord => _extras.AddToCourtRecord;
    public DialogueSoundTypes BlipSound => _blipSound;
    public Interjection Interjection => _extras.Interjection;
    public string CustomInterjection => _extras.CustomInterjection;
    public TextAlignOptions Align => _extras.Align;
    public StateChange StateChange => _stateChange;
    public FadeTypes FadeType => _extras.FadeOptions.CharFade;
    public bool HideOptions => _extras.HideOptions;
    public bool StopMusic => _extras.StopMusic;
    public string Dialogue => _dialogue;
    public AssetReferenceT<Sprite> Background => _background;
    public bool HideNameTag => _extras.HideNameTag;
    public BackgroundFade FadeDetails => _extras.FadeOptions;
    public List<GameObject> ObjectsToSpawn => _extras.ObjectsToSpawn;
    public AssetReferenceT<Sprite> CustomForeground => _extras.CustomForeground;
    public GameObject CustomTextBox => _extras.CustomTextBox;
    public bool ShowHealthBar => _extras.ShowHealthBar;
    public int HealthBarWarnAmount => _extras.HealthBarWarnAmount;
    public int HealthBarChangeAmount => _extras.HealthBarChangeAmount;
    public GameObject ScreenAnimation => _extras.ScreenAnimation;
    public bool StartTextAfterScreenAnimation => _extras.StartTextAfterAnimation;
    public bool CannotAdvanceUntilAnimationDone => _extras.CannotAdvanceUntilAnimationDone;
    public bool AddToPrevious => _extras.AddToPrevious;
    public bool DontAddSpaceToPrevious => _extras.DontAddSpaceToPrevious;
    public Sprite LeftEvidence => _extras.LeftEvidence;
    public Sprite RightEvidence => _extras.RightEvidence;
    public bool SkipIconAnimation => _extras.SkipIconAnimation;
    public bool WaitForIconAnimation => _extras.WaitForIconAnimation;
    public bool ShowTextBox => _extras.ShowTextBox;
    public VideoClip Video => _extras.Video;
    public string CustomBlipSound => _customSound;


    public void SetDialogue(string str) { _dialogue = str; }
    public void SetBlipSound(DialogueSoundTypes sound) { _blipSound = sound; }
}

[Serializable]
public class Character : ICloneable
{
    [ShowAssetPreview(128, 128)] public GameObject CharOnScreen;
    [Dropdown("AnimationNameList")] public string AnimPlaying;
    public Vector2 Offset;
    public ScrollSpeedOptions MoveSpeed;
    [ShowIf("MoveSpeed", ScrollSpeedOptions.Custom)] [AllowNesting] public float MoveTime = 0.5f;
    public bool SkipOpeningAnimation;
    public bool DontWaitForOpeningAnimation;
    public bool DontWaitForOpeningAnimationToEnd;
    public bool InFrontOfForeground;
    public bool Flip;

    [HideInInspector] public float zPos;
    
    List<string> AnimationNameList()
    {
        List<string> list = new List<string>
        {
            "No Animations Found"
        };

        if (!CharOnScreen) return list;

        Animator anim = null;
        try
        {
            anim = CharOnScreen.GetComponent<Animator>();
        } catch {}

        if (anim == null) return list;

        list = new List<string>();

        foreach(AnimationClip ac in anim.runtimeAnimatorController.animationClips)
        {
            if (ac.name == "Blink") continue;
            string name = ac.name.ToLower().Replace("_mouth_idle", "").Replace("_mouth_talk", "").Replace("_mouth_opening", "").Replace("_blink", "");
            name = name.ToLower().Replace("_idle", "").Replace("_talk", "").Replace("_opening", "");
            list.Add(name);
        }

        list.Sort();
        list.Insert(0, "Keep Previous Animation");

        return list;
    }

    public object Clone()
    {
        return new Character
        {
            CharOnScreen = CharOnScreen,
            AnimPlaying = AnimPlaying,
            Offset = Offset,
            MoveSpeed = MoveSpeed,
            SkipOpeningAnimation = SkipOpeningAnimation,
            DontWaitForOpeningAnimation = DontWaitForOpeningAnimation,
            DontWaitForOpeningAnimationToEnd = DontWaitForOpeningAnimationToEnd,
            InFrontOfForeground = InFrontOfForeground,
            Flip = Flip
        };
    }
}

[Serializable]
public struct StateChange
{
    public EvidenceSO EvidenceToAdd;
    public EvidenceSO EvidenceToRemove;
    public EvidenceSO PersonToAdd;
}

[Serializable]
public struct Metadata
{
    public bool Thinking;
    public bool FlipBackground;
    public bool AutoEnd;
    public bool StopMusic;
    public bool AddToCourtRecord;
    public bool HideNameTag;
    public bool HideOptions;
    public bool ShowTextBox;
    public bool AddToPrevious;
    [ShowIf("AddToPrevious")] [AllowNesting] public bool DontAddSpaceToPrevious;
    [HideIf("HideNameTag")] [AllowNesting] public string CustomName;
    public AssetReferenceT<Sprite> CustomForeground;
    public GameObject CustomTextBox;
    public Interjection Interjection;
    [ShowIf("Interjection", Interjection.Custom)] [AllowNesting] public string CustomInterjection;
    public TextAlignOptions Align;
    public BackgroundFade FadeOptions;
    public bool ShowHealthBar;
    [ShowIf("ShowHealthBar")] [AllowNesting] [Range(-10, 10)] public int HealthBarChangeAmount;
    [ShowIf("ShowHealthBar")] [AllowNesting] [Range(0, 10)] public int HealthBarWarnAmount;
    public bool ShowPsycheLock;
    [ShowIf("ShowPsycheLock")] [AllowNesting] [Range(1, 5)] public int LockCount;
    [ShowIf("ShowPsycheLock")] [AllowNesting] public bool BreakLock;
    [ShowIf("ShowPsycheLock")] [AllowNesting] public bool BreakChain;
    public List<GameObject> ObjectsToSpawn;
    public GameObject ScreenAnimation;
    [ShowIf("HasScreenAnimation")] [AllowNesting] public bool StartTextAfterAnimation;
    [ShowIf("HasScreenAnimation")] [AllowNesting] public bool CannotAdvanceUntilAnimationDone;
    [ShowIf("HasScreenAnimation")] [AllowNesting] public bool FlipScreenAnimation;
    public Sprite LeftEvidence;
    public Sprite RightEvidence;
    [Label("Skip Animation")] [AllowNesting] public bool SkipIconAnimation;
    [Label("Wait For Animation")] [AllowNesting] public bool WaitForIconAnimation;
    [Space] public VideoClip Video;
    
    private bool HasScreenAnimation => ScreenAnimation != null;
}

[Serializable]
public struct BackgroundFade
{
    public FadeTypes CharFade;
    [Tooltip("The time, in seconds, it takes for a character to fade out.\nKeep at 0 for the default value (0.2).")]
    public float CharFadeSpeed;
    public BGFadeTypes BackgroundFadeType;
    [HideIf("BackgroundFadeType", BGFadeTypes.None)] [AllowNesting] public BGFadePos BackgroundFadePos;
    [HideIf("BackgroundFadeType", BGFadeTypes.None)] [AllowNesting] public float LengthInSeconds;
    [HideIf("BackgroundFadeType", BGFadeTypes.None)] [AllowNesting] public Color Color;
    [ShowIf(EConditionOperator.And, "IsBackgroundFade", "IsFading")] [AllowNesting] public bool IncludeForeground;
    public bool InvertColors;
    public bool Grayscale;

    private bool IsBackgroundFade => BackgroundFadePos == BGFadePos.BackgroundGone;
    private bool IsFading => BackgroundFadeType != BGFadeTypes.None;

}

public enum FadeTypes
{
    Auto,
    SkipFade,
    ForceFade
}

public enum BGFadeTypes
{
    None,
    FadeIn,
    FadeOut,
    FadeOutThenIn
}

public enum BGFadePos
{
    BackgroundGone = 10,
    UIStays = -10,
    Everything = -15
}

public enum Interjection
{
    NA,
    Objection,
    HoldIt,
    TakeThat,
    Custom
}

public enum BackgroundPositions
{
    Left=0,
    Center=50,
    Right=100
}

public enum BackgroundPositionsVertical
{
    Bottom=0,
    Center=50,
    Top=100
}

public enum ScrollSpeedOptions
{
    Default,
    Slow,
    Fast,
    Custom
}