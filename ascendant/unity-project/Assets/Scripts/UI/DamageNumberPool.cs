using UnityEngine;
using Ascendant.Core;
using Ascendant.Utils;

namespace Ascendant.UI
{
    public class DamageNumberPool : MonoBehaviour
    {
        [SerializeField] DamageNumber _prefab;
        [SerializeField] int _initialPoolSize = 20;

        ObjectPool<DamageNumber> _pool;

        void Awake()
        {
            _pool = new ObjectPool<DamageNumber>(_prefab, transform, _initialPoolSize);
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }

        void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            var dmgNum = _pool.Get();
            dmgNum.Show(evt.Damage, evt.WorldPosition, evt.IsCritical, ReturnToPool);
        }

        void ReturnToPool(DamageNumber dmgNum)
        {
            _pool.Return(dmgNum);
        }
    }
}
