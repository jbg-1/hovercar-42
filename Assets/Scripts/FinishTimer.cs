using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishTimer : MonoBehaviour
{
    [SerializeField] HUD hud;

    private float finishTimerDuration = 30f;
    
    public event OnTimerFinished onFinish;
    private bool timerRunning = false;

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    private void Update()
    {
        if (timerRunning)
        {
            finishTimerDuration -= Time.deltaTime;
            hud.UpdateTimer(FormatTimer(finishTimerDuration));
            if (finishTimerDuration < 0)
            {
                onFinish();
                timerRunning = false;
            }
        }
    }

    private string FormatTimer(float timer)
    {
        int seconds = Mathf.FloorToInt(timer % 60);
        return string.Format("{0:00}", seconds);
    }

    public delegate void OnTimerFinished();
}
