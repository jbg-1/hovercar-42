using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerIcons : MonoBehaviour
{
  public Sprite playerIconSprite;
  public int playerIconWidth = 300;
  public int playerIconHeight = 300;
  private CarController[] carsList;

  void Update()
  {
    if (carsList != null)
    {
      carsList = FindObjectsOfType<CarController>();

      foreach (CarController car in carsList)
      {
        UpdatePlayerIcon(car);
      }
    }
  }

  public void Init()
  {
    carsList = FindObjectsOfType<CarController>();

    foreach (CarController car in carsList)
    {
      CreatePlayerIconForCar(car);
    }
  }

  private GameObject CreatePlayerIconForCar(CarController car)
  {
    Color iconColor;

    try
    {
      iconColor = ToColor(car.playerColor.Value.ToString());
    }
    catch
    {
      //Debug.LogWarning("Invalid or missing color for car: " + car.OwnerClientId.ToString() + ". Skipping icon creation.");
      return null;
    }

    GameObject playerIcon = new GameObject("PlayerIcon_" + car.playerColor.Value + "_" + car.OwnerClientId.ToString());
    playerIcon.layer = LayerMask.NameToLayer("PlayerIcons");

    playerIcon.transform.position = new Vector3(car.transform.position.x, car.transform.position.y + 500, car.transform.position.z);
    playerIcon.transform.rotation = Quaternion.Euler(90, 0, 0);
    playerIcon.transform.localScale = new Vector3(playerIconWidth, playerIconHeight, 0);
    playerIcon.transform.parent = transform;

    var spriteRenderer = playerIcon.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = playerIconSprite;
    spriteRenderer.color = iconColor;

    car.hasPlayerIcon = true;

    return playerIcon;
  }

  private void UpdatePlayerIcon(CarController car)
  {
    if (!car.hasPlayerIcon)
    {
      CreatePlayerIconForCar(car);
      return;
    }

    GameObject playerIcon = transform.Find("PlayerIcon_" + car.playerColor.Value + "_" + car.OwnerClientId.ToString()).gameObject;
    playerIcon.transform.position = new Vector3(car.transform.position.x, car.transform.position.y + 500, car.transform.position.z);
    playerIcon.transform.rotation = Quaternion.Euler(90, 0, 0);
  }

  private Color ToColor(string color)
  {
    return (Color)typeof(Color).GetProperty(color.ToLowerInvariant()).GetValue(null, null);
  }
}
