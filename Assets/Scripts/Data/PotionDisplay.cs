using System;
using UnityEngine;
using UnityEngine.UI;

public class PotionDisplay : MonoBehaviour
{
    [SerializeField] private Image icon;

    private GameObject spawnedToolTip;

    private Potion representingPotion;

    private Action onPress;
    public void OnPress()
    {
        onPress?.Invoke();
    }

    public void Set(Potion p, Action onPress)
    {
        representingPotion = p;
        this.onPress += onPress;
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representingPotion, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
