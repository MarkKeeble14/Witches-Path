using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShowSpellStatChangeDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image statIcon;
    [SerializeField] private Image signIcon;
    [SerializeField] private CanvasGroup cv;

    [SerializeField] private float fadeSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 moveVector;
    [SerializeField] private float waitDuration;

    private Action onDestroyAction;

    public void Set(SpellStat stat, Sign sign, Action callOnDestroy)
    {
        statIcon.sprite = UIManager._Instance.GetSpellStatIcon(stat);
        signIcon.sprite = UIManager._Instance.GetSignIcon(sign);
        StartCoroutine(Lifetime());

        onDestroyAction += callOnDestroy;
    }

    private void Update()
    {
        transform.localPosition += moveVector * moveSpeed * Time.deltaTime;
    }

    private IEnumerator Lifetime()
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, 1, fadeSpeed));
        yield return new WaitForSeconds(waitDuration);
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, 0, fadeSpeed));

        onDestroyAction?.Invoke();
        Destroy(gameObject);
    }
}
