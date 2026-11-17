using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class SoundButton : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        public void Play()
        {
            audioSource.Play();
            Debug.Log("[Practice] 효과음을 재생했습니다");
        }
    }
}
