using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoom : MonoBehaviour
{
    [SerializeField] LayerMask roomLayer;
    [SerializeField] private LevelGeneration levelGen;
    [SerializeField] private GameObject blank;


    void Update()
    {
        Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomLayer);
        if(roomDetection == null && levelGen.stopGeneration)
        {
            int rand = Random.Range(0, 6);
            if (rand == 0)
                Instantiate(levelGen.TB[Random.Range(0, levelGen.TB.Length)], transform.position, Quaternion.identity);
            else if (rand == 1)
                Instantiate(levelGen.TBL[Random.Range(0, levelGen.TBL.Length)], transform.position, Quaternion.identity);
            else if (rand == 2)
                Instantiate(levelGen.TBR[Random.Range(0, levelGen.TBR.Length)], transform.position, Quaternion.identity);
            else if (rand == 3)
                Instantiate(levelGen.TBLR[Random.Range(0, levelGen.TBLR.Length)], transform.position, Quaternion.identity);
            else if (rand == 4 || rand == 5)
                Instantiate(blank, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
