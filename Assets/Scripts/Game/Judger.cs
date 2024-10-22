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
        // ��Ʈ �ð� ���� �ð� �����Դϴ�.
        double delta = inputTime - note.Time;

        return Mathf.Lerp(100f, 0f, (float)((Math.Clamp(Math.Abs(delta), JudgeTime, BreakTime) - JudgeTime) / (BreakTime - JudgeTime)));
    }

    // BreakTime�� MissTime ���̿� �ִ��� Ȯ���մϴ�.
    public bool CheckInputTimeValid(double inputTime, Note note) 
    {
        // ��Ʈ �ð� ���� �ð� �����Դϴ�.
        double delta = inputTime - note.Time;

        return -BreakTime <= delta && delta <= MissTime;
    }

    // �� ��Ʈ�� Clear �Ǵ� ���� �ð��� ��ȯ�մϴ�.
    public double GetLongClearedCriteriaTime(Note note)
    {
        // ��Ʈ�� ������ JudgeTime��ŭ�� ������ ������ �ð��� ������ �˴ϴ�.
        return note.Time + note.Length - JudgeTime;
    }

    // ��Ʈ�� Miss �Ǵ� ���� �ð��� ��ȯ�մϴ�.
    public double GetMissedCriteriaTime(Note note)
    {
        // MissTime��ŭ �� �ʰ� ������ �ð��� ������ �˴ϴ�.
        return note.Time + MissTime;
    }

    // �� ��Ʈ�� Clear �Ǿ����� Ȯ���մϴ�.
    public bool CheckLongCleared(double currentPlayTime, Note note)
    {
        return GetLongClearedCriteriaTime(note) <= currentPlayTime;
    }

    // ��Ʈ�� Miss �Ǿ����� Ȯ���մϴ�.
    public bool CheckMissed(double currentPlayTime, Note note)
    {
        return GetMissedCriteriaTime(note) <= currentPlayTime;
    }
}
