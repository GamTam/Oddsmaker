using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum DialogueOptions
{
    Normal,
    ToInvestigation,
    ToInvestigationLock,
    CrossExamination,
    ChoicePrompt,
    CourtRecordPrompt,
    CourtRecordPromptLock,
    ImagePrompt,
    ImagePromptLock,
    CanGameOver,
    PhaseStart,
    CaseStart,
    Testimony,
    PsycheLockDamage,
    PhaseEnd,
    CaseEnd
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Ace Attorney/Dialogue")]
[Serializable]
public class DialogueSO : ScriptableObject
{
    public TBLine[] dialogueText = Array.Empty<TBLine>();
    public DialogueSO nextLine;
    public InvestigationMenu.FlagTalkPair[] conditionalNextLine = Array.Empty<InvestigationMenu.FlagTalkPair>();
    
    [Space]
    
    [Tooltip("The story flags that must be true (or false by placing a '!' at the start) in order to skip a chunk of text.")]
    public string[] skipConditions = new string[0];
    
    [Tooltip("The index of the dialogue to jump to if the conditions have been met.\n\nStarts at 0.")]
    public int skipIndex = 0;

    [Tooltip("Tells the game which flag to enable. Depending on which flags have been enabled, you can do different things.")]
    public string StoryFlag = String.Empty;
    [Space]
    [Space]
    public DialogueOptions dialogueType = DialogueOptions.Normal;

    public bool IsInvestigation => dialogueType == DialogueOptions.ToInvestigation;
    public bool IsCrossExamination => dialogueType == DialogueOptions.CrossExamination;
    public bool IsChoicePrompt => dialogueType == DialogueOptions.ChoicePrompt;
    public bool IsCourtRecordPrompt => dialogueType == DialogueOptions.CourtRecordPrompt;
    public bool IsImagePrompt => dialogueType == DialogueOptions.ImagePrompt;
    public bool CanGameOver => dialogueType == DialogueOptions.CanGameOver;
    public bool IsPsycheLockPrompt => dialogueType == DialogueOptions.CourtRecordPromptLock;
    public bool IsCaseStart => dialogueType == DialogueOptions.CaseStart;
    public bool IsPhaseStart => dialogueType == DialogueOptions.PhaseStart;

    [Tooltip("The possible responses that the player can make once all dialogue has run out.\n\nFirst box is the text that will display on the button,\nand the second box is the dialogue that will play.")]
    [ShowIf("dialogueType", DialogueOptions.ChoicePrompt)] public Response[] responses = new Response[0];
    
    [ShowIf(EConditionOperator.Or, "IsCrossExamination", "IsInvestigation", "IsCourtRecordPrompt", "IsPsycheLockPrompt")] [Label("Evidence")] public InvestigationMenu.EvidenceTalkPair[] evidence = new InvestigationMenu.EvidenceTalkPair[0];

    [Tooltip("The game objects that the player is able to interact with using the magnifying glass.\n\nGo to 'Prefabs/Investigation/Interactable Objects' for examples.")]

    [Space]
    [ShowIf("dialogueType", DialogueOptions.CrossExamination)] public DialogueSO prevLine;
    [ShowIf("dialogueType", DialogueOptions.CrossExamination)] public InvestigationMenu.FlagTalkPair[] conditionalPrevLine = new InvestigationMenu.FlagTalkPair[0];
    
    [Space]
    [ShowIf("dialogueType", DialogueOptions.CrossExamination)] public DialogueSO pressSequence;
    [ShowIf("dialogueType", DialogueOptions.CrossExamination)] public InvestigationMenu.FlagTalkPair[]  conditionalPressSequence = new InvestigationMenu.FlagTalkPair[0];

    [Space]
    [ShowIf("dialogueType", DialogueOptions.ToInvestigation)] public List<TalkSO> talkText = new List<TalkSO>();
    [ShowIf("dialogueType", DialogueOptions.ToInvestigation)] public List<MoveSO> moveablePlaces = new List<MoveSO>();
    [ShowIf(EConditionOperator.Or, "IsInvestigation", "IsImagePrompt")] [Label("Interactables")] public GameObject Interactables = null;
    [ShowIf(EConditionOperator.Or, "IsInvestigation", "IsCrossExamination", "IsCourtRecordPrompt", "IsImagePrompt", "IsPsycheLockPrompt")] public DialogueSO wrongPresentSequence;
    [ShowIf("dialogueType", DialogueOptions.ToInvestigation)] public DialogueSO noCluesHere;
    [ShowIf("dialogueType", DialogueOptions.CourtRecordPromptLock)] public DialogueSO GiveUp;

    [ShowIf("dialogueType", DialogueOptions.CanGameOver)]  public DialogueSO GameOverDialogue;

    [Space] public bool GoToTitleScreen;
    [ShowIf("IsCaseStart")] public string CaseName;
    [ShowIf(EConditionOperator.Or, "IsPhaseStart", "IsCaseStart")] public string PhaseName = String.Empty;

    [Space]
    [ShowIf(EConditionOperator.Or, "IsPhaseStart", "IsCaseStart")] [AllowNesting] public List<EvidenceSO> StartingEvidence = new List<EvidenceSO>();
    [ShowIf(EConditionOperator.Or, "IsPhaseStart", "IsCaseStart")] [AllowNesting] public List<EvidenceSO> StartingProfiles = new List<EvidenceSO>();

    public bool HasResponses => dialogueType == DialogueOptions.ChoicePrompt && FindNextLine() == null;
    public bool HasNextLine => FindNextLine() != null;
    public bool HasPressingSequence => FindPress() != null;
    public bool HasPresentPrompt => dialogueType == DialogueOptions.CourtRecordPrompt || dialogueType == DialogueOptions.CourtRecordPromptLock;
    
    [Tooltip("If ticked, the Investigation menu (Examine, Move, Talk, Present) will show up upon completion of the dialogue.")]
    public bool showInvestigationPanel => dialogueType == DialogueOptions.ToInvestigation;

    public InvestigationMenu.EvidenceTalkPair[] ReturnListOfEvidence() {
        return evidence;
    }
    
    public DialogueSO FindNextLine()
    {
        if (conditionalNextLine.Length > 0)
        {
            foreach (InvestigationMenu.FlagTalkPair flag in conditionalNextLine)
            {
                if (Globals.CheckStoryFlags(flag.Flags))
                {
                    return flag.Dialogue;
                }
            }
        }
        
        return nextLine;
    }
    
    public DialogueSO FindPreviousLine()
    {
        if (conditionalPrevLine.Length > 0)
        {
            foreach (InvestigationMenu.FlagTalkPair flag in conditionalPrevLine)
            {
                if (Globals.CheckStoryFlags(flag.Flags))
                {
                    return flag.Dialogue;
                }
            }
        }
        
        return prevLine;
    }
    
    public DialogueSO FindPress()
    {
        if (conditionalPressSequence == null) return pressSequence;
        if (conditionalPressSequence.Length <= 0) return pressSequence;
        
        foreach (InvestigationMenu.FlagTalkPair flag in conditionalPressSequence)
        {
            if (Globals.CheckStoryFlags(flag.Flags))
            {
                return flag.Dialogue;
            }
        }

        return pressSequence;
    }
}