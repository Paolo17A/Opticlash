using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //================================================================================
    [field: SerializeField] public AudioSource AudioSource { get; set; }

    [Header("BACKGROUND MUSIC")]
    [SerializeField] private AudioClip loadingBGM;
    [SerializeField] private AudioClip gameplayBGM;

    [Header("LOBBY SOUND EFFECTS")]
    [SerializeField] private AudioClip UpgradeSuccessSFX;
    [SerializeField] private AudioClip OpenBoxSFX;
    [SerializeField] private AudioClip ShowOptiSFX;

    [Header("MATCHING SOUND EFFECTS")]
    [SerializeField] private List<AudioClip> MatchSFX;
    [SerializeField] private List<AudioClip> MoveSFX;
    [SerializeField] private List<AudioClip> ShuffleSFX;

    [Header("COMBAT SOUND EFFECTS")]
    [SerializeField] private AudioClip CannonSFX;
    [SerializeField] private AudioClip WalkSFX;
    [SerializeField] public AudioClip HitSFX;
    [SerializeField] private List<AudioClip> DoubleDamageSFX;
    [SerializeField] private List<AudioClip> ShieldSFX;
    [SerializeField] private AudioClip GameOverSFX;

    [field: Header("DEBUGGER")]
    [field: SerializeField] public bool IsPlaying { get; set; }
    //================================================================================

    public void PlayAudio()
    {
        AudioSource.Play();
    }

    public void SwitchToGameplayMusic()
    {
        AudioSource.Stop();
        AudioSource.clip = gameplayBGM;
    }

    public void SwitchToLoadingMusic()
    {
        AudioSource.Stop();
        AudioSource.clip = loadingBGM;
        PlayAudio();
        AudioSource.mute = false;
        IsPlaying = true;
    }

    #region SFX
    #region LOBBY
    public void PlayOpenBoxSFX()
    {
        AudioSource.clip = OpenBoxSFX;
        PlayAudio();
    }

    public void PlayShowOptiSFX()
    {
        AudioSource.clip = ShowOptiSFX;
        PlayAudio();
    }

    public void PlayUpgradeSFX()
    {
        AudioSource.clip = UpgradeSuccessSFX;
        PlayAudio();
    }
    #endregion

    #region MATCHING
    public void PlayMatchSFX()
    {
        AudioSource.clip = MatchSFX[Random.Range(0, MatchSFX.Count)];
        PlayAudio();
    }

    public void PlayMoveSFX()
    {
        AudioSource.clip = MoveSFX[Random.Range(0, MoveSFX.Count)];
        PlayAudio();
    }

    public void PlayShuffleSFX()
    {
        AudioSource.clip = ShuffleSFX[Random.Range(0, ShuffleSFX.Count)];
        PlayAudio();
    }
    #endregion

    #region COMBAT
    public void PlayCannonSFX()
    {
        AudioSource.clip = CannonSFX;
        PlayAudio();
    }

    public void PlayWalkSFX()
    {
        AudioSource.clip = WalkSFX;
        PlayAudio();
    }

    public void PlayHitSFX()
    {
        AudioSource.clip = HitSFX;
        PlayAudio();
    }

    public void PlayDoubleDamageSFX()
    {
        AudioSource.clip = DoubleDamageSFX[Random.Range(0, DoubleDamageSFX.Count)];
        PlayAudio();
    }

    public void PlayShieldSFX()
    {
        AudioSource.clip = ShieldSFX[Random.Range(0, ShieldSFX.Count)];
        PlayAudio();
    }

    public void PlayGameOverSFX()
    {
        AudioSource.clip = GameOverSFX;
        PlayAudio();
    }
    #endregion
    #endregion
}
