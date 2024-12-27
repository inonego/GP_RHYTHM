using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    public IEnumerator DoPlay(Chart chart)
    {
        yield return null;

        Processor.Instance.Play(chart);
    }

    public void Play(Chart chart)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("MainScene");

        if (async.isDone)
        {
            StartCoroutine(DoPlay(chart));
        }
    }
}
