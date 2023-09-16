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
    public GameObject[] LR;
    public GameObject[] LRT;
    public GameObject roomStart;
    public GameObject roomEnd;
    public GameObject player;
    private Vector3 playerSpawnPos;

    public GameObject[] shopLR;
    public GameObject[] shopLRT;
    public GameObject[] shopTB;
    public GameObject[] shopTBL;
    public GameObject[] shopTBR;
    private int chancesToSpawnShop = -20;
    private bool shopHasBeenAdded = false;
    private int maxShopAmount = 2;

    private float moveAmount = 20f;
    private int direction;

    private float timeBtwRoom;
    private float startTimeBtwRoom = 0.05f;

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
        playerSpawnPos = new Vector3(transform.position.x - 2f, transform.position.y - 0.5f, transform.position.z);

        /*Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmount);
        transform.position = newPos;
        Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);*/

        direction = Random.Range(1, 7);
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
        bool shouldSpawnShop = (Random.Range(0, 101) < chancesToSpawnShop && maxShopAmount > 0) ;

        if( direction == 1 || direction == 2) // Move up
        { 
            if(transform.position.y < maxY)
            {
                rightCounter = 0;

                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmount / 2);
                transform.position = newPos;

                if (!shouldSpawnShop)
                {
                    int rand = Random.Range(0, 4);
                    if (rand == 0)
                        Instantiate(TB[Random.Range(0, TB.Length)], transform.position, Quaternion.identity);
                    else if (rand == 1)
                        Instantiate(TBL[Random.Range(0, TBL.Length)], transform.position, Quaternion.identity);
                    else if (rand == 2)
                        Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);
                    else if (rand == 3)
                        Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                }
                else
                {
                    int rand = Random.Range(0, 3);
                    if (rand == 0)
                        Instantiate(shopTB[Random.Range(0, shopTB.Length)], transform.position, Quaternion.identity);
                    else if (rand == 1)
                        Instantiate(shopTBL[Random.Range(0, shopTBL.Length)], transform.position, Quaternion.identity);
                    else if (rand == 2)
                        Instantiate(shopTBR[Random.Range(0, shopTBR.Length)], transform.position, Quaternion.identity);

                    shopHasBeenAdded = true;
                }

                direction = Random.Range(1, 7);
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

                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmount / 2);
                transform.position = newPos;

                if (!shouldSpawnShop)
                {
                    int rand = Random.Range(0, 4);
                    if (rand == 0)
                        Instantiate(TB[Random.Range(0, TB.Length)], transform.position, Quaternion.identity);
                    else if (rand == 1)
                        Instantiate(TBL[Random.Range(0, TBL.Length)], transform.position, Quaternion.identity);
                    else if (rand == 2)
                        Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);
                    else if (rand == 3)
                        Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                }
                else
                {
                    int rand = Random.Range(0, 3);
                    if (rand == 0)
                        Instantiate(shopTB[Random.Range(0, shopTB.Length)], transform.position, Quaternion.identity);
                    else if (rand == 1)
                        Instantiate(shopTBL[Random.Range(0, shopTBL.Length)], transform.position, Quaternion.identity);
                    else if (rand == 2)
                        Instantiate(shopTBR[Random.Range(0, shopTBR.Length)], transform.position, Quaternion.identity);

                    shopHasBeenAdded = true;
                }

                direction = Random.Range(3, 7);
            }
            else
            {
                direction = 5;
            }
        }
        else if (direction == 5 || direction == 6) // Move right
        {
            rightCounter++;

            if(transform.position.x < maxX) 
            {
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomLayer);
                if(roomDetection.GetComponent<RoomType>().type != 2 && roomDetection.GetComponent<RoomType>().type != 3 && roomDetection.GetComponent<RoomType>().type != 4 && roomDetection.GetComponent<RoomType>().type != 5 && roomDetection.GetComponent<RoomType>().type != 10)
                {
                    if (rightCounter >= 2)
                    {
                        if (!roomDetection.GetComponent<RoomType>().isShop)
                        {
                            roomDetection.GetComponent<RoomType>().RoomDestruction();
                            int randSpawn = Random.Range(0, 3);
                            if (randSpawn == 0)
                                Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                            else if (randSpawn == 1)
                                Instantiate(LR[Random.Range(0, LR.Length)], transform.position, Quaternion.identity);
                            else if (randSpawn == 2)
                                Instantiate(LRT[Random.Range(0, LRT.Length)], transform.position, Quaternion.identity);
                        }
                        else
                        {
                            roomDetection.GetComponent<RoomType>().RoomDestruction();
                            int randSpawn = Random.Range(0, 2);
                            if (randSpawn == 0)
                                Instantiate(shopLR[Random.Range(0, shopLR.Length)], transform.position, Quaternion.identity);
                            else if (randSpawn == 1)
                                Instantiate(shopLRT[Random.Range(0, shopLRT.Length)], transform.position, Quaternion.identity);

                            shopHasBeenAdded = true;
                        }
                    }
                    else
                    {
                        if (!roomDetection.GetComponent<RoomType>().isShop)
                        {
                            roomDetection.GetComponent<RoomType>().RoomDestruction();
                            int randRightRoom = Random.Range(0, 2);
                            if (randRightRoom == 0)
                                Instantiate(TBR[Random.Range(0, TBR.Length)], transform.position, Quaternion.identity);
                            else if (randRightRoom == 1)
                                Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                        }
                        else 
                        {
                            roomDetection.GetComponent<RoomType>().RoomDestruction();
                            Instantiate(shopTBR[Random.Range(0, shopTBR.Length)], transform.position, Quaternion.identity);

                            shopHasBeenAdded = true;
                        }
                    }
                }

                Vector2 newPos = new Vector2(transform.position.x + moveAmount, transform.position.y);
                transform.position = newPos;

                if (!shouldSpawnShop)
                {
                    int rand = Random.Range(0, 2);
                    if (rand == 0)
                        Instantiate(TBL[Random.Range(0, TBL.Length)], transform.position, Quaternion.identity);
                    else if (rand == 1)
                        Instantiate(TBLR[Random.Range(0, TBLR.Length)], transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(shopTBL[Random.Range(0, shopTBL.Length)], transform.position, Quaternion.identity);
                    shopHasBeenAdded = true;
                }

                direction = Random.Range(1, 7);

            }
            else
            {
                stopGeneration = true;

                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, roomLayer);
                if(roomDetection != null)
                    roomDetection.GetComponent<RoomType>().RoomDestruction();

                Instantiate(roomEnd, transform.position, Quaternion.identity);
                Instantiate(player, playerSpawnPos, Quaternion.identity);
            }

            if (!shopHasBeenAdded && !shouldSpawnShop)
            {
                chancesToSpawnShop += 10;
            }
        }

        if (shouldSpawnShop && !shopHasBeenAdded)
        {
            chancesToSpawnShop = 101;
        }
        else if (shopHasBeenAdded && chancesToSpawnShop > 0)
        {
            chancesToSpawnShop = -30;
            maxShopAmount --;
        }
        shopHasBeenAdded = false;
    }
}
