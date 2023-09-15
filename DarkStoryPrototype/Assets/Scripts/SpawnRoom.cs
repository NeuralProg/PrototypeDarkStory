using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoom : MonoBehaviour
{
    [SerializeField] LayerMask roomLayer;
    [SerializeField] private LevelGeneration levelGen;
    [SerializeField] private GameObject blank;
    private bool stop;


    void Update()
    {
        Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomLayer);  
        if(roomDetection == null && levelGen.stopGeneration && !stop)
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

            stop = true;
            Destroy(gameObject);
            //StartCoroutine(CorrectGeneration());
        }

    }

    private IEnumerator CorrectGeneration()
    {
        yield return new WaitForSeconds(2f);

        Collider2D roomDetectionCurrent = Physics2D.OverlapCircle(new Vector3(transform.position.x, transform.position.y, transform.position.z), 1, roomLayer);
        Collider2D roomDetectionLeft = Physics2D.OverlapCircle(new Vector3(transform.position.x - 20f, transform.position.y, transform.position.z), 1, roomLayer);
        Collider2D roomDetectionRight = Physics2D.OverlapCircle(new Vector3(transform.position.x + 20f, transform.position.y, transform.position.z), 1, roomLayer);
        Collider2D roomDetectionTop = Physics2D.OverlapCircle(new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z), 1, roomLayer);
        Collider2D roomDetectionBot = Physics2D.OverlapCircle(new Vector3(transform.position.x, transform.position.y - 10f, transform.position.z), 1, roomLayer);
        if ( (roomDetectionLeft.GetComponent<RoomType>().type == 0 || roomDetectionLeft.GetComponent<RoomType>().type == 1 || roomDetectionLeft.GetComponent<RoomType>().type == 10)
            && (roomDetectionRight.GetComponent<RoomType>().type == 0 || roomDetectionRight.GetComponent<RoomType>().type == 2 || roomDetectionRight.GetComponent<RoomType>().type == 10)
            && (roomDetectionTop.GetComponent<RoomType>().type == 4 || roomDetectionRight.GetComponent<RoomType>().type == 5 || roomDetectionRight.GetComponent<RoomType>().type == 10)
            && (roomDetectionBot.GetComponent<RoomType>().type == 4 || roomDetectionRight.GetComponent<RoomType>().type == 10))
        {
            roomDetectionCurrent.GetComponent<RoomType>().RoomDestruction();
            Instantiate(blank, transform.position, Quaternion.identity);
            print("test");
        }

        Destroy(gameObject);
    }
}
