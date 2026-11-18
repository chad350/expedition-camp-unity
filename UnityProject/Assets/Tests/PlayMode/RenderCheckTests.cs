using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ExpeditionCamp.Tests
{
    // 실행 중 화면에 분홍색(오류 셰이더)이 보이는지 확인합니다.
    // Unity 에디터는 그릴 수 없는 재질을 분홍색(255, 0, 255)으로 표시합니다.
    // 빌드에서는 같은 재질이 분홍색이 아니라 아예 보이지 않을 수 있어서, 에디터 실행 화면으로 확인합니다.
    public class RenderCheckTests
    {
        private const int Width = 1280;
        private const int Height = 720;
        private const int MaxPinkPixels = 200;
        private const float SceneTimeoutSecond = 15f;

        [SetUp]
        public void ResetState()
        {
            SaveSystem.ClearForTest();
            GameSession.ResetSession();
            GameData.Reload();
        }

        [UnityTest]
        public IEnumerator LobbyHasNoErrorPink()
        {
            yield return LoadAndCheck("Lobby", 0);
        }

        [UnityTest]
        public IEnumerator Field01HasNoErrorPink()
        {
            yield return LoadAndCheck("Field_01", 401001);
        }

        [UnityTest]
        public IEnumerator Field02HasNoErrorPink()
        {
            yield return LoadAndCheck("Field_02", 401006);
        }

        private static IEnumerator LoadAndCheck(string sceneName, int stageId)
        {
            if (stageId != 0)
            {
                GameSession.CurrentStageId = stageId;
            }
            SceneManager.LoadScene(sceneName);
            float elapsed = 0f;
            while (SceneManager.GetActiveScene().name != sceneName && elapsed < SceneTimeoutSecond)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name, sceneName + " 씬으로 이동하지 않았습니다");
            for (int frame = 0; frame < 10; frame++)
            {
                yield return null;
            }

            Camera camera = Camera.main;
            Assert.IsNotNull(camera, sceneName + " 에 Main Camera 가 없습니다");
            int pink = CountPinkPixels(camera);
            Debug.Log("[RenderCheck] " + sceneName + " 분홍색 픽셀 " + pink + "개");
            Assert.LessOrEqual(pink, MaxPinkPixels, sceneName + " 화면에 분홍색 픽셀이 " + pink + "개 있습니다. 재질의 Shader 를 확인합니다.");
        }

        private static int CountPinkPixels(Camera camera)
        {
            RenderTexture texture = new RenderTexture(Width, Height, 24);
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            Texture2D image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;

            int count = 0;
            foreach (Color32 pixel in image.GetPixels32())
            {
                if (pixel.r >= 250 && pixel.g <= 5 && pixel.b >= 250)
                {
                    count++;
                }
            }
            Object.Destroy(image);
            texture.Release();
            Object.Destroy(texture);
            return count;
        }
    }
}
