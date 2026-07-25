using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeIcon : MonoBehaviour
{

    public Action<UpgradeIcon> BuyUpgrade;
    [Header("Upgrade details")]
    public UpgradeType upgradeType;


    [Header("UI References")]
    [SerializeField] TMPro.TextMeshProUGUI priceText;
    [SerializeField] GameObject priceDescription;
    [SerializeField] GameObject boughtIcon;
    public int upgradeCost;


    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectUpgrade);
        priceText.text = upgradeCost.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SelectUpgrade()
    {
        SetUnlocked();
        BuyUpgrade?.Invoke(this);
    }

    //The upgrade has been unlocked
    public void SetUnlocked()
    {
        //deselect
        button.interactable = false;
        //remove cost
        priceDescription.SetActive(false);
        boughtIcon.SetActive(true);
    }

    public void CheckIfCanBeBought(float fragmentsAmount)
    {
        if (fragmentsAmount < upgradeCost)
        {
            //disable the button
            button.interactable = false;
        }
    }
}
