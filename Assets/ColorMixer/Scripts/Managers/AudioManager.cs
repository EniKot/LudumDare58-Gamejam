using System;
using System.Collections.Generic;
using UnityEngine;
using Tools;

[DisallowMultipleComponent]
public class AudioManager : Singleton<AudioManager>
{
    [Serializable]
    public class NamedClip
    {
        public string key;          // 如 "Shoot" / "ChestHit" / "ChestOpen" / "CardPickup" / "DoorOpen" / "PickColor"
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool spatial = false; // 勾选则按3D播放（位置音效）
    }

    [Header("🔊 资源库：按 key 映射音效")]
    public List<NamedClip> sfxClips = new List<NamedClip>();

    [Header("🎵 BGM 槽")]
    public AudioSource bgmSource;

    [Header("🔁 SFX 播放通道数量（池）")]
    public int sfxVoices = 8;

    private readonly Dictionary<string, NamedClip> _sfxMap = new Dictionary<string, NamedClip>();
    private AudioSource[] _sfxSources;
    private int _nextVoice;

    protected override void Awake()
    {
        base.Awake();

        // 建立 key -> clip 映射
        _sfxMap.Clear();
        foreach (var nc in sfxClips)
        {
            if (!string.IsNullOrEmpty(nc.key) && nc.clip != null)
                _sfxMap[nc.key] = nc;
        }

        // 准备 SFX 声道池
        if (_sfxSources == null || _sfxSources.Length != sfxVoices)
        {
            _sfxSources = new AudioSource[sfxVoices];
            for (int i = 0; i < sfxVoices; i++)
            {
                var go = new GameObject($"SFX_Voice_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 默认2D
                _sfxSources[i] = src;
            }
        }

        // 准备 BGM 源
        if (bgmSource == null)
        {
            var go = new GameObject("BGM_Source");
            go.transform.SetParent(transform);
            bgmSource = go.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f; // 2D
        }
    }

    private AudioSource NextSfxSource()
    {
        var src = _sfxSources[_nextVoice];
        _nextVoice = (_nextVoice + 1) % _sfxSources.Length;
        return src;
    }

    public void PlaySFX(string key)
    {
        if (_sfxMap.TryGetValue(key, out var nc))
        {
            var src = NextSfxSource();
            src.spatialBlend = 0f;
            src.transform.position = Vector3.zero;
            src.volume = nc.volume;
            src.clip = nc.clip;
            src.Play();
        }
        // 未配置的 key 静默忽略，保持简单
    }

    public void PlaySFXAt(string key, Vector3 worldPos)
    {
        if (_sfxMap.TryGetValue(key, out var nc))
        {
            var src = NextSfxSource();
            src.transform.position = worldPos;
            src.spatialBlend = nc.spatial ? 1f : 0f;
            src.volume = nc.volume;
            src.clip = nc.clip;
            src.Play();
        }
    }

    public void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }
}
