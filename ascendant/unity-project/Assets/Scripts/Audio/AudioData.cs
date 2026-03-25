using UnityEngine;

namespace Ascendant.Audio
{
    public enum SoundId
    {
        // SFX
        TapImpact,
        EnemyHit,
        EnemyDeath,
        AbilityActivate,
        LevelUpFanfare,
        GoldPickup,
        UIClick,
        BossPhaseTransition,
        AscensionFanfare,

        // Music
        MusicMenu,
        MusicVerdantMeadows,
        MusicCrystalCaverns,
        MusicVolcanicForge,
        MusicStormPeaks,
        MusicShadowMarsh,
        MusicCelestialSpire,
        MusicBossFight
    }

    [CreateAssetMenu(fileName = "AudioData", menuName = "Ascendant/Audio Data")]
    public class AudioData : ScriptableObject
    {
        [System.Serializable]
        public class SoundEntry
        {
            public SoundId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.5f, 2f)] public float pitch = 1f;
            public bool isMusic;
        }

        public SoundEntry[] sounds;
    }
}
