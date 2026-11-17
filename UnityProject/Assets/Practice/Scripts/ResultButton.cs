using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class ResultButton : MonoBehaviour
    {
        [SerializeField] private GameObject resultPanel;

        public void Show()
        {
            resultPanel.SetActive(true);
            Debug.Log("[Practice] 결과 창을 열었습니다");
        }
    }
}
