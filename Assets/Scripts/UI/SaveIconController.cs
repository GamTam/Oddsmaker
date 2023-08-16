using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveIconController : MonoBehaviour
{
    [SerializeField] private GameObject NoneText;
    [SerializeField] private GameObject SomeText;
    [Space] 
    [SerializeField] private Sprite[] _saveBackgrounds;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _saveNumText;
    [SerializeField] private TMP_Text _gameTitleText;
    [SerializeField] private TMP_Text _dateTimeText;
    [SerializeField] private TMP_Text _caseNameText;
    [SerializeField] private TMP_Text _phaseNameText;

    public void UpdateIcon(SaveIconData data, int backupNum = 0, bool saveMode = false)
    {
        if (data == null)
        {
            SomeText.SetActive(false);
            NoneText.SetActive(true);
            _icon.enabled = false;
            _saveNumText.text = backupNum.ToString();
            GetComponent<Image>().sprite = _saveBackgrounds[0];
            NoneText.GetComponent<Image>().enabled = !saveMode;
            
            return;
        }
        
        SomeText.SetActive(true);
        NoneText.SetActive(false);
        GetComponent<Image>().sprite = _saveBackgrounds[1];
        _icon.enabled = true;
        data.IconColor.a = 1;
        _icon.color = data.IconColor;
        _saveNumText.text = data.SaveNum.ToString();
        _gameTitleText.text = data.GameName;
        _dateTimeText.text = data.SaveDate;
        TimeSpan time = TimeSpan.FromSeconds(data.PlayTime);
        //_dateTimeText.text = $"{data.SaveDate}\n{time.ToString("hh':'mm':'ss")}";
        _caseNameText.text = $"Episode {data.CaseNum}   {data.CaseName}";
        _phaseNameText.text = data.PhaseName;
    }

    public int FindSaveNumber()
    {
        int num = 0;

        try
        {
            num = Int32.Parse(_saveNumText.text) - 1;
        }
        catch
        {
            Debug.LogError("Unable to find save file number");
        }

        return num;
    }
}

public class SaveIconData
{
    public int SaveNum;
    public string GameName;
    public string CaseName;
    public string PhaseName;
    public int CaseNum;
    public string SaveDate;
    public double PlayTime;
    public Color IconColor;
}