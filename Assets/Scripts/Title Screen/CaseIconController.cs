using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaseIconController : MonoBehaviour
{
    [SerializeField] private Image _caseIcon;
    [SerializeField] private Image _caseName;
    [SerializeField] private TMP_Text _caseNameAlt;
    [SerializeField] private Image _black;
    [SerializeField] private Sprite _hiddenCase;

    [HideInInspector] public bool CaseLocked;

    public void UpdateCase(Sprite caseIcon=null, Sprite caseName=null, string altCaseName=null)
    {
        _caseName.gameObject.SetActive(true);
        
        if (caseIcon != null) _caseIcon.sprite = caseIcon;

        if (caseName != null)
        {
            _caseName.sprite = caseName;
            _caseName.enabled = true;
            _caseNameAlt.enabled = false;
        }
        else
        {
            _caseName.enabled = false;
            _caseNameAlt.enabled = true;
            _caseNameAlt.text = altCaseName;
        }
    }

    public void HideCase()
    {
        _caseIcon.sprite = _hiddenCase;
        _caseName.gameObject.SetActive(false);
        CaseLocked = true;
    }

    public IEnumerator SetSelected(bool selected)
    {
        float startPoint = 0;
        float endPoint = 0.8f;
        
        if (selected)
        {
            startPoint = 0.8f;
            endPoint = 0;
        }

        float time = 0f;
        float duration = 0.3f;
        Color col;

        while (time < duration)
        {
            col = _black.color;
            col.a = Mathf.Lerp(startPoint, endPoint, time / duration);
            _black.color = col;
            time += Time.deltaTime;
            yield return null;
        }
        
        col = _black.color;
        col.a = endPoint;
        _black.color = col;
    }

}
