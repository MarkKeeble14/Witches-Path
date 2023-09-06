using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLayoutGroup : MonoBehaviour
{
    [SerializeField] private float archHeight = 1;
    [SerializeField] private float spaceBetweenElements = 50;
    [SerializeField] private float rotationPerX = -.1f;
    [SerializeField] private Vector2 childSize;

    public void AddChild(Transform t)
    {
        (t.transform as RectTransform).sizeDelta = childSize;
        t.SetParent(transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount % 2 == 1)
        {
            int centerPoint = transform.childCount / 2;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform childRect = transform.GetChild(i) as RectTransform;
                childRect.localPosition = new Vector2(spaceBetweenElements * (i - centerPoint), -Mathf.Abs(i - centerPoint) * archHeight);
                childRect.localEulerAngles = new Vector3(0, 0, childRect.localPosition.x * rotationPerX);
            }
        }
        else
        {
            float leftCenter = Mathf.FloorToInt(transform.childCount / 2) - .5f;
            float rightCenter = Mathf.CeilToInt(transform.childCount / 2) - .5f;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform childRect = transform.GetChild(i) as RectTransform;
                if (i <= leftCenter)
                {
                    childRect.localPosition = new Vector2(spaceBetweenElements * (i - leftCenter), -Mathf.Abs(i - leftCenter) * archHeight);
                }
                else if (i >= rightCenter)
                {
                    childRect.localPosition = new Vector2(spaceBetweenElements * (i - rightCenter), -Mathf.Abs(i - rightCenter) * archHeight);
                }
                childRect.localEulerAngles = new Vector3(0, 0, childRect.localPosition.x * rotationPerX);
            }
        }
    }
}
