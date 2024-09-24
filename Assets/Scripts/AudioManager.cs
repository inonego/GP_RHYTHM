using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityCommunity.UnitySingleton;
using UnityEditor;
using UnityEngine;

public enum SoundType
{
    Music
}

public class AudioManager : MonoSingleton<AudioManager>
{
    private FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
    private FMOD.Channel[] channels = new FMOD.Channel[2300];
    private FMOD.Sound[] sounds = new FMOD.Sound[Enum.GetNames(typeof(SoundType)).Length];

    protected override void Awake()
    {
        RuntimeManager.CoreSystem.getMasterChannelGroup(out channelGroup);

        for (int i = 0; i < channels.Length; i++)
        {
            channels[i] = new FMOD.Channel();

            channels[i].setChannelGroup(channelGroup);
        }
    }

    private void OnDestroy()
    {
        ReleaseAll();
    }

    private void Load(SoundType soundType, FMOD.Sound sound)
    {
        Release(soundType);

        sounds[(int)soundType] = sound;
    }

    public void Load(SoundType soundType, string path, FMOD.MODE mode = FMOD.MODE.DEFAULT)
    {
        FMOD.RESULT result = RuntimeManager.CoreSystem.createSound(path, mode, out FMOD.Sound sound);

        Load(soundType, sound);
    }

    public void Load(SoundType soundType, AudioClip audioClip, FMOD.MODE mode = FMOD.MODE.DEFAULT)
    {
        float[] audioData = new float[audioClip.samples * audioClip.channels];
        
        audioClip.GetData(audioData, 0);

        uint lenbytes = (uint)(audioClip.samples * audioClip.channels * sizeof(float));

        FMOD.CREATESOUNDEXINFO info = new FMOD.CREATESOUNDEXINFO();
        info.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        info.length = lenbytes;                        // PCM 데이터 길이
        info.numchannels = audioClip.channels;         // 채널 수
        info.defaultfrequency = audioClip.frequency;   // 샘플링 주파수
        info.format = FMOD.SOUND_FORMAT.PCMFLOAT;      // PCM float 포맷 설정

        FMOD.Sound sound;

        FMOD.RESULT result = RuntimeManager.CoreSystem.createSound("", FMOD.MODE.OPENUSER | mode, ref info, out sound);

        IntPtr ptr1, ptr2; uint len1, len2;

        result = sound.@lock(0, lenbytes, out ptr1, out ptr2, out len1, out len2);
        { 
            Marshal.Copy(audioData, 0, ptr1, (int)(len1 / sizeof(float)));

            if (len2 > 0)
            {
                Marshal.Copy(audioData, (int)(len1 / sizeof(float)), ptr2, (int)(len2 / sizeof(float)));
            }
        } 
        result = sound.unlock(ptr1, ptr2, len1, len2);

        Load(soundType, sound);
    }

    public void Play(int channel, SoundType soundType)
    {
        RuntimeManager.CoreSystem.playSound(sounds[(int)soundType], channelGroup, false, out channels[channel]);
    }

    public void Stop(int channel)
    {
        channels[channel].isPlaying(out bool isPlaying);

        if (isPlaying) channels[channel].stop();
    }

    public void Release(SoundType soundType)
    {
        sounds[(int)soundType].release();
    }

    public void ReleaseAll()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].release();
        }
    }

    public double GetCurrentPlayTime(int channel)
    {
        channels[channel].getPosition(out uint position, FMOD.TIMEUNIT.MS);

        return position * 0.001;
    }
}
