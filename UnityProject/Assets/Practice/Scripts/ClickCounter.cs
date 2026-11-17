using TMPro;
using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class ClickCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private string caption = "버튼";

        private int _count;

        public void Count()
        {
            _count++;
            label.text = caption + " " + _count;
            Debug.Log("[Practice] " + gameObject.name + " 클릭 " + _count);
        }
    }
}
