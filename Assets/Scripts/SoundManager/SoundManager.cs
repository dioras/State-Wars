using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[Header("Base")]
	[SerializeField] private SoundsStorage soundStorageSO = default;
	[SerializeField] private GameObject audioSourcePrefab = default;
	[SerializeField] private Transform audioParent = default;

	[Header("Settings")]
	/// <summary>
	///	Deffaul count audioSources in starting pool
	/// </summary>
	[SerializeField] private int deffaultAudioSourcesCount = 10;
	/// <summary>
	/// Dellay to check continuous factor in ms
	/// </summary>
	[SerializeField] private int dellayToCheskSoundMs = 250;
	
    [Header("Variables")]
	private readonly List<AudioSourceObject> audioSourceObjects = new List<AudioSourceObject>();
	private CancellationTokenSource cancellationTokenSource = default;

	[Header("Actions")]
	/// <summary>
	/// Action to play sound once time
	/// </summary>
	public static Action<SoundType> PlaySomeSoundOnce = default;
	/// <summary>
	/// Action to play sound while _factor = true
	/// </summary>
	public static Action<SoundType, Func<bool>> PlaySomeSoundContinuous = default;
	/// <summary>
	/// Action to stop all sounds with some sound type - immediately
	/// </summary>
	public static Action<SoundType> StopSomeSoundNow = default;
	/// <summary>
	/// Action to stop all sound immediately
	/// </summary>
	public static Action StopAllSoundNow = default;
	/// <summary>
	/// Action to stop all sound without mainMelody immediately
	/// </summary>
	public static Action StopAllSoundWithoutMelodyNow = default;

	public static bool CanPlayMusic { get; set; } = true;
	public static bool CanPlaySounds { get; set; } = true;

	private void Awake() {
		PrepareStartingAudioSources();
	}

	private void OnEnable() {
		PlaySomeSoundOnce += PlaySoundOnce;
		PlaySomeSoundContinuous += PlaySoundContinuous;
		StopSomeSoundNow += StopSomeSound;
		StopAllSoundNow += StopAllSound;
		StopAllSoundWithoutMelodyNow += StopAllSoundNotMusic;

		// play main melody - for example
		//PlaySomeSoundContinuous?.Invoke(SoundType.MainMelody, () => true);

		cancellationTokenSource = new CancellationTokenSource();
	}

	private void OnDisable() {
		PlaySomeSoundOnce -= PlaySoundOnce;
		PlaySomeSoundContinuous -= PlaySoundContinuous;
		StopSomeSoundNow -= StopSomeSound;
		StopAllSoundNow -= StopAllSound;
		StopAllSoundWithoutMelodyNow -= StopAllSoundNotMusic;
	}

	private void OnDestroy() {
		cancellationTokenSource.Cancel();
	}

	/// <summary>
	/// Starting create pool of audioSources
	/// </summary>
	private void PrepareStartingAudioSources() {
		for (int i = 0; i < deffaultAudioSourcesCount; i++) {
			AddAudioSource();
		}
	}

	/// <summary>
	/// Add audioSource to pool
	/// </summary>
	private void AddAudioSource() {
		audioSourceObjects.Add(Instantiate(audioSourcePrefab, audioParent).GetComponent<AudioSourceObject>());
	}

	/// <summary>
	/// Play sound once time
	/// </summary>
	/// <param name="_soundType"></param>
	private void PlaySoundOnce(SoundType _soundType) {
		try {
			if ((_soundType != SoundType.MainMelody && CanPlaySounds) || (_soundType == SoundType.MainMelody && CanPlayMusic))
			{
				GetFreeAudioSource().PlaySound(soundStorageSO.GetConcreteSound(_soundType));
			}
		}
		catch (Exception _ex) {
			Debug.Log($"Problem with SoundManager - {_ex}");
		}
	}

	/// <summary>
	/// Play sound while _factor = true
	/// </summary>
	/// <param name="_soundType"></param>
	/// <param name="_factor"></param>
	private async void PlaySoundContinuous(SoundType _soundType, Func<bool> _factor) {
		try {
			if (CheckSource(_soundType)) {
				return;
			}

			if ((_soundType != SoundType.MainMelody && CanPlaySounds) || (_soundType == SoundType.MainMelody && CanPlayMusic))
			{
				var someSource = GetFreeAudioSource();
				someSource.PlaySound(soundStorageSO.GetConcreteSound(_soundType), true);
				while (!cancellationTokenSource.IsCancellationRequested && _factor.Invoke() && ((_soundType != SoundType.MainMelody && CanPlaySounds) || (_soundType == SoundType.MainMelody && CanPlayMusic)))
				{
					await Task.Delay(dellayToCheskSoundMs);
				}

				if (!cancellationTokenSource.IsCancellationRequested)
				{
					someSource.StopSound();
				}
			}

		}
		catch (Exception _ex) {
			Debug.Log($"Problem with SoundManager - {_ex}");
		}
	}

	/// <summary>
	/// Find first free audioSource in pool. If no free audioSource - add new and find it
	/// </summary>
	/// <returns></returns>
	private AudioSourceObject GetFreeAudioSource() {
		var temp = audioSourceObjects.Find(someAudioSource => !someAudioSource.IsBusy);
		if (temp != null) {
			return temp;
		}
		else {
			AddAudioSource();
			return GetFreeAudioSource();
		}
	}

	/// <summary>
	/// Check audioSource with concrete soundType
	/// </summary>
	/// <param name="_soundType"></param>
	/// <returns></returns>
	private bool CheckSource(SoundType _soundType) {
		var temp = audioSourceObjects.Find(someSource => someSource.SourceSoundType == _soundType);
		if (temp == null) {
			return false;
		}
		else {
			return temp.IsBusy;
		}
	}

	/// <summary>
	/// Stop all sounds with some sound type - immediately
	/// </summary>
	/// <param name="_soundType"></param>
	private void StopSomeSound(SoundType _soundType) {
		var temp = audioSourceObjects.FindAll(someSource => someSource.SourceSoundType == _soundType);
		if (temp != null && temp.Count > 0) {
			foreach (var item in temp) {
				item.StopSound();
			}
		}
	}

	/// <summary>
	/// Stop all sound immediately
	/// </summary>
	private void StopAllSound() {
		foreach (var item in audioSourceObjects) {
			item.StopSound();
		}
	}

	/// <summary>
	/// Stop all sound without mainMelody immediately
	/// </summary>
	private void StopAllSoundNotMusic()
	{
		foreach (var item in audioSourceObjects)
		{
			if (item.SourceSoundType != SoundType.MainMelody)
			{
				item.StopSound();
			}
		}
	}
}
