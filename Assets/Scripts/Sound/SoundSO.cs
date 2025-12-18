using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundSO", menuName = "Scriptable Objects/SoundSO")]
public class SoundSO : ScriptableObject
{
    public List<Sounds> SoundsList;
}
[Serializable]
public struct Sounds
{
    public SoundTypes Type;
    public AudioClip Clip;
}

