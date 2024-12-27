using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Visualizer : MonoSingleton<Visualizer>
{
    public TextMeshProUGUI CurrentComboUI;
    public TextMeshProUGUI CurrentProcessScoreUI;
    public TextMeshProUGUI RateUI;
    public TextMeshProUGUI SpeedUI;

    public List<KeyEffect> KeyEffectList = new List<KeyEffect>();
    public List<Animation> NoteEffectList = new List<Animation>();
    public List<KeyEffect> JudgeEffectList = new List<KeyEffect>();

    public Animation ComboAnimation;
    public Animation ProcessScoreAnimation;

    private void Start()
    {
        Processor.Instance.OnInputProcess += OnInputProcess;
        Processor.Instance.OnNoteProcess += OnNoteProcess;
    }

    private void Update()
    {
        if (Processor.Instance.Current != null)
        {
            CurrentComboUI.text = Processor.Instance.Current.CurrentCombo.ToString();
            CurrentProcessScoreUI.text = (Mathf.CeilToInt(Processor.Instance.Current.CurrentProcessScore / 10f) * 10f).ToString();
            RateUI.text = string.Format("{0:#00.00}%", Processor.Instance.Current.Rate);
        }

        SpeedUI.text = string.Format("x {0:#0.0}", NoteManager.Instance.UserSpeed);
    }

    private void OnInputProcess(int index, InputDATA inputDATA)
    {
        bool isKeyPressed = inputDATA.Type == InputType.KeyPressed;

        if (isKeyPressed)
        {
            KeyEffectList[index].Play();
        }
    }

    private void PlayJudgeEffect(double delta)
    {
        int count = JudgeEffectList.Count;

        delta = Math.Clamp(delta, -Judger.Instance.JudgeTime, +Judger.Instance.JudgeTime) / Judger.Instance.JudgeTime;

        int i = Mathf.RoundToInt((float)((delta + 1.0) * 0.5 * (count - 1)));

        JudgeEffectList[i].Play();
    }

    private void OnNoteProcess(int index, NoteDATA noteDATA)
    {
        double delta = noteDATA.JudgeDATA.Value.Time - noteDATA.Note.Time;

        PlayJudgeEffect(delta);

        if (noteDATA.JudgeDATA.Value.Score > 0f)
        {
            NoteEffectList[index].Play();

            ComboAnimation.Play();
            ProcessScoreAnimation.Play();
        }
    }
}
