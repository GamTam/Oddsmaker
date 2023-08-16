using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warping : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ChangeScenes changeScenes = GameObject.FindWithTag("ChangeScenes").GetComponent<ChangeScenes>();
        changeScenes.StartCoroutine(changeScenes.FadeIn());
        if(Globals.WarpFlag)
        {
            string myString = PlayerPrefs.GetString("MyStringKey");
            GameObject spawnLocation = GameObject.Find(myString);
            GameObject[] player = new GameObject[4];
            player[0] = GameObject.Find("Player");
            player[1] = GameObject.Find("Owl House");
            player[2] = GameObject.Find("Minion");
            player[3] = GameObject.Find("JoJo");
            for(int j = 0; j < 4; j++)
            {
                player[j].transform.position = spawnLocation.transform.position;
            }
        }
        StartCoroutine(DeleteObject());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DeleteObject()
    {
        yield return new WaitForSeconds(0.5f);
        Globals.TurnOffAllWarpFlags();
        Destroy(gameObject);
    }
}
