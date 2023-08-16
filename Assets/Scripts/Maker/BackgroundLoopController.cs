using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLoopController : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;

    private int _width;
    private int _height;
    
    private void Start()
    {
        _width = _sprite.texture.width;
        _height = _sprite.texture.height;
    }

    private void Update()
    {
        float x = transform.position.x;
        float y = transform.position.y;

        if (x * 100 > _width * 1.5f) x -= (_width / 100f) * 3f;
        else if (x * 100 < -(_width * 1.5f)) x += (_width / 100f) * 3f;
        
        if (y * 100f > _height * 1.5f) y -= (_height / 100f) * 3f;
        else if (y * 100f < -(_height * 1.5f)) y += (_height / 100f) * 3f;

        transform.position = new Vector2(x, y);
    }
}
