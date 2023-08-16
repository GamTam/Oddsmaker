using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnemySpawner : MonoBehaviour
{
    public GameObject _enemySpawner;

    public bool _isCutscene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !(other.isTrigger))
        {
            if(_enemySpawner != null)
            {
                _enemySpawner.SetActive(false);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !(other.isTrigger))
        {
            if(_enemySpawner != null)
            {
                if(!_isCutscene)
                {
                    _enemySpawner.SetActive(true);
                }
            }
        }
    }
}
