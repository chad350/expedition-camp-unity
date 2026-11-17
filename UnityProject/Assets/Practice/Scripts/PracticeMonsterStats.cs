using TMPro;
using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class PracticeMonsterStats : MonoBehaviour
    {
        [Min(0f)] public float moveSpeed = 2f;
        [Min(0f)] public float attackRange = 2f;
        [Min(0.1f)] public float attackInterval = 1.5f;

        [SerializeField] private Transform rangeCircle;
        [SerializeField] private TMP_Text label;

        private void Update()
        {
            if (rangeCircle != null)
            {
                rangeCircle.localScale = new Vector3(attackRange * 2f, attackRange * 2f, 1f);
            }
            if (label != null)
            {
                label.text = gameObject.name + "\n공격 범위 " + attackRange.ToString("0.0");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
