using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNodes : MonoBehaviour
{

    int numToSpawn = 28;

    public float currentSpawnOffset;
    public float spawnOffSet = 0.3f;
    // Start is called before the first frame update
    void Start()
    {


       // gameObject.name = "Node";
      //  return;

       /* if(gameObject.name == "Node")
        {
            currentSpawnOffset = spawnOffSet;
            for (int i = 0; i < numToSpawn; i++)
            {
                //Clone a new node
                GameObject clone = Instantiate(gameObject, new Vector3(transform.position.x  , transform.position.y + currentSpawnOffset, 0), Quaternion.identity);
                currentSpawnOffset += spawnOffSet;

            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
