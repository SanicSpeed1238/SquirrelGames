using UnityEngine;
using System.Collections;

public class CS_MinigameMusic : MonoBehaviour
{
    [Header("Music List")]
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip setupMusic;

    private AudioSource musicPlayer;

    private void Start()
    {
        InitializeMusic();

        PlaySetupMusic();
    }

    private void InitializeMusic()
    {
        musicPlayer = GetComponent<AudioSource>();

        musicPlayer.clip = gameMusic;
        musicPlayer.Play();
        musicPlayer.Stop();

        musicPlayer.clip = setupMusic;
        musicPlayer.Play();
        musicPlayer.Stop();
    }

    public void PlaySetupMusic()
    {
        musicPlayer.clip = setupMusic;
        musicPlayer.Play();
    }

    public void PlayGameMusic()
    {
        musicPlayer.clip = gameMusic;
        musicPlayer.Play();
    }

    public void FadeOutMusic(float fadeOutSpeed)
    {
        StartCoroutine(FadeOutCurrentClip(fadeOutSpeed));
    }
    IEnumerator FadeOutCurrentClip(float fadeOutSpeed)
    {
        float startVolume = musicPlayer.volume;

        while (musicPlayer.volume > 0)
        {
            musicPlayer.volume -= startVolume * Time.deltaTime / fadeOutSpeed;
            yield return null;
        }

        musicPlayer.Stop();
        musicPlayer.volume = startVolume;
    }

    public void StopMusic()
    {
        musicPlayer.Stop();
    }
}