using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField] private Transform[] startingPositions;
    public GameObject[] TB;
    public GameObject[] TBL;
    public GameObject[] TBR;
    public GameObject[] TBLR;
    public GameObject roomStart;
    public GameObject roomEnd;

    private float moveAmount = 10f;
    private int direction;

    private float timeBtwRoom;
    private float startTimeBtwRoom = 0f;

    private int rightCounter;

    [SerializeField] private LayerMask roomLayer;
    [SerializeField] private float maxX;
    [SerializeField] private float maxY;
    [SerializeField] private float minY;
    [HideInInspector] public bool stopGeneration;

    private bool startGenerating = false;


    void Start()
    {
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;
        Instantiate(roomStart, transform.position, Quaternion.identity);

        direction = Random.Range(1, 6);
        startGenerating = true;
    }

    private void Update()
    {
        if (startGenerating)
        {
            if (timeBtwRoom <= 0 && !stopGeneration)
            {
                Move();
                timeBtwRoom = startTimeBtwRoom;
            }
            else
            {
                timeBtwRoom -= Time.deltaTime;
            }
        }
    }

    private void Move()
    {
        if( direction == 1 || direction == 2) // Move up
        { 
            if(transform.position.y < maxY)
            {
                rightCounter = 0;

                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmount);
                transform.position = newPos;

                int rand = Random.Range(0, 4);
                if(rand == 0)
                    Instantiate(TB[Random.Range(0, TB.Length)], transform.position, Quaternion.identity);
                else if (rand == 1)
                    Instantiate(TBL[Random.Range(0, TBL.Length)], transform.position, Quaternion.identity);
                else if (rand == 2)
                    Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);
                else if (rand == 3)
                    Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);

                direction = Random.Range(1, 6);
                if(direction == 3) 
                { 
                    direction = 2;
                }
                if (direction == 4)
                {
                    direction = 5;
                }
            }
            else
            {
                direction = 5;
            }
        }
        else if (direction == 3 || direction == 4) // Move down
        {
            if (transform.position.y > minY)
            {
                rightCounter = 0;

                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmount);
                transform.position = newPos;

                int rand = Random.Range(0, 4);
                if (rand == 0)
                    Instantiate(TB[Random.Range(0, TB.Length)], transform.position, Quaternion.identity);
                else if (rand == 1)
                    Instantiate(TBL[Random.Range(0, TBL.Length)], transform.position, Quaternion.identity);
                else if (rand == 2)
                    Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);
                else if (rand == 3)
                    Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);

                direction = Random.Range(3, 6);
            }
            else
            {
                direction = 5;
            }
        }
        else if (direction == 5) // Move right
        {
            rightCounter++;

            if(transform.position.x < maxX) 
            {
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomLayer);
                if(roomDetection.GetComponent<RoomType>().type != 2 && roomDetection.GetComponent<RoomType>().type != 3 && roomDetection.GetComponent<RoomType>().type != 4)
                {
                    if (rightCounter >= 2)
                    {
                        roomDetection.GetComponent<RoomType>().RoomDestruction();
                        Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                    }
                    else
                    {
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        int randRightRoom = Random.Range(2, 4);
                        if (randRightRoom == 2)
                            Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);
                        else if (randRightRoom == 3)
                            Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                    }
                }

                Vector2 newPos = new Vector2(transform.position.x + moveAmount, transform.position.y);
                transform.position = newPos;

                int rand = Random.Range(1, 4);
                if (rand == 1 )
                    Instantiate(TBL[Random.Range(0, TBL.Length)], transform.position, Quaternion.identity);
                else if (rand == 3 || rand == 2)
                    Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);

                direction = Random.Range(1, 6);
            }
            else
            {
                stopGeneration = true;
                Instantiate(roomEnd, transform.position, Quaternion.identity);
                return;
            }
        }
    }
}
