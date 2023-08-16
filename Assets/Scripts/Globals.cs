using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = System.Random;

public static class Globals
{
    public static MusicManager MusicManager;
    public static SoundManager SoundManager;

    public static PlayerController Player;
    public static Vector3 PlayerPos = new Vector3();
    public static PlayerInput Input;
    public static EventSystem EventSystem;

    public static bool InBattle;

    public static int SaveFile = 0;
    public static List<sItem> Items = new List<sItem>();
    public static PlayerStats[] PlayerStatsList = new PlayerStats[4];

    public static double PlayTime;
    public static PlayerDir PlayerDir = PlayerDir._d;

    public static List<string> PlayedCutscenes = new List<string>();
    public static List<string> OpenedChests = new List<string>();

    public static bool BeginSceneLoad;
    public static GameState GameState = GameState.Play;

    public static bool WarpFlag;

    public static Dictionary<string, ArrayList> LoadTSV(string file) {
        
        Dictionary<string, ArrayList> dictionary = new Dictionary<string, ArrayList>();
        ArrayList list = new ArrayList();

        using (var reader = new StreamReader(Application.dataPath + "/files/" + file + ".tsv")) {
            while (!reader.EndOfStream)
            {
                list = new ArrayList();
                var line = reader.ReadLine();
                if (line == null) continue;
                string[] values = line.Split('	');
                for (int i=1; i < values.Length; i++) {
                    list.Add(values[i]);
                }
                if (values[0] != "") dictionary.Add(values[0], list);
            }
        }

        return dictionary;
    }
    
    public static void ListSwap<T>(IList<T> list, int indexA, int indexB)
    {
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
    }

    public static int DamageFormula(int atk, int def, out bool crit, int luck = 500)
    {
        int threshold = (int) ((luck / 400f) * 100f);
        int chance = GetRandomNumber(0, 101);

        crit = chance < threshold;
        
        return Mathf.Max(Mathf.RoundToInt((float) ((atk * 4 - def * 2) * GetRandomNumber(0.7f, 1.2f))), 1);
    }
    
    public static bool IsAnimationPlaying(Animator anim, string stateName, int animLayer=0)
    {
        if (anim.GetCurrentAnimatorStateInfo(animLayer).IsName(stateName) &&
            anim.GetCurrentAnimatorStateInfo(animLayer).normalizedTime < 1.0f)
            return true;
        
        return false;
    }
    
    public static int Mod(int x, int m) {
        int r = x%m;
        return r<0 ? r+m : r;
    }
    
    public static String NumberToChar(int num, bool capital) {
        Char c = (Char)((capital ? 65 : 97) + (num - 1));

        return c.ToString();
    }
    
    public static int GetRandomNumber(int minimum, int maximum)
    { 
        Random random = new Random();
        return random.Next(minimum, maximum);
    }
    
    public static double GetRandomNumber(double minimum, double maximum)
    { 
        Random random = new Random();
        return random.NextDouble() * (maximum - minimum) + minimum;
    }
    
    public static string RemoveRichText(string str)
    {
     
        str = RemoveRichTextDynamicTag(str, "color");
     
        str = RemoveRichTextTag(str, "b");
        str = RemoveRichTextTag(str, "i");
        
        str = RemoveRichTextDynamicTag(str, "align");
        str = RemoveRichTextDynamicTag(str, "size");
        str = RemoveRichTextDynamicTag(str, "cspace");
        str = RemoveRichTextDynamicTag(str, "font");
        str = RemoveRichTextDynamicTag(str, "indent");
        str = RemoveRichTextDynamicTag(str, "line-height");
        str = RemoveRichTextDynamicTag(str, "line-indent");
        str = RemoveRichTextDynamicTag(str, "link");
        str = RemoveRichTextDynamicTag(str, "margin");
        str = RemoveRichTextDynamicTag(str, "margin-left");
        str = RemoveRichTextDynamicTag(str, "margin-right");
        str = RemoveRichTextDynamicTag(str, "mark");
        str = RemoveRichTextDynamicTag(str, "mspace");
        str = RemoveRichTextDynamicTag(str, "noparse");
        str = RemoveRichTextDynamicTag(str, "nobr");
        str = RemoveRichTextDynamicTag(str, "page");
        str = RemoveRichTextDynamicTag(str, "pos");
        str = RemoveRichTextDynamicTag(str, "space");
        str = RemoveRichTextDynamicTag(str, "sprite index");
        str = RemoveRichTextDynamicTag(str, "sprite name");
        str = RemoveRichTextDynamicTag(str, "sprite");
        str = RemoveRichTextDynamicTag(str, "style");
        str = RemoveRichTextDynamicTag(str, "voffset");
        str = RemoveRichTextDynamicTag(str, "width");
     
        str = RemoveRichTextTag(str, "u");
        str = RemoveRichTextTag(str, "s");
        str = RemoveRichTextTag(str, "sup");
        str = RemoveRichTextTag(str, "sub");
        str = RemoveRichTextTag(str, "allcaps");
        str = RemoveRichTextTag(str, "smallcaps");
        str = RemoveRichTextTag(str, "uppercase");
        
        return str;
     
    }
    
    private static string RemoveRichTextDynamicTag (string str, string tag)
    {
        int index = -1;
        while (true)
        {
            index = str.IndexOf($"<{tag}=");
            //Debug.Log($"{{{index}}} - <noparse>{input}");
            if (index != -1)
            {
                int endIndex = str.Substring(index, str.Length - index).IndexOf('>');
                if (endIndex > 0)
                    str = str.Remove(index, endIndex + 1);
                continue;
            }
            str = RemoveRichTextTag(str, tag, false);
            return str;
        }
    }
    private static string RemoveRichTextTag (string str, string tag, bool isStart = true)
    {
        while (true)
        {
            int index = str.IndexOf(isStart ? $"<{tag}>" : $"</{tag}>");
            if (index != -1)
            {
                str = str.Remove(index, 2 + tag.Length + (!isStart).GetHashCode());
                continue;
            }
            if (isStart)
                str = RemoveRichTextTag(str, tag, false);
            return str;
        }
    }
    
    public static void UnloadAllScenesExcept(string sceneName) {
        int c = SceneManager.sceneCount;
        for (int i = 0; i < c; i++) {
            Scene scene = SceneManager.GetSceneAt (i);
            if (scene.name != sceneName) {
                SceneManager.UnloadSceneAsync (scene);
            }
        }
    }

    public static IEnumerator LoadScene(string scene, bool additive) {
        yield return null;

        AsyncOperation asyncOperation;
        if (additive) asyncOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        else asyncOperation = SceneManager.LoadSceneAsync(scene);

        asyncOperation.completed += (AsyncOperation o) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
        };
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                if (BeginSceneLoad)
                {
                    asyncOperation.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        BeginSceneLoad = false;
    }

    public static int LevelUpLut(int input)
    {
        switch (input)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return 1;
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
                return 2;
            case 15:
                return 3;
        }

        return 4;
    }
    
    public static string EncryptString(string plainText, string password)
    {
        try
        {
            string privateKey = "hgfedcba";
            byte[] privateKeyByte = Encoding.UTF8.GetBytes(privateKey);
            byte[] keyByte = Encoding.UTF8.GetBytes(password);
            byte[] inputtextbyteArray = Encoding.UTF8.GetBytes(plainText);
            using (DESCryptoServiceProvider dsp = new DESCryptoServiceProvider())
            {
                var memstr = new MemoryStream();
                var crystr = new CryptoStream(memstr, dsp.CreateEncryptor(keyByte, privateKeyByte), CryptoStreamMode.Write);
                crystr.Write(inputtextbyteArray, 0, inputtextbyteArray.Length);
                crystr.FlushFinalBlock();
                return Convert.ToBase64String(memstr.ToArray());
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    
    public static string DecryptString(string encrypted, string password)
    {
        try
        {
            string privateKey = "hgfedcba";
            byte[] privateKeyByte  = Encoding.UTF8.GetBytes(privateKey);
            byte[] keyByte = Encoding.UTF8.GetBytes(password);
            byte[] inputtextbyteArray = Convert.FromBase64String(encrypted.Replace(" ", "+"));
            using (DESCryptoServiceProvider dEsp = new DESCryptoServiceProvider())
            {
                var memstr = new MemoryStream();
                var crystr = new CryptoStream(memstr, dEsp.CreateDecryptor(keyByte, privateKeyByte), CryptoStreamMode.Write);
                crystr.Write(inputtextbyteArray, 0, inputtextbyteArray.Length);
                crystr.FlushFinalBlock();
                return Encoding.UTF8.GetString(memstr.ToArray());
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static void TurnOffAllWarpFlags()
    {
        WarpFlag = false;
    }

    public static void TurnOnWarpFlag()
    {
        WarpFlag = true;
    }
}

[Serializable]
public struct sItem
{
    public AttackSO Item;
    public int Count;
}

[Serializable]
public class PlayerStats
{
    public string Name;
    public int Level;
    public int HP;
    public int MaxHP;
    public int MP;
    public int MaxMP;
    public int Pow;
    public int Def;
    public int Luck;
    public int Speed;
    public int EXP;
    public int MinRequirement;
    public List<AttackSO> Attacks;
    
    public AssetReferenceT<Sprite> PFPRef;
    public AssetReferenceT<Sprite> DeadPFPRef;

    [Space]
    public Sprite PFP;
    public Sprite DeadPFP;

    public int ExpForNextLevel => Mathf.RoundToInt((float) (4 * Math.Pow(Level, 4)) / 5) + MinRequirement;
}

public enum GameState
{
    Play,
    Cutscene,
    Battle
}