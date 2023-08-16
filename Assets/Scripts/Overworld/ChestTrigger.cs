using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChestTrigger : DialogueTrigger
{
    [SerializeField] private sItem[] _rewards;
    [SerializeField] private Sprite _openedSprite;
    private string _ID;
    private Animator _animator;
    private SpriteRenderer _sprite;

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        
        _ID = Globals.EncryptString($"{SceneManager.GetActiveScene().name} {transform.position.x * transform.position.y * transform.position.z}", "Treasure");

        if (Globals.OpenedChests.Contains(_ID))
        {
            _animator.enabled = false;
            _sprite.sprite = _openedSprite;
            enabled = false;
        }
    }

    public override void TriggerDialogue()
    {
        if (Globals.OpenedChests.Contains(_ID))
        {
            enabled = false;
            return;
        }
        
        Globals.OpenedChests.Add(_ID);
        string[] dialogue = new string[_rewards.Length];

        for (int i = 0; i < _rewards.Length; i++)
        {
            dialogue[i] = $"Got <color=red>{_rewards[i].Item.Name}</color>! (x{_rewards[i].Count})";

            bool found = false;
            for (int j = 0; j < Globals.Items.Count; j++)
            {
                sItem item = Globals.Items[j];
                if (item.Item == _rewards[i].Item)
                {
                    item.Count += _rewards[i].Count;
                    found = true;
                    Globals.Items[j] = item;
                    break;
                }
            }

            if (!found)
            {
                Globals.Items.Add(_rewards[i]);
            }
        }
        
        _animator.Play("Chest Open");
        FindObjectOfType<DialogueManager>().StartText(dialogue, gameObject.transform, _spriteRenderer, _skipTextboxCloseAnimation, _forceBottom);
        enabled = false;
    }
}
