using PuzzleCubes.Communication;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCarCubeEventDispatcher : EventDispatcher
{
    [SerializeField] private MqttCommunicationHoverCar MqttCommunicationHoverCar;
    protected override void PostInitialize()
    {
        base.PostInitialize();
        MqttCommunicationHoverCar.Init();
    }
}
