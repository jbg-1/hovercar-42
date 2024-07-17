using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject playerMarkerPrefab;

    [SerializeField] private Vector3 worldCenter;
    [SerializeField] private Vector2 scale;
    [SerializeField] private RectTransform rectTransform;

    private Vector2 scaleChange;

    private void Awake()
    {
        scaleChange = new Vector2(rectTransform.rect.width/scale.x, rectTransform.rect.height / scale.y);
    }

    public void InstantiateMarker(GameObject gameObject, int id)
    {
        GameObject newMarker = Instantiate(playerMarkerPrefab, transform);
        MiniMapMarker miniMapMarker = newMarker.GetComponent<MiniMapMarker>();
        miniMapMarker.setValues(worldCenter, scaleChange, gameObject,id);
    }
}
