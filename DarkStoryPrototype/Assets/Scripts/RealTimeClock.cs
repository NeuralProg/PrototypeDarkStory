using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealTimeClock : MonoBehaviour
{
    public Transform clockMinuteHandTransform;
    public Transform clockSecondHandTransform;
    public Transform clockHoursHandTransform;

    private const float SECONDS_PER_MINUTE = 60f;
    private const float MINUTES_PER_HOUR = 60f;
    private const float HOURS_PER_DAY = 12f;

    private void Update()
    {
        // Obtenir l'heure actuelle
        System.DateTime currentTime = System.DateTime.Now;

        // Calculer les angles des aiguilles
        float secondsAngle = (((currentTime.Second - 15) / SECONDS_PER_MINUTE) * 360f);
        float minutesAngle = (((currentTime.Minute - 15) + (currentTime.Second - 15) / SECONDS_PER_MINUTE) / MINUTES_PER_HOUR) * 360f;
        float hoursAngle = (((currentTime.Hour - 15) + (currentTime.Minute - 15) / MINUTES_PER_HOUR) / HOURS_PER_DAY) * 360f;

        // Appliquer les rotations aux aiguilles
        clockSecondHandTransform.localRotation = Quaternion.Euler(0f, 0f, -secondsAngle);
        clockMinuteHandTransform.localRotation = Quaternion.Euler(0f, 0f, -minutesAngle);
        clockHoursHandTransform.localRotation = Quaternion.Euler(0f, 0f, -hoursAngle);
    }
}
