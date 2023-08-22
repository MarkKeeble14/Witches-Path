using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Circle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject foreground, background, approach; // Circle objects
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

    private float timer;
    private bool isMousedOver;
    private bool active;

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
        timer = 0;
        approachCV.alpha = 0;
        mainCV.alpha = 1;
        approach.transform.localScale = approachStartingScale;
        isMousedOver = false;
    }

    public void Set()
    {
        Vector3 randomScreenPos = new Vector3(Random.Range(0 + screenBufferHorizontal, Screen.width - screenBufferHorizontal),
            Random.Range(0 + screenBufferVertical, Screen.height - screenBufferVertical), 0);
        transform.SetAsFirstSibling();
        transform.position = randomScreenPos;
        active = true;
    }

    // Main Update
    private void Update()
    {
        // Increase timer
        timer += Time.deltaTime;

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
        CombatManager._Instance.OnNoteHit();
        StartCoroutine(EndRoutine());
    }

    private void OnFail()
    {
        // Debug.Log("Fail");
        CombatManager._Instance.OnNoteMiss();
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