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

    private bool resolved;

    public void Resolve()
    {
        resolved = true;
    }

    public void AddReward(ArtifactLabel label)
    {
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(label.ToString(), null, delegate
        {
            GameManager._Instance.AddArtifact(label);
            Destroy(spawned.gameObject);
        });
    }

    public void AddReward(BookLabel label)
    {
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(label.ToString(), null, delegate
        {
            GameManager._Instance.AddBook(label);
            Destroy(spawned.gameObject);
        });
    }

    public void AddReward(PotionIngredient label)
    {
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(label.ToString(), null, delegate
        {
            GameManager._Instance.AddPotionIngredient(label);
            Destroy(spawned.gameObject);
        });
    }

    public void AddReward(int currencyAmount)
    {
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(currencyAmount.ToString() + " Gold", null, delegate
        {
            GameManager._Instance.AlterCurrency(currencyAmount);
            Destroy(spawned.gameObject);
        });
    }

    public IEnumerator ShowRewardScreen()
    {
        rewardScreen.SetActive(true);

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
