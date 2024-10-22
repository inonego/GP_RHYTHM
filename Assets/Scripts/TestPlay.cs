using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlay : MonoBehaviour
{
    public Chart Chart;

    private void Start()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        yield return null;


        // 임시로 랜덤 노트 생성
        {
            Chart.SQListNote[0].Events.Clear();
            Chart.SQListNote[1].Events.Clear();
            Chart.SQListNote[2].Events.Clear();
            Chart.SQListNote[3].Events.Clear();

            Chart.SQSpeedChange.Events.Clear();

            for (int i = 0; i < 1024; i++)
            {
                NoteEvent noteEvent = new NoteEvent();

                noteEvent.Beat = i * 0.5f;

                noteEvent.Duration = UnityEngine.Random.Range(0f, 1f) < 0.8f ? 0f : 0.25f;

                if (UnityEngine.Random.Range(0f, 1f) > 0.7f) Chart.SQListNote[0].Events.Add(noteEvent);
                if (UnityEngine.Random.Range(0f, 1f) > 0.7f) Chart.SQListNote[1].Events.Add(noteEvent);
                if (UnityEngine.Random.Range(0f, 1f) > 0.7f) Chart.SQListNote[2].Events.Add(noteEvent);
            }

            for (int i = 0; i < 256; i++)
            {
                NoteEvent noteEvent = new NoteEvent();

                noteEvent.Beat = i * 4f;
                noteEvent.Duration = 2f;

                Chart.SQListNote[3].Events.Add(noteEvent);
            }
        }

        Processor.Instance.Play(Chart);
    }
}
