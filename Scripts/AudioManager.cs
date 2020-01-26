using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour 
{
	public AudioMixer mixer;
	public AudioMixerGroup soundEffectGroup;
	public AudioMixerSnapshot worldMapSnapshot, levelSnapshot;
	public AudioSource worldMapSource, levelSource;
	public float fTransitionTime = 2.0f;

	public AudioClip mainMenuMusic, worldMapMusic;

	public AudioSource guiClick, pickupMinion, dropMinion, guiReject, resSFX;

	void Start () 
	{
		
	}

	void Update () 
	{
		
	}

	public void SetLevelMusic(AudioClip clip)
	{
		levelSource.Stop();
		levelSource.clip = clip;
		levelSource.Play();
		mixer.TransitionToSnapshots(new AudioMixerSnapshot[1] { levelSnapshot }, new float[1] { 1.0f }, fTransitionTime);
	}

	public void ReturnToWorldMapMusic()
	{
		mixer.TransitionToSnapshots(new AudioMixerSnapshot[1] { worldMapSnapshot }, new float[1] { 1.0f }, fTransitionTime);
	}

	public void SetMainMenuMusic()
	{
		worldMapSource.Stop();
		worldMapSource.clip = mainMenuMusic;
		worldMapSource.Play();
	}
		
	public void SetWorldMapMusic()
	{
		worldMapSource.Stop();
		worldMapSource.clip = worldMapMusic;
		worldMapSource.Play();
	}

	public void PlayGUIClick()
	{
		guiClick.Play();
	}

	public void PlayPickupMinion()
	{
		pickupMinion.Play();
	}

	public void PlayDropMinion()
	{
		dropMinion.Play();
	}

	public void PlayGUIReject()
	{
		guiReject.Play();
	}

	public void PlayResSFX(AudioClip clip)
	{
		resSFX.Stop();
		resSFX.clip = clip;
		resSFX.Play();
	}

	public void SetMusicVolume(float fSet)
	{
		mixer.SetFloat("MusicVolume", fSet);
	}	

	public void SetGameVolume(float fSet)
	{
		mixer.SetFloat("SoundEffectVolume", fSet);
	}
}
