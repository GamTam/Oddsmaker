using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLineEditor : MonoBehaviour
{
    [SerializeField] private TMP_InputField _input;
    [SerializeField] [TextArea(3, 10)] [ReadOnly] public string Dialogue;

    public void UpdateText()
    {
        Dialogue = _input.text;
    }

    public void SetText(string str)
    {
        _input.text = str;
        Dialogue = str;
    }
}
