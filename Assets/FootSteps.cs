using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event footStep;
	
    void AnubisStep(){
        footStep.Post(gameObject);
    }

}
