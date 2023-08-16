using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SavePromptManager : MonoBehaviour
{
    [SerializeField] private GameObject _wait;
    [SerializeField] private GameObject _confirm;
    [SerializeField] private GameObject _saveMenu;
    [SerializeField] private SpriteRenderer _fade;
    
    private PlayerInput _playerInput;

    void Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
    }
    
    void OnEnable()
    {
        _wait.SetActive(true);
        _confirm.SetActive(false);
        Globals.SoundManager.PlayWithVersions("checkpoint");
        StartCoroutine(WaitForSaveMenu());
    }

    private IEnumerator WaitForSaveMenu()
    {
        yield return new WaitForSeconds(1f);
        _wait.SetActive(false);
        
        yield return new WaitForSeconds(0.5f);
        _confirm.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_confirm.GetComponentInChildren<Button>().gameObject);
        _playerInput.SwitchCurrentActionMap("Menu");
        
        Vector3 position = _fade.transform.position;
        position = new Vector3(position.x, position.y, (int) BGFadePos.Everything);
        _fade.transform.position = position;
        _fade.GetComponent<SpriteObjectsSortingOrder>().UpdateSortingOrders();
        
        Color startColor = Color.black;
        Color endColor = Color.clear;
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime;
            _fade.color = Color.Lerp(startColor, endColor, time / 1);
            if (Math.Abs(_fade.color.a - endColor.a) < 0.0001) break;
            yield return null;
        }

        _fade.color = endColor;
    }

    public void NextScreen(bool save)
    {
        StartCoroutine(GoToNextScreen(save));
    }

    private IEnumerator GoToNextScreen(bool save)
    {
        foreach (Button button in _confirm.GetComponentsInChildren<Button>())
        {
            if (EventSystem.current.currentSelectedGameObject == button.gameObject) continue;
            button.animator.Play("Fade Out");
        }

        yield return new WaitForSeconds(0.25f);
        
        Color startColor = Color.clear;
        Color endColor = Color.black;
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime;
            _fade.color = Color.Lerp(startColor, endColor, time / 1);
            if (Math.Abs(_fade.color.a - endColor.a) < 0.0001) break;
            yield return null;
        }

        _fade.color = endColor;

        yield return new WaitForSeconds(0.25f);
        _fade.color = Color.clear;

        Globals.UsedDialogue = new List<DialogueSO>();
        Globals.UsedTalks = new List<TalkSO>();

        if (save)
        {
            GameObject obj = Instantiate(_saveMenu);
            obj.SetActive(true);
            SaveScreenController saveMan = obj.GetComponent<SaveScreenController>();
            saveMan._startDialogueOnLeave = true;
            saveMan._saveMode = true;
        }
        else
        {
            Globals.DialogueManager.StartText(Globals.DialogueManager._dialogue);
        }
        
        gameObject.SetActive(false);
    }
}
