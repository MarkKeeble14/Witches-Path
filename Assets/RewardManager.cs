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
    private List<RewardDisplay> spawnedRewards = new List<RewardDisplay>();


    [SerializeField] private Vector2 toolTipOffset;

    private bool resolved;

    public void Resolve()
    {
        resolved = true;
    }

    public void AddReward(ArtifactLabel label)
    {
        Debug.Log("Adding Artifact Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
        Artifact artifact = GameManager._Instance.GetArtifactOfType(label);
        spawned.Set(label.ToString(), null,
            delegate
            {
                GameManager._Instance.AddArtifact(label);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnToolTips(artifact, spawned.transform);
            }, delegate
            {
                Destroy(spawnedToolTip.gameObject);
            }
        );
        spawnedRewards.Add(spawned);
    }

    public void AddReward(BookLabel label)
    {
        Debug.Log("Adding Book Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
        Book book = GameManager._Instance.GetBookOfType(label);
        spawned.Set(label.ToString(), null,
            delegate
            {
                GameManager._Instance.SwapBooks(GameManager._Instance.GetOwnedBook(0), label);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnToolTipsForBook(book, spawned.transform);
            }, delegate
            {
                Destroy(spawnedToolTip.gameObject);
            }
        );
        spawnedRewards.Add(spawned);
    }

    public void AddReward(PotionIngredient label)
    {
        Debug.Log("Adding Potion Ingredient Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(label.ToString(), null, delegate
        {
            GameManager._Instance.AddPotionIngredient(label);
            spawnedRewards.Remove(spawned);
            Destroy(spawned.gameObject);
        }, null, null);
        spawnedRewards.Add(spawned);
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
            spawnedRewards.Remove(spawned);
            Destroy(spawned.gameObject);
        }, null, null);
        spawnedRewards.Add(spawned);
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
            spawnedRewards.Remove(spawned);
            Destroy(spawned.gameObject);
        }, null, null);

        spawnedRewards.Add(spawned);
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
        while (spawnedRewards.Count > 0)
        {
            RewardDisplay reward = spawnedRewards[0];
            spawnedRewards.RemoveAt(0);
            Destroy(reward.gameObject);
        }
    }
}
