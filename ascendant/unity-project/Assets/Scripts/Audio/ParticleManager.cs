using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Audio
{
    public enum ParticleType
    {
        TapImpact,
        EnemyDeath,
        AbilityActivation,
        LevelUp,
        GoldPickup,
        MomentumGlow,
        BossPhaseTransition,
        AscensionStream
    }

    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        [Header("Pool Config")]
        [SerializeField] int _poolSizePerType = 5;

        Dictionary<ParticleType, Queue<ParticleSystem>> _pools = new();
        Transform _poolParent;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _poolParent = new GameObject("ParticlePool").transform;
            _poolParent.SetParent(transform);

            InitializePools();
        }

        void OnEnable()
        {
            EventBus.Subscribe<TapEvent>(OnTap);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<MomentumChangedEvent>(OnMomentumChanged);
            EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Subscribe<AscensionEvent>(OnAscension);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<TapEvent>(OnTap);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<MomentumChangedEvent>(OnMomentumChanged);
            EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Unsubscribe<AscensionEvent>(OnAscension);
        }

        void InitializePools()
        {
            foreach (ParticleType type in System.Enum.GetValues(typeof(ParticleType)))
            {
                _pools[type] = new Queue<ParticleSystem>();
                for (int i = 0; i < _poolSizePerType; i++)
                {
                    var ps = CreateParticleSystem(type);
                    ps.gameObject.SetActive(false);
                    _pools[type].Enqueue(ps);
                }
            }
        }

        public void Spawn(ParticleType type, Vector3 position, Color? color = null)
        {
            if (!_pools.TryGetValue(type, out var pool)) return;

            ParticleSystem ps;
            if (pool.Count > 0)
            {
                ps = pool.Dequeue();
            }
            else
            {
                ps = CreateParticleSystem(type);
            }

            ps.transform.position = position;
            ps.gameObject.SetActive(true);

            if (color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            ps.Play();

            // Return to pool after duration
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPool(ps, type, duration));
        }

        System.Collections.IEnumerator ReturnToPool(ParticleSystem ps, ParticleType type, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
                if (_pools.TryGetValue(type, out var pool))
                    pool.Enqueue(ps);
            }
        }

        ParticleSystem CreateParticleSystem(ParticleType type)
        {
            var go = new GameObject($"PS_{type}");
            go.transform.SetParent(_poolParent);
            var ps = go.AddComponent<ParticleSystem>();

            // Stop auto-play
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            var shape = ps.shape;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            switch (type)
            {
                case ParticleType.TapImpact:
                    main.duration = 0.3f;
                    main.startLifetime = 0.3f;
                    main.startSpeed = 3f;
                    main.startSize = 0.1f;
                    main.startColor = Color.white;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });
                    emission.rateOverTime = 0;
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = 0.1f;
                    EnableSizeOverLifetime(ps);
                    break;

                case ParticleType.EnemyDeath:
                    main.duration = 0.5f;
                    main.startLifetime = 0.5f;
                    main.startSpeed = 2f;
                    main.startSize = 0.15f;
                    main.startColor = Color.red;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });
                    emission.rateOverTime = 0;
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = 0.3f;
                    EnableSizeOverLifetime(ps);
                    EnableColorOverLifetime(ps);
                    break;

                case ParticleType.AbilityActivation:
                    main.duration = 0.4f;
                    main.startLifetime = 0.6f;
                    main.startSpeed = 1f;
                    main.startSize = 0.2f;
                    main.startColor = new Color(0.5f, 0.8f, 1f);
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });
                    emission.rateOverTime = 0;
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = 0.5f;
                    EnableSizeOverLifetime(ps);
                    break;

                case ParticleType.LevelUp:
                    main.duration = 1f;
                    main.startLifetime = 1f;
                    main.startSpeed = 4f;
                    main.startSize = 0.12f;
                    main.startColor = new Color(1f, 0.85f, 0f);
                    main.gravityModifier = -0.5f;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });
                    emission.rateOverTime = 0;
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = 30f;
                    shape.radius = 0.2f;
                    EnableSizeOverLifetime(ps);
                    break;

                case ParticleType.GoldPickup:
                    main.duration = 0.4f;
                    main.startLifetime = 0.6f;
                    main.startSpeed = 2f;
                    main.startSize = 0.08f;
                    main.startColor = new Color(1f, 0.9f, 0.3f);
                    main.gravityModifier = -1f;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6) });
                    emission.rateOverTime = 0;
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = 0.15f;
                    break;

                case ParticleType.MomentumGlow:
                    main.duration = 2f;
                    main.loop = true;
                    main.startLifetime = 1f;
                    main.startSpeed = 0.5f;
                    main.startSize = 0.3f;
                    main.startColor = new Color(1f, 0.5f, 0f, 0.3f);
                    emission.rateOverTime = 10;
                    shape.shapeType = ParticleSystemShapeType.Rectangle;
                    EnableColorOverLifetime(ps);
                    break;

                case ParticleType.BossPhaseTransition:
                    main.duration = 0.6f;
                    main.startLifetime = 0.8f;
                    main.startSpeed = 8f;
                    main.startSize = 0.25f;
                    main.startColor = Color.white;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });
                    emission.rateOverTime = 0;
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = 0.1f;
                    EnableSizeOverLifetime(ps);
                    EnableColorOverLifetime(ps);
                    break;

                case ParticleType.AscensionStream:
                    main.duration = 2f;
                    main.startLifetime = 2f;
                    main.startSpeed = 5f;
                    main.startSize = 0.15f;
                    main.startColor = new Color(1f, 0.85f, 0.3f);
                    main.gravityModifier = -2f;
                    emission.rateOverTime = 40;
                    shape.shapeType = ParticleSystemShapeType.Rectangle;
                    EnableSizeOverLifetime(ps);
                    break;
            }

            return ps;
        }

        void EnableSizeOverLifetime(ParticleSystem ps)
        {
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        }

        void EnableColorOverLifetime(ParticleSystem ps)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = gradient;
        }

        // --- Event handlers ---

        void OnTap(TapEvent evt) =>
            Spawn(ParticleType.TapImpact, evt.WorldPosition);

        void OnEnemyKilled(EnemyKilledEvent evt) =>
            Spawn(ParticleType.EnemyDeath, evt.WorldPosition);

        void OnAbilityUsed(AbilityUsedEvent evt) =>
            Spawn(ParticleType.AbilityActivation, Vector3.zero);

        void OnLevelUp(LevelUpEvent evt) =>
            Spawn(ParticleType.LevelUp, Vector3.zero);

        void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Type == CurrencyType.Gold && evt.Delta > 0)
                Spawn(ParticleType.GoldPickup, Vector3.zero);
        }

        void OnMomentumChanged(MomentumChangedEvent evt)
        {
            // Handled by combat UI momentum glow, no spawning here
        }

        void OnBossPhaseChanged(BossPhaseChangedEvent evt) =>
            Spawn(ParticleType.BossPhaseTransition, Vector3.zero);

        void OnAscension(AscensionEvent evt) =>
            Spawn(ParticleType.AscensionStream, Vector3.zero);

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
