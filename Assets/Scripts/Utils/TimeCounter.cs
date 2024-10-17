using System;
using UnityEngine;

[Serializable]
public partial class TimeCounter
{
    private float time = 0f;
    private float counter = 0f;

    [field: SerializeField]
    public bool isWorking {get; private set;}
    private bool wasEndedThisFrame = false;

    public bool useFixedDeltaTime = false;
    

    public void Start(float t, bool useFixedDeltaTime = false)
    {
        wasEndedThisFrame = false;

        isWorking = true;
        time = counter = t;

        this.useFixedDeltaTime = useFixedDeltaTime;
    } 

    public void Stop()
    {
        isWorking = false;
        time = counter = 0f;
    }

    public void Update()
    {
        wasEndedThisFrame = false;

        if (isWorking)
        { 
            counter -= useFixedDeltaTime ? Time.fixedDeltaTime : Time.deltaTime;

            if (counter <= 0f)
            {
                Stop();
                
                wasEndedThisFrame = true;
            }
        }
    }

    public void Pause()
    {
        isWorking = false;
    }

    public void Resume()
    {
        isWorking = true;
    }

    public float GetTimeLeft()
    {
        return counter;
    }

    public float GetElapsedTime()
    {
        return time - counter;
    }

    public float GetTimeLeft01()
    {
        return GetTimeLeft() / time;
    }

    public float GetElapsedTime01()
    {
        return GetElapsedTime() / time;
    } 

    public bool WasEndedThisFrame()
    {
        return wasEndedThisFrame;
    }
}