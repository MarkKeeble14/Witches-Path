using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTransform : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Vector3 axis;

    // Update is called once per frame
    void Update()
    {
        // Rotoate
        transform.localEulerAngles += axis * rotateSpeed * Time.deltaTime;
    }
}
