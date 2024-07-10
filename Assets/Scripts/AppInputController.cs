using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FinishTimer;

public class AppInputController : AppController
{
    [SerializeField] public static float Orientation { get; private set; } = 0;
    
    private float startOrientation;

    private bool firstDataLoaded = false;

    public static event OnUseItem onUseItem;


    public void HandleCubeControl(CubeControl cubeControl)
    {
        if (cubeControl.TranslationStepForward.GetValueOrDefault(false))
        {
            onUseItem();
        }
        if(cubeControl.Orientation != 0)
        {
            if (!firstDataLoaded)
            {
                firstDataLoaded = true;
                startOrientation = (float)cubeControl.Orientation + 180;
            }

            float input = (float)(cubeControl.Orientation + 540 - startOrientation);
            Orientation = input % 360 - 180;
        }   
    }

    public delegate void OnUseItem();

}
