using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageComponent : MonoBehaviour {

    public GameObject hitFX;
	void DealDamage() {
        transform.parent.GetComponent<DemoController>().DealDamage(this);
    }
	void TweenCamera(string data)
    {
        transform.parent.GetComponent<DemoController>().TweenCamera(data);
    }

}
