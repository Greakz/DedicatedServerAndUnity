using System;
using UnityEditor;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int Id;
    public string Username;

    public Vector3 TargetPosition;
    public DateTime ReachTargetTime;
    
    public void Update()
    {
        Vector3 distance = (TargetPosition - this.transform.position);

        TimeSpan timeLeft = ReachTargetTime - DateTime.Now;

        float stepsLeft = timeLeft.Milliseconds / (Time.deltaTime * 1000f);

        if (distance.magnitude > 0.1f)
        {
            Vector3 framePassDistance = distance * (1.0f / stepsLeft);
        
            transform.position += framePassDistance;
        }
        else
        {
            transform.position = TargetPosition;
        }
    }
}