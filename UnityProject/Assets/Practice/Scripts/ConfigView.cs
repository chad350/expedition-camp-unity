using TMPro;
using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class ConfigView : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Transform spinner;

        private void Update()
        {
            Time.timeScale = config.timeScale;
            valueText.text = "GameConfig Time Scale " + config.timeScale.ToString("0.##") + "\nCamera Distance " + config.cameraDistance.ToString("0.#");
            if (spinner != null)
            {
                spinner.Rotate(0f, 0f, 90f * Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }
    }
}
