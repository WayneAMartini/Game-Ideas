using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Combat
{
    public class Enemy : MonoBehaviour
    {
        static int _nextId;

        [SerializeField] SpriteRenderer _spriteRenderer;

        EnemyData _data;
        int _stage;
        float _currentHp;
        float _maxHp;
        float _atk;
        float _def;
        float _goldDrop;
        float _xpDrop;
        bool _isDead;

        public int Id { get; private set; }
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public float Atk => _atk;
        public float Def => _def;
        public bool IsDead => _isDead;
        public Affinity Affinity => _data != null ? _data.affinity : Affinity.None;
        public EnemyCategory Category => _data != null ? _data.category : EnemyCategory.Humanoid;
        public string EnemyName => _data != null ? _data.enemyName : "Unknown";
        public EnemyAttackType AttackType => _data != null ? _data.attackType : EnemyAttackType.Melee;

        public void Initialize(EnemyData data, int stage)
        {
            Id = _nextId++;
            _data = data;
            _stage = stage;
            _isDead = false;

            _maxHp = data.GetHp(stage);
            _currentHp = _maxHp;
            _atk = data.GetAtk(stage);
            _def = data.GetDef(stage);
            _goldDrop = data.GetGoldDrop(stage);
            _xpDrop = data.GetXpDrop(stage);

            if (_spriteRenderer != null && data.sprite != null)
                _spriteRenderer.sprite = data.sprite;
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _currentHp -= damage;

            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                Die();
            }
        }

        // Overload with damage type for type advantage calculations
        public void TakeDamage(float damage, DamageType damageType)
        {
            if (_isDead) return;

            float typeMultiplier = DamageTypeHelper.GetTypeMultiplier(damageType, Category);
            float finalDamage = damage * typeMultiplier;

            _currentHp -= finalDamage;

            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                Die();
            }
        }

        void Die()
        {
            _isDead = true;

            EventBus.Publish(new EnemyKilledEvent
            {
                EnemyId = Id,
                GoldReward = _goldDrop,
                XpReward = _xpDrop,
                WorldPosition = transform.position
            });

            // Simple death: disable after a short delay for VFX
            Destroy(gameObject, 0.1f);
        }

        public static void ResetIdCounter()
        {
            _nextId = 0;
        }
    }
}
