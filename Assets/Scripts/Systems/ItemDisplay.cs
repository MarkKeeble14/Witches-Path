using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI additionalText;

    [Header("Animations")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    private float targetScale;
    [SerializeField] private float changeScaleSpeed = 1f;

    private PowerupItem setTo;
    private GameObject spawnedToolTip;

    private void Awake()
    {
        // Set Target Scale Initially
        targetScale = regularScale;

    }

    protected void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Transforms Actual Scale Scale
        image.transform.localScale = targetScale * Vector3.one;

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
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
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
