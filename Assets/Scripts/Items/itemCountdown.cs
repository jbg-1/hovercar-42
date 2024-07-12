using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCountdown : MonoBehaviour
{
    public static ItemCountdown instance;
    private float currentTime;
    [SerializeField] private TMP_Text text;
    public static event OnUseItem onUseItem;
    public bool isCountingDown = false;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isCountingDown)
        {
            return;
        }
        currentTime -= Time.deltaTime;
        if (currentTime < 0)
        {
            text.text = "";
            onUseItem();
            isCountingDown = false;
        }
        else
        {
            text.text = currentTime.ToString("0");  
        }
    }

    public void StartCountdown(float time)
    {
        currentTime = time;
        isCountingDown = true;
    }

    public delegate void OnUseItem();

}
