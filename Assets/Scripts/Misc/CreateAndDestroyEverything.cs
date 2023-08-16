using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAndDestroyEverything : MonoBehaviour
{
    
    public static MusicManager instance;
    public Texture2D[] prefabs;
    
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        prefabs = Resources.LoadAll<Texture2D>("Sprites");
        prefabs = null;
    }
}
