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
    /// 현재 입력에 따른 노트의 점수를 계산합니다.
    /// </summary>
    /// <param name="inputTime"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public float CalculateScore(double inputTime, Note note)
    {
        // 노트 시간 기준 시간 차이입니다.
        double delta = inputTime - note.Time;

        return Mathf.Lerp(100f, 0f, (float)((Math.Clamp(Math.Abs(delta), PerfectTime, JudgeTime) - PerfectTime) / (JudgeTime - PerfectTime)));
    }

    /// <summary>
    /// 노트가 Break나 Miss 되지 않았는지 확인합니다.
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
    /// 롱 노트가 Clear 되는 기준 시간을 반환합니다.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetLongClearedCriteriaTime(Note note)
    {
        // 노트의 끝에서 JudgeTime만큼의 여유를 가지는 시간이 기준이 됩니다.
        return note.Time + note.Duration - JudgeTime;
    }

    /// <summary>
    /// 노트가 Break 되는 기준 최소 시간을 반환합니다.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetMinBreakedCriteriaTime(Note note)
    {
        // BreakTime만큼 더 일찍 누르는 시간이 기준이 됩니다.
        return note.Time - BreakTime;
    }

    /// <summary>
    /// 노트가 Break 되는 기준 최대 시간을 반환합니다.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetMaxBreakedCriteriaTime(Note note)
    {
        // JudgeTime만큼 더 일찍 누르는 시간이 기준이 됩니다.
        return note.Time - JudgeTime;
    }

    /// <summary>
    /// 노트가 Miss 되는 기준 시간을 반환합니다.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public double GetMissedCriteriaTime(Note note)
    {
        // MissTime만큼 더 늦게 누르는 시간이 기준이 됩니다.
        return note.Time + MissTime;
    }

    /// <summary>
    /// 롱 노트가 Clear 되었는지 확인합니다.
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
    /// 노트가 Break 되었는지 확인합니다.
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
    /// 노트가 Miss 되었는지 확인합니다.
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
