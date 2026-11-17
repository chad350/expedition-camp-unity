using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class Rotator : MonoBehaviour
    {
        public float rotateSpeed = 30f;
        public Vector3 axis = Vector3.forward;

        private void Update()
        {
            transform.Rotate(axis, rotateSpeed * Time.deltaTime, Space.Self);
        }
    }
}
