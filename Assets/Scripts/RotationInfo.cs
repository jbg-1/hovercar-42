using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotationInfo : MonoBehaviour
{
    [SerializeField] Slider slider;

    // Update is called once per frame
    void Update()
    {
        slider.value = AppInputController.Orientation;
    }
}
