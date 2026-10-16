using UnityEngine;

[CreateAssetMenu(fileName = "AudioVolumeSettings", menuName = "Audio/Volume Settings")]
public class AudioVolumeSettings : ScriptableObject
{
    [Range(0f, 2f)] public float musicVolume = 0.1f;
    [Range(0f, 2f)] public float startVolume = 1f;
    [Range(0f, 2f)] public float startEndVolume = 1f;
    [Range(0f, 2f)] public float finishVolume = 1f;
    [Range(0f, 2f)] public float warningVolume = 1f;
    [Range(0f, 2f)] public float runVolume = 0.25f;
    [Range(0f, 2f)] public float hitVolume = 1.5f;
    [Range(0f, 2f)] public float playerHitVolume = 1f;
    [Range(0f, 2f)] public float playerEliminatedVolume = 1f;
    [Range(0f, 2f)] public float previewVolume = 0.2f;
    [Range(0f, 2f)] public float resumeVolume = 0.2f;
    [Range(0f, 2f)] public float finalChargeVolume = 0.2f;
}
