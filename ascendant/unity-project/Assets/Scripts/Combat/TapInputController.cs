using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Combat
{
    public class TapInputController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] float _combatViewportRatio = 0.6f; // top 60% of screen
        [SerializeField] float _baseTapPower = 10f;

        [Header("References")]
        [SerializeField] MomentumSystem _momentum;

        int _tapCount;
        Heroes.ITapMechanic _activeTapMechanic;
        Heroes.ITapMechanic[] _partyTapMechanics;

        public int TapCount => _tapCount;
        public float BaseTapPower => _baseTapPower;

        public void SetTapMechanic(Heroes.ITapMechanic mechanic)
        {
            _activeTapMechanic = mechanic;
        }

        public void SetPartyTapMechanics(Heroes.ITapMechanic[] mechanics)
        {
            _partyTapMechanics = mechanics;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Combat)
                return;

            ProcessInput();
        }

        void ProcessInput()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                TryTap(Input.mousePosition);
            }
#else
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    TryTap(touch.position);
                }
            }
#endif
        }

        void TryTap(Vector3 screenPos)
        {
            // Only register taps in the combat viewport (top 60%)
            float normalizedY = screenPos.y / Screen.height;
            if (normalizedY < (1f - _combatViewportRatio))
                return;

            _tapCount++;
            _momentum.RegisterTap();

            float heroTapBonus = GetHeroTapBonus();
            float damage = DamageCalculator.CalculateTapDamage(
                _baseTapPower, heroTapBonus, _momentum.Multiplier);

            Vector3 worldPos = GameManager.Instance.MainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 10f));

            // Let all party tap mechanics process
            if (_partyTapMechanics != null)
            {
                foreach (var mechanic in _partyTapMechanics)
                    mechanic?.OnTap(_tapCount, damage, worldPos);
            }
            else
            {
                _activeTapMechanic?.OnTap(_tapCount, damage, worldPos);
            }

            // Deal damage to nearest enemy
            var target = EnemyManager.Instance?.GetNearestEnemy(worldPos);
            if (target != null)
            {
                target.TakeDamage(damage);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = target.Id,
                    Damage = damage,
                    IsCritical = false,
                    IsAoE = false,
                    WorldPosition = target.transform.position
                });
            }

            EventBus.Publish(new TapEvent
            {
                WorldPosition = worldPos,
                Damage = damage,
                TapCount = _tapCount
            });
        }

        float GetHeroTapBonus()
        {
            // Sum tap bonuses from all party heroes
            var partyManager = Party.PartyManager.Instance;
            if (partyManager != null)
            {
                float totalBonus = 0f;
                var heroes = partyManager.GetAllAliveHeroes();
                foreach (var hero in heroes)
                    totalBonus += hero.CurrentAtk;
                return totalBonus;
            }

            // Fallback: single hero
            var primary = Heroes.HeroManager.Instance?.GetPrimaryHero();
            return primary?.CurrentAtk ?? 0f;
        }
    }
}
