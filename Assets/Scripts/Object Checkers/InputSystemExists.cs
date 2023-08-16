using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputSystemExists : MonoBehaviour
{
    void Awake()
    {
        if (Globals.EventSystem != null)
        {
            Destroy(gameObject);
            return;
        }
        Globals.EventSystem = GetComponent<EventSystem>();
    }
}