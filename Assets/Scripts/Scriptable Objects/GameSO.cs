 using System;
 using UnityEngine;

 [CreateAssetMenu(fileName = "New Game", menuName = "Ace Attorney/Game")]
 [Serializable]
 public class GameSO : ScriptableObject
 {
     public string GameName;
     public string GameAuthor;
     public Color GameColor;
     public Sprite Logo;
     public CaseLogoPair[] Cases;
 }

 [Serializable]
 public struct CaseLogoPair
 {
     public bool LockedAtStart;
     public string CaseName;
     public Sprite CaseArt;
     public Sprite CaseNameArt;
     public DialogueSO StartingDialogue;
 }