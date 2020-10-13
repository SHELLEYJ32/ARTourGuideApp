using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    public GameObject creditsPanel;

    public void ShowOrHideCredits()
    {
        if (creditsPanel.activeSelf)
            creditsPanel.SetActive(false);
        else
            creditsPanel.SetActive(true);
    }
}
