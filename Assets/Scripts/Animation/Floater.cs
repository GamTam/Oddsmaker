using UnityEngine;
using System.Collections;
 
public class Floater : MonoBehaviour {
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public bool sinOffset;
    private float offsetAmount;
 
    Vector3 posOffset = new Vector3 ();
    Vector3 tempPos = new Vector3 ();
 
    void Awake () {
        posOffset = transform.localPosition;
        if (sinOffset) offsetAmount = transform.localPosition.x;
    }
     
    void Update ()
    {
        tempPos = posOffset;
        tempPos.y += Mathf.Sin((Time.timeSinceLevelLoad + offsetAmount) * frequency) * amplitude;
 
        transform.localPosition = new Vector3(transform.localPosition.x, tempPos.y);
    }
}