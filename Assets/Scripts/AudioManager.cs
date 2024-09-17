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

    public FMOD.Sound Load(string path, FMOD.MODE mode = FMOD.MODE.CREATESAMPLE)
    {
        FMOD.RESULT result = FMODUnity.RuntimeManager.CoreSystem.createSound(path, mode, out FMOD.Sound sound);

        Debug.Log(result);

        return sound;
    }

    public void Play(int index, FMOD.Sound sound)
    {
        FMODUnity.RuntimeManager.CoreSystem.playSound(sound, channelGroup, false, out channels[index]);
    }

    public void Stop(int index)
    {
        channels[index].isPlaying(out bool isPlaying);

        if (isPlaying) channels[index].stop();
    }

    public void Release(int index)
    {
        sounds[index].release();
    }

    public void ReleaseAll()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].release();
        }
    }

    public double GetCurrentPlayTime(int index)
    {
        channels[index].getPosition(out uint position, FMOD.TIMEUNIT.MS);

        return position * 0.001;
    }
}
