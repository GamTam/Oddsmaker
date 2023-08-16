using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject _continueButton;
    private ChangeScenes _sceneChanger;
    
    void Start()
    {
        if (!File.Exists(Application.persistentDataPath + "/FILE_0.oddsmaker")) _continueButton.SetActive(false);
        
        _sceneChanger = GetComponent<ChangeScenes>();
        Globals.MusicManager.Play("Title");
    }

    public void StartClick()
    {
        StartCoroutine(_sceneChanger.FadeOut("Desert"));
        Globals.Input.SwitchCurrentActionMap("Null");
        EventSystem.current.SetSelectedGameObject(null);
    }
}
