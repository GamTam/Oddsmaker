using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortSpriteOrder : MonoBehaviour
{
    private SpriteRenderer _sprite;
    
    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        _sprite.sortingOrder =  1000000 - (int) (transform.position.y * 100);
    }
}
