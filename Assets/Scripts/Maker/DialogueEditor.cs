using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DialogueEditor : MonoBehaviour
{
    public List<TBLine> _lines;
    [SerializeField] private GameObject _line;
    [SerializeField] private GameObject _listObj;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private Character[] _charsOnScreen;
    [SerializeField] private AssetReferenceT<Sprite> _background;
    [Space] [SerializeField] [ReadOnly] private List<DialogueLineEditor> _lineEditors;

    private void Start()
    {
        foreach (TBLine line in _lines)
        {
            GameObject obj = Instantiate(_line, _listObj.transform);
            obj.transform.SetSiblingIndex(_listObj.transform.childCount - 2);
            obj.GetComponent<DialogueLineEditor>().SetText(line.Dialogue);
            obj.SetActive(true);
            _lineEditors.Add(obj.GetComponent<DialogueLineEditor>());
        }

        Addressables.LoadAssetsAsync<Sprite>("Background", sprite => {Debug.Log(sprite.name);});
    }

    public void AddLine()
    {
        GameObject obj = Instantiate(_line, _listObj.transform);
        obj.transform.SetSiblingIndex(_listObj.transform.childCount - 2);
        obj.SetActive(true);
        _lineEditors.Add(obj.GetComponent<DialogueLineEditor>());
    }

    public void StartDialogue()
    {
        DialogueSO dialogue = ScriptableObject.CreateInstance<DialogueSO>();
        _lines = new List<TBLine>();
        
        foreach (DialogueLineEditor lineEditor in _lineEditors)
        {
            TBLine line = new TBLine();
            line.SetDialogue(lineEditor.Dialogue);
            line.SetBlipSound(DialogueSoundTypes.Default);
            _lines.Add(line);
            line.SetDefaultValues();
            line._charsOnScreen = _charsOnScreen;
            line._background = _background;
        }

        dialogue.dialogueText = _lines.ToArray();

        _dialogueManager.gameObject.SetActive(true);
        gameObject.SetActive(false);
        _dialogueManager.StartText(dialogue);
    }
}
