using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArtifactDisplay : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;

    private float targetScale;

    [SerializeField] private float changeScaleSpeed = 1f;

    private void Start()
    {
        targetScale = regularScale;
    }

    private void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Scale
        image.transform.localScale = targetScale * Vector3.one;
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public void SetSprite(Sprite s)
    {
        image.sprite = s;
    }

    public void SetText(string s)
    {
        text.text = s;
    }
}
