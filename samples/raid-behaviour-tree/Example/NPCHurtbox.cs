using ProjectRaid.Combat;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public class NPCHurtBox : MonoBehaviour, IDamageable
    {
        public Transform Transform => transform;
        private NPCController _npcController;

        private void Awake()
        {
            _npcController = GetComponentInParent<NPCController>();
        }

        public float TakeDamage(DamageInfo info)
        {
            _npcController.TakeDamage(info);
            return info.damageAmount;
        }
    }
}