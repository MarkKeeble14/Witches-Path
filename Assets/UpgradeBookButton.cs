using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBookButton : MonoBehaviour
{
    [SerializeField] private GameObject toSetInactive;

    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        Book currentBook = GameManager._Instance.GetOwnedBook(0);

        if (!currentBook.CanLevelUp)
        {
            toSetInactive.SetActive(false);
            return;
        }

        Book newBook = GameManager._Instance.GetBookOfType(currentBook.GetLabel());

        int timesToLevel = currentBook.GetCurrentLevel() + 1;
        for (int i = 0; i < timesToLevel; i++)
        {
            newBook.TryCallLevelUp();
        }

        spawnedToolTip = UIManager._Instance.SpawnToolTips(newBook, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
