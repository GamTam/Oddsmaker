using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue")]
public class DialogueDavid : ScriptableObject
{
    public List<DialogueLine> lines; // A list of all the lines of dialogue in the conversation

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 4)] public string dialogueText; // The dialogue text
        public bool hit = false; // Shake the screen while dialogue
        public bool shaking = false; // Shake the screen while dialogue
        public bool changeMusicBefore; // The music clip to play when this line of dialogue is displayed
        public bool changeMusicAfter; // The music clip to play when this line of dialogue is displayed
        public bool mute; // The music clip will mute after the line is displayed
        public int musicIndex;
        public float textSpeed; // The speed at which the text is displayed (in seconds per character)
        public float pauseDuration = 0.5f; // Pause duration
        public float timeBeforeText; // The time in seconds before the text is displayed
        public float waitTimeToAdvance;
        public AudioClip soundEffect;
        public Color textColor = Color.white; // The color of the dialogue text
    }
}
