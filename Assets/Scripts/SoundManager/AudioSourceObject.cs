using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceObject : MonoBehaviour
{
	/// <summary>
	/// Dellay to check if can release in ms
	/// </summary>
	[SerializeField] private int dellayToCheskSoundMs = 250;
	/// <summary>
	/// Unity audioSource
	/// </summary>
	private AudioSource audioSource = default;
	/// <summary>
	/// Conrete soundType
	/// </summary>
	public SoundType SourceSoundType { get; private set; } = SoundType.None;
	/// <summary>
	/// Can this source play some sound?
	/// </summary>
	public bool IsBusy { get; set; } = false;
	/// <summary>
	/// 
	/// </summary>
	private CancellationTokenSource cancellationTokenSource = default;

	private void Awake() {
		audioSource = gameObject.GetComponent<AudioSource>();
		cancellationTokenSource = new CancellationTokenSource();
	}

	private void OnDestroy()
	{
		cancellationTokenSource.Cancel();
	}

	/// <summary>
	/// Play _someSound
	/// </summary>
	/// <param name="_someSound"></param>
	/// <param name="_isLoop">need loop this sound?</param>
	public void PlaySound(SomeSound _someSound, bool _isLoop = false) {
		if (_someSound != null && _someSound.audioClip != null && _someSound.audioClip.Count > 0) 
		{
			audioSource.loop = _isLoop;
			SourceSoundType = _someSound.soundType;
			IsBusy = true;
			audioSource.clip = _someSound.audioClip[Random.Range(0, _someSound.audioClip.Count)];
			audioSource.volume = _someSound.soundVolume;
			if (audioSource.clip != null) {
				audioSource.Play();
			}

			if (!_isLoop) {
				CheckFoRelease();
			}
		}
	}

	/// <summary>
	/// Release non loop audioSorce after sound stop playing
	/// </summary>
	private async void CheckFoRelease() {
		while (!cancellationTokenSource.IsCancellationRequested && audioSource.isPlaying) {
			await Task.Delay(dellayToCheskSoundMs);
		}
		if (!cancellationTokenSource.IsCancellationRequested)
		{
			StopSound();
		}
	}

	public void StopSound() {
		audioSource.Stop();
		SourceSoundType = SoundType.None;
		audioSource.loop = false;
		IsBusy = false;
	}
}
