using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConfirmPotionToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject potionIcon;
    private Potion spawnedFor;
    private GameObject importantParent;

    public void Set(Potion spawnedFor, Transform potionIcon, GameObject importantParent)
    {
        this.spawnedFor = spawnedFor;
        this.potionIcon = potionIcon.gameObject;
        this.importantParent = importantParent;
    }

    public void Confirm()
    {
        spawnedFor.Use();
        Destroy(potionIcon);
    }

    public void Cancel()
    {
        DestroyToolTip();
        Destroy(importantParent);
    }

    private GameObject spawnedToolTip;
    private void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(spawnedFor, transform);
    }

    private void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SpawnToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }
}
