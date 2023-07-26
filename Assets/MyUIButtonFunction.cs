using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUIButtonFunction : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event uiClick;
    [SerializeField] AK.Wwise.Event uiHover;
    [SerializeField] AK.Wwise.Event uiNegative;
    [SerializeField] AK.Wwise.Event uiBuy;
    [SerializeField] AK.Wwise.Event uiSell;
    [SerializeField] AK.Wwise.Event hpLose;

    public void UIClick()
    {
        uiClick.Post(gameObject);
    }

    public void UIHover()
    {
        uiHover.Post(gameObject);
    }

    public void UINegative()
    {
        uiNegative.Post(gameObject);
    }

    public void UIBuy()
    {
        uiBuy.Post(gameObject);
    }

    public void UISell()
    {
        uiSell.Post(gameObject);
    }

    public void HPLose()
    {
        hpLose.Post(gameObject);
    }
}
