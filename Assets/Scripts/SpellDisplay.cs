using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public abstract class SpellDisplay : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected Image progressBar;
    [SerializeField] protected Image spellIcon;
    [SerializeField] private Image main;

    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;

    [SerializeField] private Vector2 toolTipOffset;
    private GameObject spawnedToolTip;

    private float targetScale;

    [SerializeField] private float changeScaleSpeed = 1f;

    private void Start()
    {
        targetScale = regularScale;
    }

    public bool IsAvailable => isAvailable;

    protected bool isAvailable = true;

    [SerializeField] private Sprite defaultSprite;

    protected void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Scale
        main.transform.localScale = targetScale * Vector3.one;
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public void Unset()
    {
        nameText.text = "";
        text.text = "";
        spellIcon.sprite = defaultSprite;
        progressBar.fillAmount = 1;
        isAvailable = true;
    }

    public abstract Spell GetSpell();

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnToolTips(GetSpell(), transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip.gameObject);
    }
}
