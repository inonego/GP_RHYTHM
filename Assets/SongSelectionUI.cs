using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SongSelectionUI : MonoBehaviour, IPointerDownHandler
{
    public Chart Chart;

    public TextMeshProUGUI SongNameUI;
    public TextMeshProUGUI SongBPMUI;

    public void OnEnable()
    {
        SetChart(Chart);
    }

    public void SetChart(Chart chart)
    {
        Chart = chart;

        SongNameUI.text = chart.Name;
        SongBPMUI.text = chart.InitialBPM.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.Play(Chart);
    }
}
