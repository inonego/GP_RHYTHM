using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Judger : MonoSingleton<Judger>
{
    public double MissTime = 0.0;
    public double JudgeTime = 0.0;
    public double BreakTime = 0.0;

    public float CalculateScore(double inputTime, Note note)
    {
        // 노트 시간 기준 시간 차이입니다.
        double delta = inputTime - note.Time;

        return Mathf.Lerp(100f, 0f, (float)((Math.Clamp(Math.Abs(delta), JudgeTime, BreakTime) - JudgeTime) / (BreakTime - JudgeTime)));
    }

    // BreakTime과 MissTime 사이에 있는지 확인합니다.
    public bool CheckInputTimeValid(double inputTime, Note note) 
    {
        // 노트 시간 기준 시간 차이입니다.
        double delta = inputTime - note.Time;

        return -BreakTime <= delta && delta <= MissTime;
    }

    // 롱 노트가 Clear 되는 기준 시간을 반환합니다.
    public double GetLongClearedCriteriaTime(Note note)
    {
        // 노트의 끝에서 JudgeTime만큼의 여유를 가지는 시간이 기준이 됩니다.
        return note.Time + note.Length - JudgeTime;
    }

    // 노트가 Miss 되는 기준 시간을 반환합니다.
    public double GetMissedCriteriaTime(Note note)
    {
        // MissTime만큼 더 늦게 누르는 시간이 기준이 됩니다.
        return note.Time + MissTime;
    }

    // 롱 노트가 Clear 되었는지 확인합니다.
    public bool CheckLongCleared(double currentPlayTime, Note note)
    {
        return GetLongClearedCriteriaTime(note) <= currentPlayTime;
    }

    // 노트가 Miss 되었는지 확인합니다.
    public bool CheckMissed(double currentPlayTime, Note note)
    {
        return GetMissedCriteriaTime(note) <= currentPlayTime;
    }
}
