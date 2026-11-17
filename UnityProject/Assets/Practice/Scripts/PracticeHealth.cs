using System.Collections;
using TMPro;
using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class PracticeHealth : MonoBehaviour
    {
        public int maxHp = 100;
        public int hp = 100;
        public int damagePerClick = 10;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private SpriteRenderer body;

        private bool _dead;

        private void Update()
        {
            hpText.text = _dead ? "처치됨" : "HP " + hp + " / " + maxHp;
        }

        public void TakeDamage()
        {
            if (_dead)
            {
                return;
            }
            hp = Mathf.Max(0, hp - damagePerClick);
            Debug.Log("[Practice] 몬스터 체력 " + hp);
            if (hp <= 0)
            {
                _dead = true;
                StartCoroutine(DieRoutine());
            }
        }

        private IEnumerator DieRoutine()
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                float rate = elapsed / 0.6f;
                transform.localScale = start * (1f + rate * 0.5f);
                body.color = new Color(1f, 1f, 1f, 1f - rate);
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
