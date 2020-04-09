using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pan : PandoraSingleton<Pan>
{
	public enum Type
	{
		MASTER,
		FX,
		MUSIC,
		AMBIENT,
		VOICE,
		NARRATOR
	}

	public const float DEFAULT_VOLUME = 1f;

	public const float DEFAULT_MUSIC_VOLUME = 0.45f;

	public const float DEFAULT_AMBIENT_VOLUME = 0.75f;

	private const float MUSIC_FADE_TIME = 3f;

	private readonly string[] ambients = new string[21]
	{
		"bad_entity",
		"butcher_door",
		"corpse_decay",
		"corrupted_priests",
		"creature_chain",
		"death_breath",
		"deep_down",
		"demon",
		"exhale",
		"freak_lament",
		"ghost_whisper",
		"horror_boom",
		"laugh_madness",
		"old_tension",
		"possessed_baby",
		"pure_chaos",
		"secret_winch",
		"soul_eater",
		"spirit_leech",
		"torture_shouts",
		"underworld_portal"
	};

	public float masterVolume = 1f;

	private List<AudioSource> audioSourcesFx = new List<AudioSource>();

	public float fxVolume = 1f;

	[SerializeField]
	private AudioSource currentMusic;

	[SerializeField]
	private AudioSource nextMusic;

	public float musicVolume = 0.45f;

	private List<AudioSource> audioSourcesVoice = new List<AudioSource>();

	private AudioSource audioSourceNarrator;

	private Queue<AudioClip> narrations;

	public float voiceVolume = 1f;

	private List<AudioClip> clipAmbient = new List<AudioClip>();

	private AudioSource ambientSource;

	public float ambientVolume = 0.75f;

	private Dictionary<int, AudioClip> sounds;

	public bool Initialized
	{
		get;
		private set;
	}

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		nextMusic = null;
		currentMusic = null;
		audioSourcesFx = new List<AudioSource>();
		audioSourcesVoice = new List<AudioSource>();
		sounds = new Dictionary<int, AudioClip>();
		narrations = new Queue<AudioClip>();
		Initialized = true;
	}

	private void OnDestroy()
	{
		Clear();
	}

	public void Clear()
	{
		foreach (AudioSource item in audioSourcesFx)
		{
			UnityEngine.Object.Destroy(item);
		}
		audioSourcesFx.Clear();
		foreach (AudioSource item2 in audioSourcesVoice)
		{
			UnityEngine.Object.Destroy(item2);
		}
		audioSourcesVoice.Clear();
		sounds.Clear();
	}

	public void SetVolume(Type type, float volume)
	{
		switch (type)
		{
		case Type.MASTER:
			masterVolume = Mathf.Clamp(volume, 0f, 1f);
			AudioListener.volume = masterVolume;
			break;
		case Type.FX:
			fxVolume = Mathf.Clamp(volume, 0f, 1f);
			foreach (AudioSource item in audioSourcesFx)
			{
				if (item != null)
				{
					item.volume = fxVolume;
				}
			}
			break;
		case Type.MUSIC:
			musicVolume = Mathf.Clamp(volume, 0f, 1f);
			if (currentMusic != null)
			{
				currentMusic.volume = musicVolume;
			}
			break;
		case Type.VOICE:
			voiceVolume = Mathf.Clamp(volume, 0f, 1f);
			foreach (AudioSource item2 in audioSourcesVoice)
			{
				if (item2 != null)
				{
					item2.volume = voiceVolume;
				}
			}
			if (audioSourceNarrator != null)
			{
				audioSourceNarrator.volume = voiceVolume;
			}
			break;
		case Type.NARRATOR:
			if (audioSourceNarrator != null)
			{
				audioSourceNarrator.volume = voiceVolume;
			}
			break;
		case Type.AMBIENT:
			ambientVolume = Mathf.Clamp(volume, 0f, 1f);
			if (ambientSource != null)
			{
				ambientSource.volume = ambientVolume;
			}
			break;
		}
	}

	public void PlayMusic(string name, bool ambiance)
	{
		PandoraDebug.LogDebug("Play Music = " + name);
		if (nextMusic == null)
		{
			GameObject gameObject = new GameObject();
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			AddSource(audioSource, Type.MUSIC);
		}
		if (nextMusic != null)
		{
			PandoraDebug.LogDebug("Music LOAD = " + name);
			GetSound(name, cache: false, delegate(AudioClip clip)
			{
				PandoraDebug.LogDebug("Music LOADED = " + name);
				nextMusic.clip = clip;
				PlayMusic();
				SetAmbiance(ambiance);
			});
		}
	}

	public void PlayMusic()
	{
		if (currentMusic != null && currentMusic.isPlaying)
		{
			PandoraDebug.LogDebug("PlayMusic  = " + base.name);
			StartCoroutine(Fade(currentMusic, musicVolume, 0f, kill: true));
		}
		currentMusic = nextMusic;
		if (currentMusic != null && !currentMusic.isPlaying)
		{
			UnityEngine.Object.DontDestroyOnLoad(currentMusic.gameObject);
			StartCoroutine(Fade(currentMusic, 0f, musicVolume, kill: false));
		}
		nextMusic = null;
	}

	public void PauseMusic(bool fade = true)
	{
		if ((bool)currentMusic && currentMusic.isPlaying)
		{
			if (fade)
			{
				StartCoroutine(Fade(currentMusic, musicVolume, 0f, kill: false, delegate
				{
					if (currentMusic != null)
					{
						currentMusic.Pause();
					}
				}));
			}
			else
			{
				currentMusic.Pause();
			}
		}
	}

	public void UnPauseMusic(bool fade = true)
	{
		if ((bool)currentMusic)
		{
			if (fade)
			{
				StartCoroutine(Fade(currentMusic, 0f, musicVolume, kill: false, delegate
				{
					if (currentMusic != null)
					{
						currentMusic.UnPause();
					}
				}));
			}
			else
			{
				currentMusic.UnPause();
			}
		}
	}

	public void SoundsOn()
	{
		StartCoroutine(FadeSoundsVolume(on: true));
	}

	public void SoundsOff()
	{
		StartCoroutine(FadeSoundsVolume(on: false));
	}

	private IEnumerator FadeSoundsVolume(bool on)
	{
		float time = 0f;
		float fxVolTo = 0f;
		float fxVolFr = fxVolume;
		float ambientVolTo = 0f;
		float ambientVolFr = ambientVolume;
		float narVolTo = 0f;
		float narVolFr = voiceVolume;
		if (on)
		{
			fxVolTo = PandoraSingleton<GameManager>.Instance.Options.fxVolume;
			fxVolFr = 0f;
			ambientVolTo = PandoraSingleton<GameManager>.Instance.Options.ambientVolume;
			ambientVolFr = 0f;
			narVolTo = PandoraSingleton<GameManager>.Instance.Options.voiceVolume;
			narVolFr = 0f;
		}
		for (; time < 3f; time += Time.smoothDeltaTime)
		{
			float newVolFx = Mathf.Lerp(fxVolFr, fxVolTo, time / 3f);
			float newVolamb = Mathf.Lerp(ambientVolFr, ambientVolTo, time / 3f);
			float newVolNar = Mathf.Lerp(narVolFr, narVolTo, time / 3f);
			SetVolume(Type.FX, newVolFx);
			SetVolume(Type.NARRATOR, newVolNar);
			SetVolume(Type.AMBIENT, newVolNar);
			yield return 0;
		}
	}

	private IEnumerator Fade(AudioSource sound, float fromV, float toV, bool kill, Action onFadeDone = null)
	{
		if (sound == null)
		{
			yield break;
		}
		float time = 0f;
		sound.volume = fromV;
		if (!sound.isPlaying)
		{
			sound.Play();
		}
		for (; time < 3f; time += Time.smoothDeltaTime)
		{
			if (!(sound != null))
			{
				break;
			}
			sound.volume = Mathf.Lerp(fromV, toV, time / 3f);
			yield return 0;
		}
		if (sound != null)
		{
			sound.volume = toV;
		}
		onFadeDone?.Invoke();
		if (kill)
		{
			UnityEngine.Object.Destroy(sound.gameObject);
		}
	}

	public void SetAmbiance(bool on)
	{
		if (on)
		{
			StartCoroutine(PlayAmbiance());
		}
		else
		{
			StopCoroutine(PlayAmbiance());
		}
	}

	private IEnumerator PlayAmbiance()
	{
		Tyche tyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
		Vector3 pos = default(Vector3);
		while (true)
		{
			yield return new WaitForSeconds(tyche.Rand(50, 75));
			if (ambientSource != null)
			{
				pos.x = tyche.Rand(-75, 75);
				pos.y = tyche.Rand(-20, 20);
				pos.z = tyche.Rand(-75, 75);
				ambientSource.transform.position = pos;
				GetSound(ambients[tyche.Rand(0, ambients.Length)], cache: true, PlayAmbiantSound);
			}
		}
	}

	private void PlayAmbiantSound(AudioClip clip)
	{
		if (ambientSource != null)
		{
			ambientSource.PlayOneShot(clip);
		}
	}

	public void Narrate(string narration)
	{
		GetSound("voices/narrator/", narration, cache: false, delegate(AudioClip clip)
		{
			if (audioSourceNarrator.isPlaying && clip != audioSourceNarrator.clip)
			{
				if (!narrations.Contains(clip))
				{
					narrations.Enqueue(clip);
					StopCoroutine(NarrateQueue());
					StartCoroutine(NarrateQueue());
				}
			}
			else
			{
				audioSourceNarrator.clip = clip;
				audioSourceNarrator.Play();
			}
		});
	}

	private IEnumerator NarrateQueue()
	{
		while (audioSourceNarrator.isPlaying)
		{
			yield return new WaitForSeconds(1f);
			if (!audioSourceNarrator.isPlaying && narrations.Count > 0)
			{
				AudioClip clip = narrations.Dequeue();
				audioSourceNarrator.clip = clip;
				audioSourceNarrator.Play();
			}
		}
	}

	public void AddSource(PanFlute audioEmitter)
	{
		AddSource(audioEmitter.GetComponent<AudioSource>(), audioEmitter.fluteType);
	}

	public void AddSource(AudioSource audioSource, Type fluteType)
	{
		if (audioSource == null)
		{
			return;
		}
		switch (fluteType)
		{
		case Type.FX:
			audioSourcesFx.Add(audioSource);
			audioSource.volume = fxVolume;
			break;
		case Type.MUSIC:
			if (nextMusic == null)
			{
				nextMusic = audioSource;
			}
			nextMusic.volume = musicVolume;
			nextMusic.loop = true;
			nextMusic.spatialBlend = 0f;
			break;
		case Type.VOICE:
			audioSourcesVoice.Add(audioSource);
			audioSource.volume = voiceVolume;
			audioSource.loop = false;
			audioSource.spatialBlend = 0f;
			break;
		case Type.NARRATOR:
			if (audioSourceNarrator == null)
			{
				audioSourceNarrator = audioSource;
				audioSourceNarrator.volume = voiceVolume;
				audioSourceNarrator.loop = false;
				audioSourceNarrator.spatialBlend = 0f;
			}
			break;
		case Type.AMBIENT:
			if (ambientSource == null)
			{
				ambientSource = audioSource;
				ambientSource.volume = ambientVolume;
			}
			break;
		}
	}

	public void RemoveSource(AudioSource source, Type type)
	{
		switch (type)
		{
		case Type.FX:
			audioSourcesFx.Remove(source);
			break;
		case Type.MUSIC:
			if (currentMusic == source)
			{
				source.Stop();
				UnityEngine.Object.Destroy(source);
			}
			break;
		case Type.VOICE:
			audioSourcesVoice.Remove(source);
			break;
		case Type.AMBIENT:
			ambientSource = null;
			break;
		case Type.NARRATOR:
			audioSourceNarrator = null;
			break;
		}
	}

	public void GetSound(string soundName, bool cache, Action<AudioClip> OnLoad)
	{
		GetSound(string.Empty, soundName, cache, OnLoad);
	}

	public void GetSound(string path, string soundName, bool cache, Action<AudioClip> OnLoad)
	{
		int hash = 0;
		if (cache)
		{
			hash = soundName.GetHashCode();
			if (sounds.ContainsKey(hash))
			{
				OnLoad(sounds[hash]);
				return;
			}
		}
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<AudioClip>("Assets/sounds/runtime/" + path, AssetBundleId.SOUNDS, soundName + ".ogg", delegate(UnityEngine.Object sound)
		{
			AudioClip audioClip = (AudioClip)sound;
			if (audioClip == null)
			{
				PandoraDebug.LogWarning("Sound missing : " + soundName, "SOUND");
				OnLoad(null);
			}
			else
			{
				if (cache && hash != 0)
				{
					sounds[hash] = audioClip;
				}
				OnLoad(audioClip);
			}
		});
	}
}
