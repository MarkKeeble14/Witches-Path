using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBookButton : MonoBehaviour
{
    [SerializeField] private GameObject toSetInactive;

    private GameObject spawnedToolTip;

    public void CheckIfShouldBeActive()
    {
        Book currentBook = GameManager._Instance.GetOwnedBook(0);

        if (!currentBook.CanLevelUp)
        {
            toSetInactive.SetActive(false);
            return;
        }
    }

    public void SpawnToolTip()
    {
        // Get current book
        Book currentBook = GameManager._Instance.GetOwnedBook(0);

        // Get a new book of the same type
        Book upgradedBook = GameManager._Instance.GetBookOfType(currentBook.GetLabel());

        // Level that book up to the current books level, then one level more
        int timesToLevel = currentBook.GetCurrentLevel();
        for (int i = 0; i < timesToLevel; i++)
        {
            upgradedBook.TryCallLevelUp();
        }

        // The new book is what we use for the comparison tooltip
        spawnedToolTip = UIManager._Instance.SpawnComparisonToolTips(
            new ToolTippableComparisonData[]
                {
                    new ToolTippableComparisonData("Current: ", currentBook),
                    new ToolTippableComparisonData("Upgraded: ", upgradedBook)
                },
            transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
