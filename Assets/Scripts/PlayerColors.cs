using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColors : MonoBehaviour
{
    public PlayerMaterial[] colors;

    [System.Serializable]
    public struct PlayerMaterial
    {
        public Material material;
        public string name;
    }
}
