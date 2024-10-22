using FMOD;
using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public delegate void MusicEvent();

public class AudioManager : MonoSingleton<AudioManager>
{
    private FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();

    private FMOD.Channel musicChannel = new FMOD.Channel();
    private FMOD.Sound music = new FMOD.Sound();

    private TimeCounter preDelayCounter = new TimeCounter();
    private TimeCounter endDelayCounter = new TimeCounter();

    private FMOD.Channel[] channels = new FMOD.Channel[64];
    private FMOD.Sound[] sounds = new FMOD.Sound[32];
     
    public float PreDelayTime = 2.0f;
    public float EndDelayTime = 2.0f;

    public MusicEvent OnMusicEnded;

    protected override void Awake()
    {
        RuntimeManager.CoreSystem.getMasterChannelGroup(out channelGroup);

        musicChannel = new FMOD.Channel();

        musicChannel.setChannelGroup(channelGroup);

        for (int i = 0; i < channels.Length; i++)
        {
            channels[i] = new FMOD.Channel();

            channels[i].setChannelGroup(channelGroup);
        }
    }

    private void Update()
    {
        preDelayCounter.Update();
        endDelayCounter.Update();

        if (preDelayCounter.WasEndedThisFrame())
        {
            PlayMusicDelayed();
        }

        if (endDelayCounter.WasEndedThisFrame())
        {
            OnMusicEnded();
        }
    }

    public FMOD.Sound LoadSound(string path, FMOD.MODE mode = FMOD.MODE.DEFAULT)
    {
        RuntimeManager.CoreSystem.createSound(path, mode, out FMOD.Sound sound);

        return sound;
    }

    public FMOD.Sound LoadSound(AudioClip audioClip, FMOD.MODE mode = FMOD.MODE.DEFAULT)
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

        RuntimeManager.CoreSystem.createSound("", FMOD.MODE.OPENUSER | mode, ref info, out FMOD.Sound sound);

        IntPtr ptr1, ptr2; uint len1, len2;

        sound.@lock(0, lenbytes, out ptr1, out ptr2, out len1, out len2);
        { 
            Marshal.Copy(audioData, 0, ptr1, (int)(len1 / sizeof(float)));

            if (len2 > 0)
            {
                Marshal.Copy(audioData, (int)(len1 / sizeof(float)), ptr2, (int)(len2 / sizeof(float)));
            }
        } 
        sound.unlock(ptr1, ptr2, len1, len2);

        return sound;
    }

    public void Load(int index, FMOD.Sound sound)
    {
        sounds[index] = sound;
    }

    public void Play(int channel, int index)
    {
        Stop(channel);

        RuntimeManager.CoreSystem.playSound(sounds[index], channelGroup, false, out channels[channel]);
    }

    public void Stop(int channel)
    {
        channels[channel].isPlaying(out bool isPlaying);

        if (isPlaying)
        {
            channels[channel].stop();
        }
    }

    public double GetCurrentPlayTime(int channel)
    {
        channels[channel].getPosition(out uint position, FMOD.TIMEUNIT.MS);

        return position * 0.001;
    }

    public void LoadMusic(FMOD.Sound sound)
    {
        music = sound;
    }

    public void PlayMusic()
    {
        StopMusic();

        preDelayCounter.Start(PreDelayTime);
    }

    public void PlayMusicDelayed()
    {
        RuntimeManager.CoreSystem.playSound(music, channelGroup, false, out musicChannel);

        musicChannel.setCallback(OnChannelCallback);
    }

    public void StopMusic()
    {
        musicChannel.isPlaying(out bool isPlaying);

        if (isPlaying)
        {
            musicChannel.stop();
        }

        preDelayCounter.Stop();
        endDelayCounter.Stop();
    }

    public double GetMusicCurrentPlayTime()
    {
        if (endDelayCounter.isWorking)
        {
            music.getLength(out uint length, FMOD.TIMEUNIT.MS);

            return length * 0.001 + endDelayCounter.GetElapsedTime();
        }
        else
        {
            musicChannel.getPosition(out uint position, FMOD.TIMEUNIT.MS);

            return position * 0.001 - preDelayCounter.GetTimeLeft();
        }
    }


    private static FMOD.RESULT OnChannelCallback(IntPtr channelcontrol, FMOD.CHANNELCONTROL_TYPE controltype, FMOD.CHANNELCONTROL_CALLBACK_TYPE callbacktype, IntPtr commanddata1, IntPtr commanddata2)
    {
        if (callbacktype == FMOD.CHANNELCONTROL_CALLBACK_TYPE.END)
        {
            Instance.endDelayCounter.Start(Instance.EndDelayTime);
        }

        return FMOD.RESULT.OK;
    }
}
