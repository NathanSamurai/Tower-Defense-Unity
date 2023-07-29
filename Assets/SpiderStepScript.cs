using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderStepScript : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event spiderStep;

    public void SpiderStep()
    {
        spiderStep.Post(gameObject);
    }
}
