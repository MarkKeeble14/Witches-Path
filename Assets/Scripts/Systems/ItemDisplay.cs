using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;

public class ItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI additionalText;

    [Header("Animations")]
    [SerializeField] private float maxScale;
    [SerializeField] private float minScale;
    [SerializeField] private float scaleSpeed;
    private float scaleTo;
    private bool poppingScale;
    private bool isMousedOver;

    private PowerupItem setTo;
    private GameObject spawnedToolTip;

    private void Start()
    {
        scaleTo = minScale;
    }

    protected void Update()
    {
        // Update additional text if neccessary
        string itemAdditionalText = setTo.GetAdditionalText();
        if (itemAdditionalText.Length > 0)
        {
            additionalText.text = itemAdditionalText;
        }
        else
        {
            additionalText.gameObject.SetActive(false);
        }

        if (isMousedOver)
        {
            scaleTo = maxScale;
        }
        else
        {
            if (!poppingScale &&
                scaleTo > minScale)
            {
                scaleTo -= Time.deltaTime * scaleSpeed;
                if (scaleTo < minScale)
                    scaleTo = minScale;
            }
        }
        Vector3 targetScale = Vector3.one * scaleTo;
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        if (poppingScale && transform.localScale == targetScale)
        {
            poppingScale = false;
        }
    }

    public void AnimateScale()
    {
        scaleTo = maxScale;
        poppingScale = true;
    }

    public virtual void SetItem(PowerupItem i)
    {
        // Set item
        setTo = i;

        // Set fundamentals
        image.sprite = i.GetSprite();
        text.text = i.Name;

        // Enable additional text object if the represented item has such
        additionalText.gameObject.SetActive(i.GetAdditionalText().Length > 0);
    }

    public void SpawnToolTip()
    {
        switch (setTo)
        {
            case Artifact artifact:
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(artifact, transform);
                break;
            case Book book:
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(book, transform);
                break;
        }
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SpawnToolTip();

        isMousedOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();

        isMousedOver = false;
    }
}
