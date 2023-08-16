using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Music {

    public string name;
    
    public float volume = 1;
    public float pitch = 1;

    public AudioSource source;
    public float loopStart;
    public float loopEnd;
}
