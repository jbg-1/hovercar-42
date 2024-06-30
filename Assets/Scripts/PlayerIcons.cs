using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIcons : MonoBehaviour
{
  public Sprite playerIconSprite;
  private CarController[] cars;
  private int updateCounter = 0;

  void Update()
  {

    cars = FindObjectsOfType<CarController>();

    if (cars.Length > 0)
    {
      if (updateCounter < 1)
      {
        foreach (CarController car in cars)
        {
          CreatePlayerIconForCar(car);
        }
      }

      if (updateCounter > 1)
      {
        foreach (CarController car in cars)
        {
          UpdatePlayerIcon(car);
        }
      }

      updateCounter++;
    }

  }

  private void CreatePlayerIconForCar(CarController car)
  {
    GameObject playerIcon = new GameObject();

    playerIcon.name = "PlayerIcon_" + car.carSettings.playerColor.ToString();
    playerIcon.layer = LayerMask.NameToLayer("PlayerIcons");

    playerIcon.transform.position = car.transform.position;
    playerIcon.transform.rotation = Quaternion.Euler(90, 0, 0);
    playerIcon.transform.localScale = new Vector3(150, 150, 150);
    playerIcon.transform.parent = transform;

    playerIcon.AddComponent<SpriteRenderer>().sprite = playerIconSprite;
    playerIcon.GetComponent<SpriteRenderer>().color = ToColor(car.carSettings.playerColor);
  }

  private void UpdatePlayerIcon(CarController car)
  {
    GameObject playerIcon = transform.Find("PlayerIcon_" + car.carSettings.playerColor.ToString()).gameObject;

    playerIcon.transform.position = car.transform.position;
    playerIcon.transform.rotation = Quaternion.Euler(90, 0, 0);
  }

  private Color ToColor(string color)
  {
    return (Color)typeof(Color).GetProperty(color.ToLowerInvariant()).GetValue(null, null);
  }
}
