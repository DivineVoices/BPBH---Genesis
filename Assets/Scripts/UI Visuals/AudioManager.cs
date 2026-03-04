using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;

        public AudioClip clip;

        [Range(0f, 1f)] public float volume = 1f;

        [Range(0.1f, 3f)] public float pitch = 1f;

        public bool loop = false;

    }

    [SerializeField] private SoundEffect[] soundEffects;

    [SerializeField][Range(1, 100)] private int poolSize = 10;

    private List<AudioSource> audioSources = new List<AudioSource>();

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();

    public static AudioManager Instance { get; private set; }

    private float gloablSFXVolume = 1f;

    public float GloablSFXVolume
    {
        get { return gloablSFXVolume; }
        set { gloablSFXVolume = Mathf.Clamp01(value); }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // On crée la pool d'AudioSource
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            audioSources.Add(source);
        }

        // On remplit le dictionnaire des sons
        foreach (SoundEffect sound in soundEffects)
        {
            if (sound.clip != null)
            {
                soundDictionary[sound.name] = sound.clip;
            }
        }
    }

    /// <summary>
    /// Retourne un AudioSource disponible dans la pool
    /// SI aucun n'est disponible, recycle le premier
    /// </summary>
    /// <returns></returns>
    private AudioSource GetAvailableAudioSource()
    {

        // On prend le premier AudioSource libre
        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // Forcer le recyclage du premier AudioSource si tous sont occupés
        return audioSources.Count > 0 ? audioSources[0] : null;
    }

    /// <summary>
    /// Joue un son par son nom
    /// </summary>
    /// <param name="soundName"></param>
    public void PlaySound(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {

            foreach (SoundEffect sound in soundEffects)
            {
                if (sound.name == soundName)
                {
                    source.clip = clip;
                    source.volume = sound.volume * gloablSFXVolume;
                    source.pitch = sound.pitch;
                    source.loop = sound.loop;
                    source.Play();
                    return;
                }
            }
        }
    }


    /// <summary>
    /// Arrête tous les sons en cours de lecture
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var source in audioSources)
        {
            source.Stop();
        }
    }

    /// <summary>
    /// Arrête un son par son nom
    /// </summary>
    /// <param name="soundName"></param>
    public void StopSound(string soundName)
    {
        foreach (var source in audioSources)
        {
            if (source.isPlaying && source.clip != null && source.clip.name == soundName)
            {
                source.Stop();
            }
        }
    }


    /// <summary>
    /// Vérifie si un son est en cours de lecture
    /// </summary>
    /// <param name="soundName"></param>
    /// <returns></returns>
    public bool IsSoundPlaying(string soundName)
    {
        foreach (var source in audioSources)
        {
            if (source.isPlaying && source.clip != null && source.clip.name == soundName)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Définit la taille [1-100] de la pool d'AudioSource 
    /// </summary>
    /// <param name="size"></param>
    public void SetpoolSize(int size)
    {
        poolSize = Mathf.Clamp(size, 1, 100);

        // Ajuster la taille de la pool
        while (audioSources.Count < poolSize)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            audioSources.Add(source);
        }
        while (audioSources.Count > poolSize)
        {
            AudioSource source = audioSources[audioSources.Count - 1];
            audioSources.RemoveAt(audioSources.Count - 1);
            Destroy(source);
        }
    }

    public void PlaySoundWithPitch(string soundName, float pitch)
    {
        if (!soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            foreach (SoundEffect sound in soundEffects)
            {
                if (sound.name == soundName)
                {
                    source.clip = clip;
                    source.volume = sound.volume * gloablSFXVolume;

                    // override pitch ONLY for this playback
                    source.pitch = pitch;

                    source.loop = false;
                    source.Play();
                    return;
                }
            }
        }
    }


}
