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

        Processor.Instance.Play(Chart);
    }
}
