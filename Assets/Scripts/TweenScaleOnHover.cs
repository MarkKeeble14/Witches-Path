using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TweenScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float tweenScaleOnExit;
    [SerializeField] private float tweenScaleOnHover;
    [SerializeField] private float tweenScaleDuration;

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(tweenScaleOnHover, tweenScaleDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(tweenScaleOnExit, tweenScaleDuration);
    }
}
