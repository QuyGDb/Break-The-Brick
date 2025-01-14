using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }
    public AudioMixerGroup soundsMasterMixerGroup;
    public AudioMixerGroup musicMasterMixerGroup;
    public AudioMixerSnapshot musicLowSnapshot;
    public AudioMixerSnapshot musicOnFullSnapshot;
    public AudioMixerSnapshot musicOffSnapshot;

    public MusicTrackSO musicTrack1;
    public MusicTrackSO musicTrack2;
    public MusicTrackSO musicTrack_Ingame;
}