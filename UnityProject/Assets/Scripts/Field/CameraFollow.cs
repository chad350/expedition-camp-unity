using UnityEngine;

namespace ExpeditionCamp
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform target;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            Snap();
        }

        private void Start()
        {
            Snap();
        }

        private void LateUpdate()
        {
            Snap();
        }

        public void Snap()
        {
            if (config == null || target == null)
            {
                return;
            }
            Vector3 focus = target.position + Vector3.up * config.cameraLookHeight;
            transform.position = CalculatePosition(focus, config);
            transform.LookAt(focus);
        }

        public static Vector3 CalculatePosition(Vector3 focus, GameConfig config)
        {
            Quaternion rotation = Quaternion.Euler(config.cameraPitch, config.cameraYaw, 0f);
            return focus + rotation * new Vector3(0f, 0f, -config.cameraDistance);
        }
    }
}
