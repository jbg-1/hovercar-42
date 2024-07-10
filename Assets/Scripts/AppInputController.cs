using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppInputController : AppController
{
    [SerializeField] public static float Orientation { get; private set; } = 0;
    private float startOrientation;

    private bool firstDataLoaded = false;

    public void HandleCubeControl(CubeControl cubeControl)
    {
        //Debug.Log("Raw orientation " + cubeControl.Orientation);
        if (!firstDataLoaded)
        {
            firstDataLoaded = true;
            startOrientation = (float)cubeControl.Orientation + 180;
        }
        float input = (float)(cubeControl.Orientation + 540 - startOrientation);
        Orientation = input % 360 - 180;
        //Debug.Log("calculated orientation " + Orientation);

       
    }
}
