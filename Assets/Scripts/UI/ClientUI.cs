using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ClientUI : MonoBehaviour
{
    [SerializeField] Image image;

    public void Setup(LevelDescription levelDescription)
    {
        image.sprite = levelDescription.thumbnail;
    }

}
