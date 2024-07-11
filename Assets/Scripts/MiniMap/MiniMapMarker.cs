using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapMarker : MonoBehaviour
{
    private Vector3 worldCenter;
    private Vector2 scale;
    private GameObject carGameObject;

    [SerializeField] private RectTransform rectTransform;

    public void setValues(Vector3 worldCenter, Vector2 scale, GameObject carGameObject)
    {
        this.worldCenter = worldCenter;
        this.scale = scale;
        this.carGameObject = carGameObject;
    }

    private void Update()
    {
        rectTransform.anchoredPosition = new Vector2((carGameObject.transform.position.x-worldCenter.x) * scale.x, (carGameObject.transform.position.z - worldCenter.z) * scale.y);
    }
}
