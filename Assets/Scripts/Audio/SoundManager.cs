using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup group;
    [FormerlySerializedAs("allMusic")] [SerializeField] private List<Sound> sounds;
    
    public static SoundManager instance;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Globals.SoundManager != null)
        {
            Destroy(gameObject);
            return;
        }
        Globals.SoundManager = this;

        Dictionary<string, ArrayList> musicDict = new Dictionary<string, ArrayList>();
        musicDict = Globals.LoadTSV("Sound Data");

        int i = 0;
        foreach(KeyValuePair<string, ArrayList> entry in musicDict) {
            if (i != 0) {
                Sound sound = new Sound();
                
                sound.name = entry.Key;

                String path = "Sound/" + sound.name;

                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = Resources.Load(path) as AudioClip;
                sound.source.pitch = sound.pitch;
                sound.source.loop = Convert.ToBoolean(entry.Value[0]);
                sound.source.outputAudioMixerGroup = group;

                sounds.Add(sound);
            }

            i++;
        }
    }

    public void Stop(Sound s) {
        s.source.Stop();
    }

    public Sound Play (string name, float pitch = 1f)
    {
        Sound s = sounds.Find(x => x.name == name);
        if (s == null)
            return null;

        s.source.pitch = pitch;
        s.source.Play();

        return s;
    }
}
