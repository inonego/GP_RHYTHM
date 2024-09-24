using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI CurrentComboUI;
    public TextMeshProUGUI CurrentProcessScoreUI;

    void Update()
    {
        CurrentComboUI.text = Processor.Instance.CurrentCombo.ToString();
        CurrentProcessScoreUI.text = Processor.Instance.CurrentProcessScore.ToString();
    }
}
