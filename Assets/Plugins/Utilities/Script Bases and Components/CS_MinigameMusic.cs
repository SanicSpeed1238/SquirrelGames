using UnityEngine;

public class CS_MinigameMusic : MonoBehaviour
{
    [Header("Music List")]
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip setupMusic;

    [Header("Track List")]
    [SerializeField] private AudioClip openingTrack;
    [SerializeField] private AudioClip resultsTrack;
}