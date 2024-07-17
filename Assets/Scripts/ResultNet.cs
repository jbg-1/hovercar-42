using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultNet : MonoBehaviour
{
    public List<int> LastRaceResult;

    public static ResultNet instance;

    private void Awake()
    {
        instance = this;
    }
}
