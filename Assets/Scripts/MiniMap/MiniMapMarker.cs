using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapMarker : MonoBehaviour
{
    private Vector3 worldCenter;
    private Vector2 scale;
    private GameObject carGameObject;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;


    public void setValues(Vector3 worldCenter, Vector2 scale, GameObject carGameObject, int id)
    {
        this.worldCenter = worldCenter;
        this.scale = scale;
        this.carGameObject = carGameObject;
        image.color = PlayerColors.instance.getColor(id).color;
    }

    private void Update()
    {
        Vector2 anchor = new Vector2((carGameObject.transform.position.x - worldCenter.x) * scale.x, (carGameObject.transform.position.z - worldCenter.z) * scale.y);
        rectTransform.anchoredPosition = anchor;
    }
}
