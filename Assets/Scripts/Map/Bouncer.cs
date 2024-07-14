using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{

    [SerializeField] private float bounceRate = 1;

    public float BounceRate()
    {
        return bounceRate;
    }
}
