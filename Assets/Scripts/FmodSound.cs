using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;

public class FmodSound : MonoBehaviour
{
    #region Properties
    public static float MasterVolume => _master_volume;
    public static float SfxVolume => _sfx_volume;
    public static float BgmVolume => _bgm_volume;
    public static float AmbientVolume => _ambient_volume;
    #endregion

    #region Fields
    public const string MASTER_BUS = "Master Bus";
    public const string SFX_BUS = "SFX";
    public const string BGM_BUS = "BGM";
    public const string AMBIENT_BUS = "Ambient";
    private const float MASTER_DEFAULT_VOLUME = 70f;
    private const float SFX_DEFAULT_VOLUME = 80f;
    private const float BGM_DEFAULT_VOLUME = 70f;
    private const float AMBIENT_DEFAULT_VOLUME = 70f;

    private static float _master_volume = MASTER_DEFAULT_VOLUME;
    private static float _sfx_volume = SFX_DEFAULT_VOLUME;
    private static float _bgm_volume = BGM_DEFAULT_VOLUME;
    private static float _ambient_volume = AMBIENT_DEFAULT_VOLUME;

    [SerializeField] private string fmodEventName;

    private List<EventInstance> _eventInstances = new List<EventInstance>();
    #endregion

    #region Public Static Methods
    public static void StopAllSounds()
    {
        RuntimeManager.GetBus(SFX_BUS).stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        RuntimeManager.GetBus(BGM_BUS).stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        RuntimeManager.GetBus(AMBIENT_BUS).stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        RuntimeManager.GetBus(MASTER_BUS).stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public static float GetBusVolume(string bus)
    {
        float volume;
        RuntimeManager.GetBus(bus).getVolume(out volume);
        return volume;
    }

    public static void SetBusVolume(string bus, float volume)
    {
        RuntimeManager.GetBus(bus).setVolume(volume);
    }
    #endregion

    #region Public Methods
    public EventInstance Play()
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(fmodEventName);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, transform);
        eventInstance.start();
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }
    #endregion

    public EventInstance Stop()
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(fmodEventName);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, transform);
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        return eventInstance;
    }

    #region Private Methods
    private void OnDisable()
    {
        for (int i = 0; i < _eventInstances.Count; i++)
        {
            _eventInstances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
    #endregion
}
