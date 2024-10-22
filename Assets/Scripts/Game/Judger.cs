using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Judger : MonoSingleton<Judger>
{
    public double Calibration = 0.0;

    [Header("Time")]
    public double PerfectTime = 0.0;
    public double JudgeTime = 0.0;
    public double MissTime = 0.0;
    public double BreakTime = 0.0;

    /// <summary>
    /// ���� �Է¿� ���� ��Ʈ�� ������ ����մϴ�.
    /// </summary>
    /// <param name="inputTime"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public float CalculateScore(double inputTime, Note note)
    {
        // ��Ʈ �ð� ���� �ð� �����Դϴ�.
        double delta = inputTime - note.Time;

        return Mathf.Lerp(100f, 0f, (float)((Math.Clamp(Math.Abs(delta), PerfectTime, JudgeTime) - PerfectTime) / (JudgeTime - PerfectTime)));
    }

    /// <summary>
    /// ��Ʈ�� Break�� Miss ���� �ʾҴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public bool CheckInputTimeValid(double time, Note note) 
    {
        time += Calibration;

        double delta = time - note.Time;

        return -JudgeTime <= delta && delta <= +JudgeTime;
    }

    /// <summary>
    /// �� ��Ʈ�� Clear �Ǵ� ���� �ð��� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetLongClearedCriteriaTime(Note note)
    {
        // ��Ʈ�� ������ JudgeTime��ŭ�� ������ ������ �ð��� ������ �˴ϴ�.
        return note.Time + note.Duration - JudgeTime;
    }

    /// <summary>
    /// ��Ʈ�� Break �Ǵ� ���� �ּ� �ð��� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetMinBreakedCriteriaTime(Note note)
    {
        // BreakTime��ŭ �� ���� ������ �ð��� ������ �˴ϴ�.
        return note.Time - BreakTime;
    }

    /// <summary>
    /// ��Ʈ�� Break �Ǵ� ���� �ִ� �ð��� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetMaxBreakedCriteriaTime(Note note)
    {
        // JudgeTime��ŭ �� ���� ������ �ð��� ������ �˴ϴ�.
        return note.Time - JudgeTime;
    }

    /// <summary>
    /// ��Ʈ�� Miss �Ǵ� ���� �ð��� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetMissedCriteriaTime(Note note)
    {
        // MissTime��ŭ �� �ʰ� ������ �ð��� ������ �˴ϴ�.
        return note.Time + MissTime;
    }

    /// <summary>
    /// �� ��Ʈ�� Clear �Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public bool CheckLongCleared(double time, Note note)
    {
        time += Calibration;

        return GetLongClearedCriteriaTime(note) <= time;
    }

    /// <summary>
    /// ��Ʈ�� Break �Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public bool CheckBreaked(double time, Note note)
    {
        time += Calibration;

        return GetMinBreakedCriteriaTime(note) < time && time < GetMaxBreakedCriteriaTime(note);
    }

    /// <summary>
    /// ��Ʈ�� Miss �Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public bool CheckMissed(double time, Note note)
    {
        time += Calibration;

        return GetMissedCriteriaTime(note) < time;
    }
}
