using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceLocalPosition : MonoBehaviour
{
    [SerializeField] private List<Vector3> bounceBetween = new List<Vector3>();
    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private Vector3 goalPos;
    [SerializeField] private float speed;

    private void Awake()
    {
        foreach (Vector3 v3 in bounceBetween)
        {
            positionQueue.Enqueue(v3);
        }
        goalPos = positionQueue.Dequeue();
        positionQueue.Enqueue(goalPos);
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, goalPos, Time.deltaTime * speed);
        if (transform.localPosition == goalPos)
        {
            goalPos = positionQueue.Dequeue();
            positionQueue.Enqueue(goalPos);
        }
    }
}
