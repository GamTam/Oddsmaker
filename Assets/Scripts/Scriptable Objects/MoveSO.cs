using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Move", menuName = "Ace Attorney/Move Prompt")]
[Serializable]
public class MoveSO : ScriptableObject 
{
    public string Name;
    public Sprite Preview;
    public DialogueSO Scene;
    public bool KnownFromStart;
    public string[] ConditionFlags;
}