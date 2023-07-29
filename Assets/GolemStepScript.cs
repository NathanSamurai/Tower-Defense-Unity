using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class GolemStepScript : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event golemStep;

    public void GolemStep()
    {
        golemStep.Post(gameObject);
        Debug.Log("stepgolem");
    }
}
