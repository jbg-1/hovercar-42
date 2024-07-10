using System.Collections;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    private Renderer itemRenderer;
    private Collider itemCollider;

    // Start is called before the first frame update
    void Start()
    {
        itemRenderer = GetComponent<Renderer>();
        itemCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 50 * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out ItemController itemController))
        {
            itemController.collectItem();
            StartCoroutine(RespawnItem());

        }
    }

    private IEnumerator RespawnItem()
    {
        // Hide the item and deactivate the collider
        itemRenderer.enabled = false;
        itemCollider.enabled = false;

        // Wait for 6 seconds
        yield return new WaitForSeconds(6f);

        // Show the item again and reactivate the collider
        itemRenderer.enabled = true;
        itemCollider.enabled = true;
    }
}
