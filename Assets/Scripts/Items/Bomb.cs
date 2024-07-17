using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Bomb : NetworkBehaviour
{
    [SerializeField] private float explosionForce = 40f;
    [SerializeField] private float radius = 8f;
    [SerializeField] private ParticleSystem particlSystem;
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private AudioSource sound;

    private bool exploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer && !exploded)
        {
            exploded = true;
            explodeClientRpc();
        }
    }

    [ClientRpc]
    public void explodeClientRpc()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<CarController>(out CarController car) && car.IsOwner)
            {
                car.BombClientRpc((car.transform.position - transform.position).normalized * explosionForce);
            }
        }
        StartCoroutine(StartDestroy());
    }

    private IEnumerator StartDestroy()
    {
        sound.Play();
        particlSystem.Play();
        mesh.enabled = false;
        yield return new WaitForSeconds(3f);
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}
