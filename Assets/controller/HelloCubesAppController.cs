using System;
using System.Collections;
using System.Collections.Generic;
using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using UnityEngine;

public class HelloCubesAppController : AppController
{

    public FloatEvent orientationEvent;
    HelloCubesEventDispatcher helloCubesEventDispatcher;

    public HelloCubesAppState appState;

    public void HandleHelloCubes(HelloCubes helloCubes)
    {
        // Hello Cubes Message received
        Debug.Log(helloCubes.Message);

        // Answer message
        helloCubesEventDispatcher.DispatchHelloCubes(new HelloCubes
        {
            Message = "Hello back from " + state.CubeId
        });
    }

    public void HandleCubeControl(CubeControl cubeControl)
    {
        if (cubeControl == null) return;


        orientationEvent?.Invoke(cubeControl.Orientation.GetValueOrDefault());

        if(Mathf.Abs(appState.Orientation  -cubeControl.Orientation.GetValueOrDefault()) > 10)
        {
            appState.Orientation = cubeControl.Orientation.GetValueOrDefault();
		    stateDirty = true;
            
        } 

        // deactivate spash screen
        if(cubeControl.Moving == true)
        {
            ShowSplashScreen(false);
        }   


    }

    protected override void Initialize() { 
        helloCubesEventDispatcher = FindObjectOfType<HelloCubesEventDispatcher>();

        
        this.state = new HelloCubesAppState(state);      // HACK: override satte in base class
        appState = state as HelloCubesAppState;
    }

}
