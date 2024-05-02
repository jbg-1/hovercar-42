using System.Collections;
using System.Collections.Generic;
using PuzzleCubes.Controller;
using PuzzleCubes.Models;
using UnityEngine;




public class HelloCubesKeyboardController : KeyboardController
{

    public MqttEvent helloCubesEvent;
    public HelloCubesEventDispatcher helloCubesEventDispatcher;
    public HelloCubesAppController appController;


    protected override void Initialize()
    {
        base.Initialize();

        helloCubesEventDispatcher = this.GetComponentInParent<HelloCubesEventDispatcher>();
        
        // SAMPLE KEYBOARD BINDINGS - BEGIN

        // DISPATCH MQTT MESSAGE
        keyToEventMap.Add(KeyCode.Space, () =>
        {
            helloCubesEventDispatcher?.DispatchHelloCubes(new HelloCubes
            {
               Message = "Hello from " + appController.state.CubeId
            });
           
        });

         // MOCK  INTERNAL JSON MESSAGE
        // keyToEventMap.Add(KeyCode.Space, () =>
        // {
        //     dispatchObject(new HelloCubes
        //     {
        //        Message = "Hello, Cubes!"
        //     });
        // });
    }

}
