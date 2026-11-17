using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp.Practice
{
    public class ErrorButtons : MonoBehaviour
    {
        [SerializeField] private string noticeName = "Popup_Notice";
        [SerializeField] private GameObject removedOnStart;
        [SerializeField] private string sceneName = "Shop_Old";

        private void Start()
        {
            Destroy(removedOnStart);
        }

        public void RaiseNullReference()
        {
            GameObject notice = GameObject.Find(noticeName);
            notice.SetActive(true);
        }

        public void RaiseMissingReference()
        {
            Debug.Log("[Practice] 삭제된 오브젝트 위치 " + removedOnStart.transform.position);
        }

        public void LoadMissingScene()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
