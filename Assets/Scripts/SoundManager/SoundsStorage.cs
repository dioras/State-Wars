using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SoundsStorage", fileName = "SoundsStorage")]
public class SoundsStorage : ScriptableObject
{
	[Header("Sound Storage")]
	[SerializeField] private List<SomeSound> someSounds = default;

	public void SomeSoundPlay(SoundType _soundType) {
		SoundManager.PlaySomeSoundOnce?.Invoke(_soundType);
	}

	public void SomeSoundPlayContinuous(SoundType _soundType, Func<bool> _factor) {
		SoundManager.PlaySomeSoundContinuous?.Invoke(_soundType, _factor);
	}

	public SomeSound GetConcreteSound(SoundType _soundType) {
		return someSounds.Find(someSound => someSound.soundType == _soundType);
	}
}

[Serializable]
public class SomeSound {
	public SoundType soundType;
	[SoundBar]
	public float soundVolume = 1f;
	public List<AudioClip> audioClip;
}
