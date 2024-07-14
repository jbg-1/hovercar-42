using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DebugStartSceneNutzen : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
            text.enabled = false;
    }
}
