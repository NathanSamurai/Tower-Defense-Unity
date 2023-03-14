using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBounceRot : MonoBehaviour
{
    [Tooltip("Rotation speed.")]
    public float rotSpeed = 2.0f;
    public float timeAdjust = 1.0f;

    void Update()
    {
        float angle = Mathf.Sin(Time.time * timeAdjust) * (rotSpeed * 10);

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }
}
