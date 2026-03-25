using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] AudioData _audioData;
        [SerializeField] int _sfxPoolSize = 10;

        [Header("Volume (persisted in PlayerPrefs)")]
        [Range(0f, 1f)] [SerializeField] float _masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] float _musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] float _sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] float _uiVolume = 0.8f;

        AudioSource _musicSourceA;
        AudioSource _musicSourceB;
        bool _musicAActive = true;
        float _crossfadeDuration = 1.5f;
        float _crossfadeTimer;
        bool _isCrossfading;

        AudioSource[] _sfxPool;
        int _sfxPoolIndex;

        Dictionary<SoundId, AudioData.SoundEntry> _soundMap = new();

        SoundId _currentMusicId;

        public float MasterVolume
        {
            get => _masterVolume;
            set { _masterVolume = Mathf.Clamp01(value); SavePrefs(); UpdateVolumes(); }
        }
        public float MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = Mathf.Clamp01(value); SavePrefs(); UpdateVolumes(); }
        }
        public float SfxVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = Mathf.Clamp01(value); SavePrefs(); }
        }
        public float UIVolume
        {
            get => _uiVolume;
            set { _uiVolume = Mathf.Clamp01(value); SavePrefs(); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Create music sources
            _musicSourceA = gameObject.AddComponent<AudioSource>();
            _musicSourceA.loop = true;
            _musicSourceA.playOnAwake = false;

            _musicSourceB = gameObject.AddComponent<AudioSource>();
            _musicSourceB.loop = true;
            _musicSourceB.playOnAwake = false;

            // Create SFX pool
            _sfxPool = new AudioSource[_sfxPoolSize];
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                _sfxPool[i] = gameObject.AddComponent<AudioSource>();
                _sfxPool[i].playOnAwake = false;
            }

            LoadPrefs();
            BuildSoundMap();
            GeneratePlaceholderAudio();
        }

        void OnEnable()
        {
            EventBus.Subscribe<TapEvent>(OnTap);
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Subscribe<AscensionEvent>(OnAscension);
            EventBus.Subscribe<IslandChangedEvent>(OnIslandChanged);
            EventBus.Subscribe<IslandBossSpawnedEvent>(OnBossSpawned);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<TapEvent>(OnTap);
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Unsubscribe<AscensionEvent>(OnAscension);
            EventBus.Unsubscribe<IslandChangedEvent>(OnIslandChanged);
            EventBus.Unsubscribe<IslandBossSpawnedEvent>(OnBossSpawned);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        void Update()
        {
            if (!_isCrossfading) return;

            _crossfadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_crossfadeTimer / _crossfadeDuration);

            var active = _musicAActive ? _musicSourceA : _musicSourceB;
            var inactive = _musicAActive ? _musicSourceB : _musicSourceA;

            active.volume = t * _musicVolume * _masterVolume;
            inactive.volume = (1f - t) * _musicVolume * _masterVolume;

            if (t >= 1f)
            {
                _isCrossfading = false;
                inactive.Stop();
            }
        }

        void BuildSoundMap()
        {
            _soundMap.Clear();
            if (_audioData == null || _audioData.sounds == null) return;
            foreach (var entry in _audioData.sounds)
                _soundMap[entry.id] = entry;
        }

        public void PlaySFX(SoundId id)
        {
            if (!_soundMap.TryGetValue(id, out var entry)) return;
            if (entry.clip == null) return;

            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;

            source.clip = entry.clip;
            source.volume = entry.volume * _sfxVolume * _masterVolume;
            source.pitch = entry.pitch;
            source.Play();
        }

        public void PlayUI(SoundId id)
        {
            if (!_soundMap.TryGetValue(id, out var entry)) return;
            if (entry.clip == null) return;

            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;

            source.clip = entry.clip;
            source.volume = entry.volume * _uiVolume * _masterVolume;
            source.pitch = entry.pitch;
            source.Play();
        }

        public void PlayMusic(SoundId id)
        {
            if (id == _currentMusicId) return;
            _currentMusicId = id;

            if (!_soundMap.TryGetValue(id, out var entry)) return;
            if (entry.clip == null) return;

            // Crossfade to new track
            _musicAActive = !_musicAActive;
            var newSource = _musicAActive ? _musicSourceA : _musicSourceB;

            newSource.clip = entry.clip;
            newSource.volume = 0f;
            newSource.Play();

            _crossfadeTimer = 0f;
            _isCrossfading = true;
        }

        public void StopMusic()
        {
            _musicSourceA.Stop();
            _musicSourceB.Stop();
            _isCrossfading = false;
            _currentMusicId = default;
        }

        void UpdateVolumes()
        {
            var active = _musicAActive ? _musicSourceA : _musicSourceB;
            if (active.isPlaying)
                active.volume = _musicVolume * _masterVolume;
        }

        // --- Event handlers ---

        void OnTap(TapEvent evt) => PlaySFX(SoundId.TapImpact);
        void OnEnemyDamaged(EnemyDamagedEvent evt) => PlaySFX(SoundId.EnemyHit);
        void OnEnemyKilled(EnemyKilledEvent evt) => PlaySFX(SoundId.EnemyDeath);
        void OnAbilityUsed(AbilityUsedEvent evt) => PlaySFX(SoundId.AbilityActivate);
        void OnLevelUp(LevelUpEvent evt) => PlaySFX(SoundId.LevelUpFanfare);
        void OnBossPhaseChanged(BossPhaseChangedEvent evt) => PlaySFX(SoundId.BossPhaseTransition);
        void OnAscension(AscensionEvent evt) => PlaySFX(SoundId.AscensionFanfare);

        void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Type == CurrencyType.Gold && evt.Delta > 0)
                PlaySFX(SoundId.GoldPickup);
        }

        void OnIslandChanged(IslandChangedEvent evt)
        {
            // Map island biome to music track
            var island = Islands.IslandManager.Instance?.CurrentIsland;
            if (island?.biomeData == null) return;

            SoundId musicId = island.biomeData.biomeName switch
            {
                "Verdant Meadows" => SoundId.MusicVerdantMeadows,
                "Crystal Caverns" => SoundId.MusicCrystalCaverns,
                "Volcanic Forge" => SoundId.MusicVolcanicForge,
                "Storm Peaks" => SoundId.MusicStormPeaks,
                "Shadow Marsh" => SoundId.MusicShadowMarsh,
                "Celestial Spire" => SoundId.MusicCelestialSpire,
                _ => SoundId.MusicVerdantMeadows
            };
            PlayMusic(musicId);
        }

        void OnBossSpawned(IslandBossSpawnedEvent evt) => PlayMusic(SoundId.MusicBossFight);

        void OnBossDefeated(BossDefeatedEvent evt)
        {
            // Return to biome music
            OnIslandChanged(default);
        }

        // --- Placeholder audio generation ---

        void GeneratePlaceholderAudio()
        {
            // Only generate if AudioData has no clips assigned
            if (_audioData != null) return;

            RegisterPlaceholder(SoundId.TapImpact, GenerateSineClip("TapImpact", 800f, 0.05f));
            RegisterPlaceholder(SoundId.EnemyHit, GenerateSineClip("EnemyHit", 400f, 0.08f));
            RegisterPlaceholder(SoundId.EnemyDeath, GenerateNoiseClip("EnemyDeath", 0.15f));
            RegisterPlaceholder(SoundId.AbilityActivate, GenerateSweepClip("AbilityActivate", 300f, 900f, 0.2f));
            RegisterPlaceholder(SoundId.LevelUpFanfare, GenerateArpeggioClip("LevelUp", new[] { 523f, 659f, 784f, 1047f }, 0.6f));
            RegisterPlaceholder(SoundId.GoldPickup, GenerateSineClip("GoldPickup", 1200f, 0.06f));
            RegisterPlaceholder(SoundId.UIClick, GenerateSineClip("UIClick", 600f, 0.03f));
            RegisterPlaceholder(SoundId.BossPhaseTransition, GenerateSweepClip("BossPhase", 100f, 600f, 0.4f));
            RegisterPlaceholder(SoundId.AscensionFanfare, GenerateArpeggioClip("Ascension", new[] { 440f, 554f, 659f, 880f, 1109f }, 1f));

            // Placeholder music loops (simple ambient tones)
            RegisterPlaceholder(SoundId.MusicMenu, GenerateAmbientLoop("MusicMenu", 220f, 4f));
            RegisterPlaceholder(SoundId.MusicVerdantMeadows, GenerateAmbientLoop("MusicMeadows", 262f, 4f));
            RegisterPlaceholder(SoundId.MusicCrystalCaverns, GenerateAmbientLoop("MusicCaverns", 330f, 4f));
            RegisterPlaceholder(SoundId.MusicVolcanicForge, GenerateAmbientLoop("MusicVolcanic", 196f, 4f));
            RegisterPlaceholder(SoundId.MusicStormPeaks, GenerateAmbientLoop("MusicStorm", 392f, 4f));
            RegisterPlaceholder(SoundId.MusicShadowMarsh, GenerateAmbientLoop("MusicShadow", 175f, 4f));
            RegisterPlaceholder(SoundId.MusicCelestialSpire, GenerateAmbientLoop("MusicCelestial", 440f, 4f));
            RegisterPlaceholder(SoundId.MusicBossFight, GenerateAmbientLoop("MusicBoss", 147f, 4f));
        }

        void RegisterPlaceholder(SoundId id, AudioClip clip)
        {
            bool isMusic = id.ToString().StartsWith("Music");
            _soundMap[id] = new AudioData.SoundEntry
            {
                id = id,
                clip = clip,
                volume = isMusic ? 0.5f : 0.8f,
                pitch = 1f,
                isMusic = isMusic
            };
        }

        static AudioClip GenerateSineClip(string name, float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / sampleCount; // linear fade out
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip GenerateNoiseClip(string name, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float envelope = 1f - (float)i / sampleCount;
                samples[i] = Random.Range(-1f, 1f) * envelope * 0.3f;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip GenerateSweepClip(string name, float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, progress);
                float envelope = 1f - progress;
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip GenerateArpeggioClip(string name, float[] notes, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float noteLength = duration / notes.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
                float noteT = (t - noteIndex * noteLength) / noteLength;
                float envelope = Mathf.Max(0f, 1f - noteT * 2f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * envelope * 0.4f;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip GenerateAmbientLoop(string name, float baseFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // Layered sine waves for ambient feel
                float s = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.2f;
                s += Mathf.Sin(2f * Mathf.PI * baseFreq * 1.5f * t) * 0.1f;
                s += Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.05f;
                // Gentle volume swell
                float envelope = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                samples[i] = s * envelope;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // --- PlayerPrefs persistence ---

        void LoadPrefs()
        {
            _masterVolume = PlayerPrefs.GetFloat("audio_master", 1f);
            _musicVolume = PlayerPrefs.GetFloat("audio_music", 0.7f);
            _sfxVolume = PlayerPrefs.GetFloat("audio_sfx", 1f);
            _uiVolume = PlayerPrefs.GetFloat("audio_ui", 0.8f);
        }

        void SavePrefs()
        {
            PlayerPrefs.SetFloat("audio_master", _masterVolume);
            PlayerPrefs.SetFloat("audio_music", _musicVolume);
            PlayerPrefs.SetFloat("audio_sfx", _sfxVolume);
            PlayerPrefs.SetFloat("audio_ui", _uiVolume);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
