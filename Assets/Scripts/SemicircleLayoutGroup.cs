using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemicircleLayoutGroup : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private float maxAngle = 180;
    [SerializeField] private float initialAngle;
    [SerializeField] private bool reverseOrder;

    [Header("Animating Movement")]
    [SerializeField] private bool animate;
    [SerializeField] private float animateSpeed;

    public void SetRadius(float radius)
    {
        this.radius = radius;
    }

    public void SetChildOrder(Transform child)
    {
        if (reverseOrder)
        {
            child.SetAsFirstSibling();
        }
    }

    private void Update()
    {
        int numChildren = transform.childCount;
        float increment = maxAngle / numChildren;

        for (int i = 0; i < numChildren; i++)
        {
            float angle = initialAngle + i * increment + increment / 2;
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

            Vector3 pos = new Vector3(x, y, 0);

            if (animate)
            {
                Vector3 goalPos = transform.position + pos;
                Transform moving = transform.GetChild(i);
                moving.position = Vector3.Lerp(moving.position, goalPos, animateSpeed * Time.deltaTime);
            }
            else
            {
                transform.GetChild(i).position = transform.position + pos;
            }
        }
    }
}
