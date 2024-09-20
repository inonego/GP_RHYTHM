using FMOD;
using System;
using System.Runtime.InteropServices;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class AudioManager : PersistentMonoSingleton<AudioManager>
{
    private FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
    private FMOD.Channel[] channels = new FMOD.Channel[2300];
    private FMOD.Sound[] sounds = new FMOD.Sound[20];

    private void Start()
    {
        FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out channelGroup);

        for (int i = 0; i < channels.Length; i++)
        {
            channels[i] = new FMOD.Channel();

            channels[i].setChannelGroup(channelGroup);
        }
    }

    private void OnApplicationQuit()
    {
        ReleaseAll();
    }

    public void Load(int index, string path, FMOD.MODE mode = FMOD.MODE.CREATESAMPLE)
    {
        FMOD.RESULT result = FMODUnity.RuntimeManager.CoreSystem.createSound(path, mode, out FMOD.Sound sound);

        UnityEngine.Debug.Log(result);

        sounds[index] = sound;
    }

    public void Load(int index, AudioClip clip, FMOD.MODE mode = FMOD.MODE.CREATESAMPLE)
    {
        float[] samples = new float[clip.samples * clip.channels];

        clip.GetData(samples, 0);

        uint lenbytes = (uint)(clip.samples * clip.channels * sizeof(float));

        FMOD.CREATESOUNDEXINFO info = new FMOD.CREATESOUNDEXINFO();

        info.length = lenbytes;
        info.format = FMOD.SOUND_FORMAT.PCMFLOAT;
        info.defaultfrequency = clip.frequency;
        info.numchannels = clip.channels;

        FMOD.RESULT result;
        FMOD.Sound sound;

        result = FMODUnity.RuntimeManager.CoreSystem.createSound("", FMOD.MODE.OPENUSER | mode, ref info, out sound);

        IntPtr ptr1, ptr2;
        uint len1, len2;
        
        result = sound.@lock(0, lenbytes, out ptr1, out ptr2, out len1, out len2);

        Marshal.Copy(samples, 0, ptr1, (int)(len1 / sizeof(float)));
        
        if (len2 > 0)
        {
            Marshal.Copy(samples, (int)(len1 / sizeof(float)), ptr2, (int)(len2 / sizeof(float)));
        }

        result = sound.unlock(ptr1, ptr2, len1, len2);
        result = sound.setMode(FMOD.MODE.LOOP_NORMAL);

        sounds[index] = sound;
    }

    public void Play(int channel, int sound)
    {
        FMODUnity.RuntimeManager.CoreSystem.playSound(sounds[sound], channelGroup, false, out channels[channel]);
    }

    public void Stop(int channel)
    {
        channels[channel].isPlaying(out bool isPlaying);

        if (isPlaying) channels[channel].stop();
    }

    public void Release(int sound)
    {
        sounds[sound].release();
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
