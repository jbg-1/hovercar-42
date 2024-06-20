using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppInputController : AppController
{
    [SerializeField] public static float Orientation { get; private set; } = 0;
    private float startOrientation;

    public void HandleCubeControl(CubeControl cubeControl)
    {
        Orientation = (float)cubeControl.Orientation;
    }
}
