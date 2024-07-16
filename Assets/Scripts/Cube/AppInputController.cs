using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FinishTimer;

public class AppInputController : AppController
{
    [SerializeField] public static float Orientation { get; private set; } = 0;
    
    private float startOrientation;

    private bool firstDataLoaded = false;

    public static event OnUseItem onUseItem;

    protected override void Initialize()
    {
        base.Initialize();
        state.CubeId = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

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
                startOrientation = (float)cubeControl.Orientation;
            }

            float input = (float)(cubeControl.Orientation - startOrientation) + 540;
            Orientation = -(input % 360 - 180);
        }   
    }

    public delegate void OnUseItem();

}
