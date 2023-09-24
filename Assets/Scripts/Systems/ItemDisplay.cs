using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI additionalText;

    [Header("Animations")]
    [SerializeField] private float onActivateTweenTo;
    [SerializeField] private float onActivateTweenDuration;
    [SerializeField] private float returnScaleSpeed;
    private Tween scaleTween;

    private PowerupItem setTo;
    private GameObject spawnedToolTip;

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

        if (scaleTween != null && !scaleTween.active)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * returnScaleSpeed);
        }
    }

    public void AnimateScale()
    {
        scaleTween = transform.DOScale(onActivateTweenTo, onActivateTweenDuration);
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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }
}
