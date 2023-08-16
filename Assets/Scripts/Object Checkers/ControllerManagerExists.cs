using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerManagerExists : MonoBehaviour
{
    void Awake()
    {
        if (Globals.Input != null)
        {
            Destroy(gameObject);
            return;
        }
        Globals.Input = GetComponent<PlayerInput>();
    }
}
