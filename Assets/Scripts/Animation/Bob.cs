using UnityEngine;
using System.Collections;
 
public class Bob : MonoBehaviour {
    public float amplitude = 0.5f;
    public float frequency = 1f;

    public RectTransform _RectTransform;
    
    public bool sinOffset;
    private float offsetAmount;
    private float _bottom;
 
    Vector2 scaleOffset = new Vector2 ();
    Vector2 tempScale = new Vector2 ();
 
    void Awake () {
        scaleOffset = _RectTransform.sizeDelta;
        if (sinOffset) offsetAmount = transform.localPosition.x;
        _bottom = _RectTransform.offsetMin.y;
    }
     
    void Update ()
    {
        tempScale = scaleOffset;
        tempScale.y += Mathf.Sin((Time.timeSinceLevelLoad + offsetAmount) * frequency) * amplitude;
        _RectTransform.sizeDelta = tempScale;

        _RectTransform.offsetMin = new Vector2(_RectTransform.offsetMin.x, _bottom);
    }
}