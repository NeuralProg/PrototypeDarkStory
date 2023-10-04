using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScript : MonoBehaviour
{
    private float _startingPos; //This is starting position of the sprites.
    private float _lengthOfSprite;    //This is the length of the sprites.
    [SerializeField] private float AmountOfParallax;  //This is amount of parallax scroll. 
    [SerializeField] private Transform MainCamera;   //Reference of the camera.
    [SerializeField] private bool invertMovement = false;


    void Start()
    {
        //Getting the starting X position of sprite.
        _startingPos = transform.position.x;
        //Getting the length of the sprites.
        _lengthOfSprite = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        Vector3 Position = MainCamera.position;
        float Temp = Position.x * (1 - AmountOfParallax);
        float Distance = Position.x * AmountOfParallax;

        if (!invertMovement)
        {
            Vector3 NewPosition = new Vector3(_startingPos + Distance, transform.position.y, transform.position.z);
            transform.position = NewPosition;
        }
        else
        {
            Vector3 NewPosition = new Vector3(-(_startingPos + Distance), transform.position.y, transform.position.z);
            transform.position = NewPosition;
        }
    }
}
