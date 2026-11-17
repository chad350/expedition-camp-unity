using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.Tests
{
    public class UiThemeTests
    {
        private const string KenneyFolder = "Assets/ThirdParty/Kenney/";

        private static List<string> FindKenneyImages(IEnumerable<Image> images)
        {
            List<string> found = new List<string>();
            foreach (Image image in images)
            {
                if (image.sprite != null && AssetDatabase.GetAssetPath(image.sprite).StartsWith(KenneyFolder))
                {
                    found.Add(image.name);
                }
            }
            return found;
        }

        [Test]
        public void UiPrefabsUseThemeInsteadOfKenneySprites()
        {
            List<string> found = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/UI" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (string name in FindKenneyImages(prefab.GetComponentsInChildren<Image>(true)))
                {
                    found.Add(path + " > " + name);
                }
            }

            Assert.IsEmpty(found, "테마가 적용되지 않은 이미지: " + string.Join(", ", found));
        }

        [TestCase("Assets/Scenes/Boot.unity")]
        [TestCase("Assets/Scenes/Lobby.unity")]
        [TestCase("Assets/Scenes/Field_01.unity")]
        [TestCase("Assets/Scenes/Field_02.unity")]
        public void GameScenesUseThemeInsteadOfKenneySprites(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            List<Image> images = new List<Image>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                images.AddRange(root.GetComponentsInChildren<Image>(true));
            }

            List<string> found = FindKenneyImages(images);
            EditorSceneManager.CloseScene(scene, true);

            Assert.IsEmpty(found, "테마가 적용되지 않은 이미지: " + string.Join(", ", found));
        }
    }
}
