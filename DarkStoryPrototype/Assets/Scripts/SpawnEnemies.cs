using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    private LevelGeneration levelGen;

    [Header("Enemies info")]
    [SerializeField] private GameObject[] enemyObject;
    [SerializeField] private Transform[] enemyPos;
    [SerializeField] private Transform[] waypointsPos; //[..., ..., ..., ...] on prends les 3 premiers elements
    private int enemyIndex = 0;
    private bool stop = false;

    private void Awake()
    {
        levelGen = GameObject.Find("LevelGen").GetComponent<LevelGeneration>();
    }

    private void Update()
    {
        if (levelGen.stopGeneration && !stop)
        {
            stop = true;
            StartToSpawnEnemies();
        }
    }

    private void StartToSpawnEnemies()
    {
        foreach (var enemy in enemyObject)
        {
            var enemyRef = Instantiate(enemyObject[enemyIndex], enemyPos[enemyIndex].position, Quaternion.identity);
            enemyRef.transform.parent = gameObject.transform.parent;

            enemyRef.gameObject.transform.GetChild(1).GetChild(0).gameObject.transform.position = waypointsPos[enemyIndex * 3].position;
            enemyRef.gameObject.transform.GetChild(1).GetChild(1).gameObject.transform.position = waypointsPos[(enemyIndex * 3) + 1].position;
            enemyRef.gameObject.transform.GetChild(1).GetChild(2).gameObject.transform.position = waypointsPos[(enemyIndex * 3) + 2].position;

            enemyIndex++;
        }
        Destroy(gameObject);
    }
}
