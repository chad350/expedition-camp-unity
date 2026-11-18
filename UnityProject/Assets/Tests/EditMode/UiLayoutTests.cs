using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.Tests
{
    // 여러 해상도에서 클릭 대상(버튼 · 토글)이 서로 겹치거나 화면 밖으로 나가는지 확인합니다.
    // 실제 기기를 쓰지 않고, Canvas Scaler 계산식으로 해상도별 Canvas 크기를 만들어 배치를 계산합니다.
    public class UiLayoutTests
    {
        private static readonly string[] Scenes =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/Field_01.unity",
            "Assets/Scenes/Field_02.unity",
        };

        // 검사할 화면 크기 (가로, 세로) · 대표 기기 비율
        private static readonly int[][] Resolutions =
        {
            new[] { 1920, 1080 },  // 16:9 PC · 기준 해상도
            new[] { 1280, 720 },   // 16:9 저해상도
            new[] { 2340, 1080 },  // 19.5:9 휴대폰
            new[] { 2560, 1080 },  // 21:9 와이드 모니터
            new[] { 2560, 1600 },  // 16:10 태블릿 · 노트북
            new[] { 2048, 1536 },  // 4:3 태블릿
        };

        private const float Tolerance = 1f;
        private static int checkedTargets;

        private static IEnumerable<TestCaseData> Cases()
        {
            foreach (int[] size in Resolutions)
            {
                yield return new TestCaseData(size[0], size[1]).SetName("UiDoesNotOverlap_" + size[0] + "x" + size[1]);
            }
        }

        [TestCaseSource(nameof(Cases))]
        public void ClickTargetsDoNotOverlapOrLeaveScreen(int width, int height)
        {
            List<string> problems = new List<string>();
            checkedTargets = 0;
            foreach (string scenePath in Scenes)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                    {
                        if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            CheckCanvas(scene.name, canvas, width, height, problems);
                        }
                    }
                }
                EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log("[UiLayout] " + width + "x" + height + " 클릭 대상 " + checkedTargets + "개 검사");
            Assert.GreaterOrEqual(checkedTargets, 10, "검사한 클릭 대상이 너무 적습니다. 씬 · 팝업 구성이 바뀌었는지 확인합니다.");
            Assert.IsEmpty(problems, width + "x" + height + " 에서 문제가 있습니다\n" + string.Join("\n", problems));
        }

        private static Vector2 CanvasSize(Canvas canvas, int width, int height)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return new Vector2(width, height);
            }
            Vector2 reference = scaler.referenceResolution;
            float logWidth = Mathf.Log(width / reference.x, 2f);
            float logHeight = Mathf.Log(height / reference.y, 2f);
            float scale = Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight, scaler.matchWidthOrHeight));
            return new Vector2(width / scale, height / scale);
        }

        private static void CheckCanvas(string sceneName, Canvas canvas, int width, int height, List<string> problems)
        {
            RectTransform canvasRect = (RectTransform)canvas.transform;
            canvas.renderMode = RenderMode.WorldSpace;
            canvasRect.localScale = Vector3.one;
            canvasRect.position = Vector3.zero;
            canvasRect.sizeDelta = CanvasSize(canvas, width, height);
            Rect screen = WorldRect(canvasRect);

            // 처음부터 보이는 화면을 1묶음, 팝업은 1개씩 열었다고 보고 각각 1묶음으로 검사합니다.
            List<Transform> popups = new List<Transform>();
            foreach (Transform child in canvasRect)
            {
                if (child.name.StartsWith("Popup_"))
                {
                    popups.Add(child);
                }
            }
            Dictionary<Transform, bool> popupStates = new Dictionary<Transform, bool>();
            foreach (Transform popup in popups)
            {
                popupStates[popup] = popup.gameObject.activeSelf;
                popup.gameObject.SetActive(false);
            }

            CheckGroup(sceneName, "기본 화면", canvasRect, screen, problems);
            foreach (Transform popup in popups)
            {
                popup.gameObject.SetActive(true);
                CheckGroup(sceneName, popup.name, (RectTransform)popup, screen, problems);
                popup.gameObject.SetActive(false);
            }

            foreach (KeyValuePair<Transform, bool> state in popupStates)
            {
                state.Key.gameObject.SetActive(state.Value);
            }
        }

        private static void CheckGroup(string sceneName, string groupName, RectTransform groupRoot, Rect screen, List<string> problems)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRoot);

            List<Selectable> targets = new List<Selectable>();
            foreach (Selectable selectable in groupRoot.GetComponentsInChildren<Selectable>(false))
            {
                if ((selectable is Button || selectable is Toggle) && selectable.gameObject.activeInHierarchy)
                {
                    targets.Add(selectable);
                }
            }

            checkedTargets += targets.Count;
            string prefix = sceneName + " · " + groupName + " · ";
            for (int i = 0; i < targets.Count; i++)
            {
                Rect a = WorldRect((RectTransform)targets[i].transform);
                if (a.xMin < screen.xMin - Tolerance || a.yMin < screen.yMin - Tolerance
                    || a.xMax > screen.xMax + Tolerance || a.yMax > screen.yMax + Tolerance)
                {
                    problems.Add(prefix + targets[i].name + " 가 화면 밖으로 나갑니다");
                }
                for (int j = i + 1; j < targets.Count; j++)
                {
                    Transform ta = targets[i].transform;
                    Transform tb = targets[j].transform;
                    if (ta.IsChildOf(tb) || tb.IsChildOf(ta))
                    {
                        continue;
                    }
                    Rect b = WorldRect((RectTransform)tb);
                    float overlapWidth = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin);
                    float overlapHeight = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
                    if (overlapWidth > Tolerance && overlapHeight > Tolerance)
                    {
                        problems.Add(prefix + ta.name + " 와 " + tb.name + " 가 겹칩니다 (" + overlapWidth.ToString("0") + " x " + overlapHeight.ToString("0") + ")");
                    }
                }
            }
        }

        private static Rect WorldRect(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
