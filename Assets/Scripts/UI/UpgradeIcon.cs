using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeIcon : MonoBehaviour
{
    public Action LeftIcon;
    public Action<int, string> ShowUpgrade;
    public Action<UpgradeIcon> BuyUpgrade;
    [Header("Upgrade details")]
    public UpgradeType upgradeType;
    [SerializeField] string upgradeDescription;

    [Header("UI References")]
    [SerializeField] GameObject boughtIcon;
    public int upgradeCost;

    bool upgradeBought = false;
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectUpgrade);
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
        boughtIcon.SetActive(true);
        upgradeBought = true;
        LeaveIcon();
    }

    public void CheckIfCanBeBought(float fragmentsAmount)
    {
        if (fragmentsAmount < upgradeCost)
        {
            //disable the button
            button.interactable = false;
        }
    }

    public void HoverOver()
    {
        if(!upgradeBought) ShowUpgrade?.Invoke(upgradeCost, upgradeDescription);
    }

    public void LeaveIcon()
    {
        LeftIcon?.Invoke();
    }
}
