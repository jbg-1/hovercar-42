using System;
using System.Collections;
using System.Collections.Generic;
using PuzzleCubes.Models;
using UnityEngine;

public class HelloCubesAppState : AppState
{
    private float orientation;

    public float Orientation { get => orientation; set => orientation = value; }

    public HelloCubesAppState(AppState toCopy) : base(toCopy) {
    }
}
