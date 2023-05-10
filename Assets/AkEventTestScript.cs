using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkEventTestScript : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event Ciaone;

    // Start is called before the first frame update
    void Start()
    {
        Ciaone.Post(gameObject);
    }
}
