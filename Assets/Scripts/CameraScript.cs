using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [SerializeField] private GameObject cameraPositionArim;
    [SerializeField] private GameObject lookAt;
    [SerializeField] private float speed;
    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * (cameraPositionArim.transform.position - transform.position),Space.World);
        transform.LookAt(lookAt.transform.position);
    }
}
