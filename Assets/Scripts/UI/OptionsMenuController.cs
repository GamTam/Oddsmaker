using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private SaveData _saveData;
    [SerializeField] private GameObject loadSave;
    [SerializeField] private GameObject _initialSelectedObject;
    [SerializeField] private GameObject _initialSelectedRow;
    
    private PlayerInput _playerInput;
    private InputAction _back;
    
    void Awake()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _back = _playerInput.actions["Menu/Cancel"];
    }

    private void OnEnable()
    {
        Globals.InOptions = true;
        EventSystem.current.SetSelectedGameObject(_initialSelectedObject);
    }

    void Update()
    {
        if (_back.triggered)
        {
            Globals.InOptions = false;
            Globals.SoundManager.Play("back");
            _saveData.SaveSettingsIntoJson();
            transform.parent.gameObject.SetActive(false);
        }
    }

    public void OpenSaveMenu(bool load)
    {
        StartCoroutine(SaveMenu(load));
    }

    private IEnumerator SaveMenu(bool load)
    {
        yield return new WaitForSeconds(0.16f);
        GameObject obj = Instantiate(loadSave);
        SaveScreenController save = obj.GetComponent<SaveScreenController>();
        save.PrevMenu = transform.parent.gameObject;
        save._saveMode = !load;
        obj.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }
}
