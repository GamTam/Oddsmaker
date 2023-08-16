using UnityEngine;

[System.Serializable]
public class Response
{
    [SerializeField] private string _responseText;
    [SerializeField] private DialogueSO _dialogue;

    public string ResponseText => _responseText;
    public DialogueSO DialogueObject => _dialogue;
}
