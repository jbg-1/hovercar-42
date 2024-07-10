using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float maxRotation;

    [SerializeField] private AnimationCurve animationCurveX;
    [SerializeField] private AnimationCurve animationCurveY;
    [SerializeField] private AnimationCurve animationCurveZ;



    private float currentTime;

    private bool forewardDirection = true;

    void Update()
    {
        currentTime += forewardDirection ? Time.deltaTime * speed : -Time.deltaTime * speed;

        if (forewardDirection)
        {
            if (currentTime > 1)
            {
                forewardDirection = !forewardDirection;
            }
        }
        else
        {
            if (currentTime < 0)
            {
                forewardDirection = !forewardDirection;
            }
        }
        transform.rotation = Quaternion.Euler(animationCurveX.Evaluate(currentTime) * maxRotation, animationCurveY.Evaluate(currentTime) * maxRotation, animationCurveZ.Evaluate(currentTime) * maxRotation);
    }
}
