using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CarAudioController : NetworkBehaviour
{
    [SerializeField] private AudioSource carTurbine;
    [SerializeField] private AudioSource bounce;

    [ClientRpc]
    public void BounceClientRpc(float value)
    {
        bounce.volume = value;
        bounce.Play();
    }
}
