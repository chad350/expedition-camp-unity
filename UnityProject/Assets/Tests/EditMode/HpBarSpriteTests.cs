using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.Tests
{
    public class HpBarSpriteTests
    {
        private static List<string> FindFilledWithoutSprite(IEnumerable<Image> images)
        {
            List<string> missing = new List<string>();
            foreach (Image image in images)
            {
                if (image.type == Image.Type.Filled && image.sprite == null)
                {
                    missing.Add(image.name);
                }
            }
            return missing;
        }

        [Test]
        public void MonsterBaseHpFillHasSprite()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Monster_Base.prefab");

            List<string> missing = FindFilledWithoutSprite(prefab.GetComponentsInChildren<Image>(true));

            Assert.IsEmpty(missing, "Filled 이미지에 Sprite 가 없습니다: " + string.Join(", ", missing));
        }

        [TestCase("Assets/Scenes/Field_01.unity")]
        [TestCase("Assets/Scenes/Field_02.unity")]
        public void FieldHpFillHasSprite(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            List<Image> images = new List<Image>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                images.AddRange(root.GetComponentsInChildren<Image>(true));
            }

            List<string> missing = FindFilledWithoutSprite(images);
            EditorSceneManager.CloseScene(scene, true);

            Assert.IsEmpty(missing, "Filled 이미지에 Sprite 가 없습니다: " + string.Join(", ", missing));
        }
    }
}
