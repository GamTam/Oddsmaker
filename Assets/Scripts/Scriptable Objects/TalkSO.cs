using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Talk", menuName = "Ace Attorney/Talk Prompt")]
[Serializable]
public class TalkSO : ScriptableObject 
{
    public string Name;
    public DialogueSO DialogueSO;
    public bool Locked;
    public string[] ConditionFlags;
}