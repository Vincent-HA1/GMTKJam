using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] TMPro.TextMeshProUGUI fragmentsText;
    [SerializeField] TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] TMPro.TextMeshProUGUI upgradeCostText;
    [SerializeField] GameObject description;
    [SerializeField] List<UpgradeIcon> upgrades;
    [SerializeField] Button leaveShop;
    [SerializeField] Animator sceneFadeAnimator;

    SaveData currentSaveData;

    EventSystem currentEventSystem;
    // Start is called before the first frame update
    void Start()
    {
        currentEventSystem = EventSystem.current;
        leaveShop.onClick.AddListener(LeaveShop);
        InitialiseShop();
        StartCoroutine(WaitForSceneFade());
    }

    IEnumerator WaitForSceneFade()
    {
        currentEventSystem.enabled = false;
        yield return new WaitForEndOfFrame();
        //if (movingSpikes) movingSpikes.SetPosition();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        currentEventSystem.enabled = true;
    }

    void InitialiseShop()
    {
        //Load the upgrades
        //Check what is bought and not
        currentSaveData = SaveSystem.Load();
        if (currentSaveData != null)
        {
            foreach (var upgrade in upgrades)
            {
                //Check if they have been bought already
                if (currentSaveData.upgrades.IsUnlocked(upgrade.upgradeType))
                {
                    upgrade.SetUnlocked(); //If so, disable it
                }
                //Bind events
                //on hover and on click
                //if cannot be bought, then
                upgrade.BuyUpgrade += BuyUpgrade;
                upgrade.ShowUpgrade += UpdateDescription;
                upgrade.LeftIcon += HideDescription;
            }
            UpdateBuyableUpgrades();
        }
    }


    // Update is called once per frame
    void Update()
    {
        fragmentsText.text = currentSaveData.fragments.ToString();
    }

    void UpdateDescription(int cost, string description)
    {
        this.description.SetActive(true);
        descriptionText.text = description;
        upgradeCostText.text = cost.ToString();
    }

    void HideDescription()
    {
        description.SetActive(false);
    }
    void BuyUpgrade(UpgradeIcon upgradeToUnlock)
    {
        currentSaveData.upgrades.SetUnlocked(upgradeToUnlock.upgradeType);
        currentSaveData.fragments -= upgradeToUnlock.upgradeCost;
        //Update player stats
        switch (upgradeToUnlock.upgradeType)
        {
            case UpgradeType.Jump:
                currentSaveData.stats.jumpMult = 1.1f;
                break;
            case UpgradeType.Health:
                currentSaveData.stats.healthAdd = 1;
                break;
            case UpgradeType.Speed:
                currentSaveData.stats.speedMult = 1.2f;
                break;
            case UpgradeType.Range:
                currentSaveData.stats.rangeMult = 1.2f;
                break;
        }
        UpdateBuyableUpgrades();
    }

    void UpdateBuyableUpgrades()
    {
        //Update whether each upgrade can be bought after changing fragments
        foreach(var upgrade in upgrades)
        {
            upgrade.CheckIfCanBeBought(currentSaveData.fragments);
        }
    }

    void LeaveShop()
    {
        //go to next stage
        SaveSystem.Save(currentSaveData);
        StartCoroutine(LoadSceneAfterFade(currentSaveData.levelIndex + 1));
        currentEventSystem.enabled = false;
    }


    IEnumerator LoadSceneAfterFade(int sceneIndex)
    {
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(sceneIndex);
    }
}
