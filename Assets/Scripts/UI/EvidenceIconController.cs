using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceIconController : MonoBehaviour
{
    [SerializeField] private Image _evidenceIcon;

    public IEnumerator Expand(Sprite icon)
    {
        if (transform.localScale != Vector3.zero)
        {
            yield return StartCoroutine(Shrink());
        }
        
        SetImage(icon);
        
        transform.localScale = Vector3.zero;
        Globals.SoundManager.Play("evidenceImage");

        float time = 0;
        float duration = 0.1f;

        while (time < duration)
        {
            transform.localScale = Vector3.one * (time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
        
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator Shrink()
    {
        transform.localScale = Vector3.one;
        
        Globals.SoundManager.Play("evidenceImage");

        float time = 0;
        float duration = 0.1f;

        while (time < duration)
        {
            transform.localScale = new Vector3(1 + Mathf.Max(0.2f * (Mathf.Log(time / duration + 0.1f) + 1.5f), 0f), 1 - (time / duration), 1);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        SetImage(null);

        yield return new WaitForSeconds(0.2f);
    }
    
    public void SetImage(Sprite img)
    {
        _evidenceIcon.sprite = img;
    }

    public Sprite CurrentIcon => _evidenceIcon.sprite;
}
