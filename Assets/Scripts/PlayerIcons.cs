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

    playerIcon.name = "PlayerIcon_" + car.carSettings.playerColor + "_" + car.OwnerClientId.ToString();
    playerIcon.layer = LayerMask.NameToLayer("PlayerIcons");

    Debug.Log("Name: " + playerIcon.name);

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
    GameObject playerIcon = transform.Find("PlayerIcon_" + car.carSettings.playerColor + "_" + car.OwnerClientId.ToString()).gameObject;

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
      Debug.Log("Else Statement" + cars.Length);
      foreach (CarController car in cars)
      {
        Debug.Log("Else Statement", car);
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

  [ServerRpc(RequireOwnership = false)]
  public void AddCarToCarsServerRpc(CarController car)
  {
    if (cars == null)
    {
      cars = new CarController[1];
      cars[0] = car;
    }
    else
    {
      CarController[] tempCars = new CarController[cars.Length + 1];

      for (int i = 0; i < cars.Length; i++)
      {
        tempCars[i] = cars[i];
      }

      tempCars[tempCars.Length - 1] = car;
      cars = tempCars;
    }

    AddCarToCarsClientRpc(cars);
  }

  [ClientRpc]
  private void AddCarToCarsClientRpc(CarController[] cars)
  {
    this.cars = cars;

    foreach (CarController car in cars)
    {
      CreatePlayerIconForCar(car);
    }
  }
}
