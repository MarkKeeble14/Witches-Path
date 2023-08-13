using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager _Instance { get; private set; }
    private void Awake()
    {
        _Instance = this;
    }

    [SerializeField] private RewardDisplay simpleRewardDisplay;
    [SerializeField] private Transform rewardList;
    [SerializeField] private GameObject rewardScreen;

    [SerializeField] private float toolTipOffset = 50;

    private bool resolved;

    public void Resolve()
    {
        resolved = true;
    }

    public void AddReward(ArtifactLabel label)
    {
        Debug.Log("Adding Artifact Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        ToolTip spawnedToolTip = null;
        string finalizedToolTipText = GameManager._Instance.GetArtifactOfType(label).ToolTipText;
        spawned.Set(label.ToString(), null,
            delegate
            {
                GameManager._Instance.AddArtifact(label);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnToolTip(finalizedToolTipText, spawned.transform, new Vector3(toolTipOffset, 0, 0));
            }, delegate
            {
                Destroy(spawnedToolTip.gameObject);
            }
        );
    }

    public void AddReward(BookLabel label)
    {
        Debug.Log("Adding Book Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        ToolTip spawnedToolTip = null;
        string finalizedToolTipText = GameManager._Instance.GetBookOfType(label).ToolTipText;
        spawned.Set(label.ToString(), null,
            delegate
            {
                GameManager._Instance.SwapBooks(GameManager._Instance.GetOwnedBook(0), label);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnToolTip(finalizedToolTipText, spawned.transform, new Vector3(toolTipOffset, 0, 0));
            }, delegate
            {
                Destroy(spawnedToolTip.gameObject);
            }
        );
    }

    public void AddReward(PotionIngredient label)
    {
        Debug.Log("Adding Potion Ingredient Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(label.ToString(), null, delegate
        {
            GameManager._Instance.AddPotionIngredient(label);
            Destroy(spawned.gameObject);
        }, null, null);
    }

    public void AddCurrencyReward(int currencyAmount)
    {
        if (currencyAmount <= 0)
        {
            return;
        }

        Debug.Log("Adding Currency Reward: " + currencyAmount);

        // Lucky Coin Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.LuckyCoin))
        {
            currencyAmount = Mathf.CeilToInt(currencyAmount * (LuckyCoin.CurrencyMultiplier / 100));
            GameManager._Instance.AnimateArtifact(ArtifactLabel.LuckyCoin);
        }

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(currencyAmount.ToString() + " Gold", null, delegate
        {
            GameManager._Instance.AlterCurrency(currencyAmount);
            Destroy(spawned.gameObject);
        }, null, null);
    }

    public void AddClothierCurrencyReward(int currencyAmount)
    {
        if (currencyAmount <= 0)
        {
            return;
        }

        Debug.Log("Adding Clothier Currency Reward: " + currencyAmount);

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(currencyAmount.ToString() + " Pelt" + (currencyAmount > 1 ? "s" : ""), null, delegate
        {
            GameManager._Instance.AlterClothierCurrency(currencyAmount);
            Destroy(spawned.gameObject);
        }, null, null);
    }

    public IEnumerator ShowRewardScreen()
    {
        Debug.Log("1: " + rewardScreen + ", Reward Screen Active Self: " + rewardScreen.activeSelf);

        rewardScreen.gameObject.SetActive(true);

        Debug.Log("2: " + rewardScreen + ", Reward Screen Active Self: " + rewardScreen.activeSelf);

        yield return new WaitUntil(() => resolved);

        resolved = false;
        ClearRewardsScreen();
        HideRewardScreen();
    }

    public void HideRewardScreen()
    {
        rewardScreen.SetActive(false);
    }

    public void ClearRewardsScreen()
    {
        while (rewardList.childCount > 0)
        {
            Destroy(rewardList.GetChild(0));
        }
    }
}
