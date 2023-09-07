using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLayoutGroup : MonoBehaviour
{
    [SerializeField] private float archHeight = 1;
    [SerializeField] private List<float> numChildSpacing = new List<float>();
    [SerializeField] private float rotationPerX = -.1f;
    [SerializeField] private Vector2 childSize;
    [SerializeField] private bool animatePosition;
    [SerializeField] private bool animateRotation;
    [SerializeField] private float animatePosRate;
    [SerializeField] private float animateRotRate;
    [SerializeField] private float alterBaseLine;
    private List<Transform> inHand = new List<Transform>();

    public void RelinquishControl(Transform t)
    {
        t.SetParent(null);
    }

    public void AddChild(Transform t)
    {
        (t.transform as RectTransform).sizeDelta = childSize;
        t.SetParent(transform, true);
        AddTransformToHand(t);
    }

    public void AddTransformToHand(Transform t)
    {
        inHand.Add(t);
    }

    public void RemoveTransformFromHand(Transform t)
    {
        inHand.Remove(t);
    }

    private void SetPos(RectTransform rect, Vector2 targetPos)
    {
        SetPos(rect, new Vector3(targetPos.x, targetPos.y, 0));
    }

    private void SetPos(RectTransform rect, Vector3 targetPos)
    {
        if (animatePosition)
        {
            rect.localPosition = Vector3.MoveTowards(rect.localPosition, targetPos, Time.deltaTime * animatePosRate);
        }
        else
        {
            rect.localPosition = targetPos;
        }
    }

    private void SetRot(RectTransform rect, Vector3 targetEuler)
    {
        if (animateRotation)
        {
            rect.rotation = Quaternion.Lerp(rect.rotation, Quaternion.Euler(targetEuler), animateRotRate * Time.deltaTime);
        }
        else
        {
            rect.rotation = Quaternion.Euler(targetEuler);
        }
    }

    public void InsertTransformToHand(Transform t, int index)
    {
        inHand.Insert(index, t);
    }

    // Update is called once per frame
    void Update()
    {
        if (inHand.Count % 2 == 1)
        {
            int centerPoint = inHand.Count / 2;
            for (int i = 0; i < inHand.Count; i++)
            {
                RectTransform childRect = inHand[i] as RectTransform;
                SetPos(childRect, new Vector2(numChildSpacing[inHand.Count] * (i - centerPoint), (-Mathf.Abs(i - centerPoint) * archHeight) + alterBaseLine));
                SetRot(childRect, new Vector3(0, 0, childRect.localPosition.x * rotationPerX));
            }
        }
        else
        {
            float leftCenter = Mathf.FloorToInt(inHand.Count / 2) - .5f;
            float rightCenter = Mathf.CeilToInt(inHand.Count / 2) - .5f;
            for (int i = 0; i < inHand.Count; i++)
            {
                RectTransform childRect = inHand[i] as RectTransform;

                if (i <= leftCenter)
                {
                    SetPos(childRect, new Vector2(numChildSpacing[inHand.Count] * (i - leftCenter), (-Mathf.Abs(i - leftCenter) * archHeight) + alterBaseLine));
                }
                else if (i >= rightCenter)
                {
                    SetPos(childRect, new Vector2(numChildSpacing[inHand.Count] * (i - rightCenter), (-Mathf.Abs(i - rightCenter) * archHeight) + alterBaseLine));
                }
                SetRot(childRect, new Vector3(0, 0, childRect.localPosition.x * rotationPerX));
            }
        }
    }
}
