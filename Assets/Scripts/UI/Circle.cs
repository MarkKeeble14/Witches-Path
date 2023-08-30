using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Circle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image foreground, background, approach; // Circle objects
    [SerializeField] private Image main;
    private RectTransform mainTransform;
    [SerializeField] private CanvasGroup approachCV;
    [SerializeField] private CanvasGroup mainCV;
    [SerializeField] private float fadeOutMainCVRate;

    [SerializeField] private float minApproachScale = 0.9f;
    [SerializeField] private float maxAcceptedApproachScale = 1.05f;
    [SerializeField] private Vector3 approachScaleRate;
    [SerializeField] private float approachAlphaChangeRate;

    [SerializeField] private Vector3 approachStartingScale;

    [SerializeField] private float screenBufferHorizontal;
    [SerializeField] private float screenBufferVertical;

    private bool isMousedOver;
    private bool active;

    private void Awake()
    {
        // Get Reference
        mainTransform = main.transform as RectTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMousedOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMousedOver = false;
    }

    public void ResetCircle()
    {
        approachCV.alpha = 0;
        mainCV.alpha = 1;
        approach.transform.localScale = approachStartingScale;
        isMousedOver = false;
    }

    private void RandomizeScreenPosition(ScreenQuadrant setToQuadrant)
    {
        Vector3 position;
        switch (setToQuadrant)
        {
            case ScreenQuadrant.BottomLeft:
                position = new Vector3(Random.Range(0 + screenBufferHorizontal, Screen.width / 2),
                    Random.Range(0 + screenBufferVertical, Screen.height / 2), 0);
                break;

            case ScreenQuadrant.BottomRight:
                position = new Vector3(Random.Range(Screen.width / 2, Screen.width - screenBufferHorizontal),
                    Random.Range(0 + screenBufferVertical, Screen.height / 2), 0);
                break;

            case ScreenQuadrant.TopLeft:
                position = new Vector3(Random.Range(0 + screenBufferHorizontal, Screen.width / 2),
                    Random.Range(Screen.height / 2, Screen.height - screenBufferVertical), 0);
                break;

            case ScreenQuadrant.TopRight:
                position = new Vector3(Random.Range(Screen.width / 2, Screen.width - screenBufferHorizontal),
                    Random.Range(Screen.height / 2, Screen.height - screenBufferVertical), 0);
                break;

            default:
                position = new Vector3(Random.Range(0 + screenBufferHorizontal, Screen.width - screenBufferHorizontal),
                    Random.Range(0 + screenBufferVertical, Screen.height - screenBufferVertical), 0);
                break;

        }

        transform.SetAsFirstSibling();
        transform.position = position;
    }

    public void Set(ScreenQuadrant setToQuadrant, Color c)
    {
        RandomizeScreenPosition(setToQuadrant);
        active = true;
        main.color = c;
    }

    // Main Update
    private void Update()
    {
        if (!active) return;

        // Approach Circle modifier
        if (approach.transform.localScale.x >= minApproachScale)
        {
            approachCV.alpha = Mathf.MoveTowards(approachCV.alpha, 1, Time.deltaTime * approachAlphaChangeRate);
            approach.transform.localScale -= approachScaleRate * Time.deltaTime;

            // Check if mouse is over the circle within a range of it being at it's smallest, if so we consider it a pass
            if (approach.transform.localScale.x <= maxAcceptedApproachScale && isMousedOver)
            {
                OnHit();
            }
        }
        else
        {
            // We are under the minApproachScale, consider it a fail
            OnFail();
        }
    }

    private void OnHit()
    {
        // Debug.Log("Hit");
        CombatManager._Instance.OnNoteHit(mainTransform);
        StartCoroutine(EndRoutine());
    }

    private void OnFail()
    {
        // Debug.Log("Fail");
        CombatManager._Instance.OnNoteMiss(mainTransform);
        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        active = false;

        while (mainCV.alpha > 0)
        {
            mainCV.alpha = Mathf.MoveTowards(mainCV.alpha, 0, Time.deltaTime * fadeOutMainCVRate);
            yield return null;
        }

        CombatManager._Instance.ReleaseCircle(this);
    }

    public void Cancel()
    {
        active = false;
        StartCoroutine(EndRoutine());
    }
}