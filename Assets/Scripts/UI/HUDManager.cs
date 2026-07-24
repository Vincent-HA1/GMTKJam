using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI fragmentAmountText;
    [SerializeField] Transform heartsParent;
    //[SerializeField] Transform bigCoinsParent;
    [SerializeField] GameObject heartPrefab;
    [SerializeField] Slider cooldownSlider;
    //[SerializeField] GameObject bigCoinIndicatorPrefab;


    List<Heart> hearts = new List<Heart>();
    //List<BigCoinIndicator> bigCoins = new List<BigCoinIndicator>();

    //Run from the level manager. Fills the UI
    public void InitialiseUI(float playerMaxHealth = 3, float startingFragments = 0)
    {
        //Inintiailsie the hearts
        for (int i = 0; i < playerMaxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsParent);
            hearts.Add(heart.GetComponent<Heart>());
        }
        ////initialise the big coins.
        //for (int i = 0; i < bigCoinsFound.Count; i++)
        //{
        //    BigCoinIndicator bigCoin = Instantiate(bigCoinIndicatorPrefab, bigCoinsParent).GetComponent<BigCoinIndicator>();
        //    bigCoins.Add(bigCoin);
        //    //If this big coin was found, show it as filled on the HUD
        //    if (bigCoinsFound[i] == 1)
        //    {
        //        bigCoin.SetFound();
        //    }
        //}
        UpdateFragmentAmount(startingFragments);
    }


    //Called when the player's health changes
    public void UpdateHealthAmount(float newHealth)
    {
        //So check the hearts amount. Go through each heart and manually fill or unfill it
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < newHealth)
            {
                hearts[i].SetFilled();
            }
            else
            {
                hearts[i].SetEmpty();
            }
        }
    }

    //public void UpdateBigCoinIndicator(int index)
    //{
    //    bigCoins[index].SetFound();
    //}

    public void UpdateFragmentAmount(float fragmentAmount)
    {
        string fragmentAmountString = fragmentAmount.ToString();
        string finalString = "";
        for (int i = 0; i < fragmentAmountString.Length; i++)
        {
            /*
            finalString += $"<sprite index={fragmentAmountString[i]}>";
            */
            finalString += $"{ fragmentAmountString[i]}";
        }
        fragmentAmountText.text = finalString;
    }

    public void UpdateCooldown(float cooldown)
    {
        cooldownSlider.value = cooldownSlider.maxValue - cooldown;
    }
}
