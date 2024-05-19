using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppInputController : AppController
{
    public static float Orientation { get; private set; } = 0;

    public void HandleCubeControl(CubeControl cubeControl)
    {
        Orientation = cubeControl.Orientation.Value;
    }
}
