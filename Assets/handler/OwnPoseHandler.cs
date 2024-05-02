using System.Collections;
using System.Collections.Generic;
using PuzzleCubes.Models;
using UnityEngine;

public class OwnPoseHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public void HandlePose(CubePose pose)
    {
        this.transform.position = new Vector3(pose.Position.x, this.transform.position.y, pose.Position.y);

        // ignore orientation in own Cube
        
    }
}
