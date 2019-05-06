using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {

	public static AudioManager instance;

	[SerializeField] AudioClip worldBGM;
	[SerializeField] AudioClip dangerBGM;
	[SerializeField] AudioClip menuBGM;
	[SerializeField] AudioClip gameOverBGM;
	public AudioClip keySfx;
	public AudioClip pickupSfx;
	public AudioClip ringUseSfx;
	public AudioClip swordUseSfx;
	public AudioClip slingUseSfx;
	public AudioClip potionUseSfx;
	public AudioClip hurtSfx;
	public AudioClip alertSfx;
	public AudioClip breakSfx;
	public AudioClip hitSfx;
	public AudioClip openDoorSfx;

	AudioSource source;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
			source = GetComponent<AudioSource>();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		else {
			Destroy(gameObject);
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		AudioListener listener = Camera.main.GetComponent<AudioListener>();
		if (listener != null) {
			Destroy(listener);
		}
		if (scene.name == "Playtest") {
			playWorldBGM();
		}
		else if (scene.name == "Game Over") {
			playGameOverBGM();
		}
		else {
			playMenuBGM();
		}
	}

	public static void playSFX(AudioClip clip, float volumeScale = 1f) {
		instance.source.PlayOneShot(clip, volumeScale);
	}

	static void playWorldBGM() {
		instance.source.clip = instance.worldBGM;
		instance.source.Play();
	}

	static void playMenuBGM() {
		instance.source.clip = instance.menuBGM;
		instance.source.Play();
	}

	static void playGameOverBGM() {
		instance.source.clip = instance.gameOverBGM;
		instance.source.Play();
	}

	public static void switchToDangerBGM() {
		instance.source.clip = instance.dangerBGM;
//		instance.source.time
		instance.source.Play();
	}

	public static void switchToWorldBGM() {
		instance.source.clip = instance.worldBGM;
		instance.source.Play();
	}




}
