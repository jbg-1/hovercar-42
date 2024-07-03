using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerIcons : MonoBehaviour
{
  public Sprite playerIconSprite;
  public int playerIconWidth = 300;
  public int playerIconHeight = 300;
  private CarController[] cars;

  // void Update()
  // {

  //   cars = FindObjectsOfType<CarController>();

  //   if (cars.Length > 0)
  //   {
  //     if (updateCounter < 1)
  //     {
  //       foreach (CarController car in cars)
  //       {
  //         CreatePlayerIconForCar(car);
  //         DeleteEmptyPlayerIconForCar(car); // fix because the first player icon is empty but not skippable
  //       }
  //     }

  //     if (updateCounter > 1)
  //     {
  //       foreach (CarController car in cars)
  //       {
  //         UpdatePlayerIcon(car);
  //       }
  //     }

  //     updateCounter++;
  //   }

  // }

  void Update()
  {
    if (cars != null)
    {
      foreach (CarController car in cars)
      {
        UpdatePlayerIcon(car);
      }
    }
  }

  private GameObject CreatePlayerIconForCar(CarController car)
  {
    GameObject playerIcon = new GameObject();

    playerIcon.name = "PlayerIcon_" + car.carSettings.playerColor.ToString();
    playerIcon.layer = LayerMask.NameToLayer("PlayerIcons");

    playerIcon.transform.position = car.transform.position;
    playerIcon.transform.rotation = Quaternion.Euler(90, 0, 0);
    playerIcon.transform.localScale = new Vector3(playerIconWidth, playerIconHeight, 0);
    playerIcon.transform.parent = transform;

    playerIcon.AddComponent<SpriteRenderer>().sprite = playerIconSprite;
    playerIcon.GetComponent<SpriteRenderer>().color = ToColor(car.carSettings.playerColor);

    return playerIcon;
  }

  private void UpdatePlayerIcon(CarController car)
  {
    GameObject playerIcon = transform.Find("PlayerIcon_" + car.carSettings.playerColor.ToString()).gameObject;

    playerIcon.transform.position = car.transform.position;
    playerIcon.transform.rotation = Quaternion.Euler(90, 0, 0);
  }

  private void DeleteEmptyPlayerIconForCar(CarController car)
  {
    GameObject playerIcon = transform.Find("PlayerIcon_" + car.carSettings.playerColor.ToString()).gameObject;
    Destroy(playerIcon);
  }
  private Color ToColor(string color)
  {
    return (Color)typeof(Color).GetProperty(color.ToLowerInvariant()).GetValue(null, null);
  }

  [ServerRpc(RequireOwnership = false)]
  public void NotifyJoinServerRpc()
  {
    NotifyJoinClientRpc();
  }

  [ClientRpc]
  private void NotifyJoinClientRpc()
  {
    if (cars == null)
    {
      cars = FindObjectsOfType<CarController>();
      foreach (CarController car in cars)
      {
        CreatePlayerIconForCar(car);
      }
    }
    else
    {
      foreach (CarController car in cars)
      {
        DeleteEmptyPlayerIconForCar(car);
      }
      cars = null;
      cars = FindObjectsOfType<CarController>();
      foreach (CarController car in cars)
      {
        CreatePlayerIconForCar(car);
      }
    }
  }
}
