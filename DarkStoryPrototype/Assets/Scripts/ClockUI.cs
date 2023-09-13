using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour {

    [HideInInspector] public const float REAL_SECONDS_PER_INGAME_DAY = 60f * 20; // How long is the day (in seconds)
    [HideInInspector] public float day; // 1 = 1 full rotation
    [HideInInspector] public bool stop = false;
    public float timeRatio = 1f;

    [SerializeField] private Transform clockMinuteHandTransform;
    [SerializeField] private Transform clockSecondHandTransform;

    private void Update() {
        timeRatio = Mathf.Clamp(timeRatio, 0.5f, 10f); // Clamp 

        if(!stop)
            day += (Time.deltaTime / REAL_SECONDS_PER_INGAME_DAY) * timeRatio; // we multiply the value added to the hands by the timeRatio

        float dayNormalized = day % 1f;

        float rotationDegreesPerDay = 360f; // A full rotation for one day
        clockMinuteHandTransform.eulerAngles = new Vector3(0, 0, -dayNormalized * rotationDegreesPerDay);

        float secondPerDay = 60f; // How many rotations in 1 day
        clockSecondHandTransform.eulerAngles = new Vector3(0, 0, -dayNormalized * rotationDegreesPerDay * secondPerDay);
    }

}
