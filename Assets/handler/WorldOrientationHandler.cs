using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldOrientationHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public void HandleOrienatiton(float orientation)
    {
        
        this.transform.rotation = Quaternion.Euler(0.0f, + orientation, 0.0f);
    }
}
