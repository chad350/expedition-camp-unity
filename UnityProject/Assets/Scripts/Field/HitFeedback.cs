using System.Collections;
using UnityEngine;

namespace ExpeditionCamp
{
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float flashSecond = 0.12f;

        private Vector3 _baseScale;
        private Coroutine _flash;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        public void Flash()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            if (_flash != null)
            {
                StopCoroutine(_flash);
            }
            _flash = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            target.color = flashColor;
            transform.localScale = _baseScale * 1.12f;
            yield return new WaitForSeconds(flashSecond);
            target.color = Color.white;
            transform.localScale = _baseScale;
            _flash = null;
        }

        public IEnumerator FadeOut(float second)
        {
            float elapsed = 0f;
            Color start = Color.white;
            while (elapsed < second)
            {
                elapsed += Time.deltaTime;
                float rate = Mathf.Clamp01(elapsed / second);
                target.color = new Color(start.r, start.g, start.b, 1f - rate);
                transform.localScale = _baseScale * (1f - rate * 0.6f);
                yield return null;
            }
        }
    }
}
