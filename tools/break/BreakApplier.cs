using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp.EditorTools
{
    public static class BreakApplier
    {
        public static void Apply()
        {
            string breakId = ReadArgument("-breakId");
            Debug.Log("[Break] " + breakId + " 적용을 시작합니다");
            switch (breakId)
            {
                case "01": RaiseLobbyBackgroundOrder(); break;
                case "02": ClearResultPopup(); break;
                case "03": CenterBagButtonAnchor(); break;
                case "04": ChangeRockShader(); break;
                case "08": MiswireShopSlotIcon(); break;
                case "09": OverrideBridgeCrabSpeed(); break;
                case "10": ApplyBaseAttackRange(); break;
                case "11": UntagRabbit(); break;
                case "12": MoveDropItemToHiddenLayer(); break;
                case "13": TurnOffDropItemTrigger(); break;
                case "14": DeleteLobbyEventSystem(); break;
                case "15": RemoveField02FromBuildList(); break;
                case "16": SaveConfigTimeScale(); break;
                default:
                    Debug.LogError("[Break] 알 수 없는 번호입니다: " + breakId);
                    EditorApplication.Exit(1);
                    return;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Break] " + breakId + " 적용을 마쳤습니다");
        }

        private static string ReadArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] == name)
                {
                    return arguments[index + 1];
                }
            }
            return "";
        }

        private static Scene Open(string sceneName)
        {
            return EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
        }

        private static GameObject FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == name)
                    {
                        return child.gameObject;
                    }
                }
            }
            throw new InvalidOperationException("오브젝트를 찾을 수 없습니다: " + name);
        }

        private static void Save(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ClearResultPopup()
        {
            Scene scene = Open("Field_01");
            FieldManager manager = FindInScene(scene, "FieldManager").GetComponent<FieldManager>();
            SerializedObject serialized = new SerializedObject(manager);
            serialized.FindProperty("resultPopup").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Save(scene);
        }

        private static void OverrideBridgeCrabSpeed()
        {
            Scene scene = Open("Field_01");
            Monster crab = FindInScene(scene, "Monster_Crab_Bridge").GetComponent<Monster>();
            SerializedObject serialized = new SerializedObject(crab);
            serialized.FindProperty("moveSpeed").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(crab);
            Save(scene);
        }

        private static void ApplyBaseAttackRange()
        {
            string path = "Assets/Prefabs/Monster_Base.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            contents.GetComponent<Monster>().attackRange = 30f;
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void UntagRabbit()
        {
            string path = "Assets/Resources/Monsters/Monster_Rabbit.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            contents.tag = "Untagged";
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void MoveDropItemToHiddenLayer()
        {
            string path = "Assets/Prefabs/DropItem.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            int hidden = LayerMask.NameToLayer("Hidden");
            foreach (Transform child in contents.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = hidden;
            }
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void TurnOffDropItemTrigger()
        {
            string path = "Assets/Prefabs/DropItem.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            contents.GetComponent<SphereCollider>().isTrigger = false;
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void RaiseLobbyBackgroundOrder()
        {
            Scene scene = Open("Lobby");
            FindInScene(scene, "Background").GetComponent<SpriteRenderer>().sortingOrder = 10;
            Save(scene);
        }

        private static void CenterBagButtonAnchor()
        {
            Scene scene = Open("Lobby");
            RectTransform rect = (RectTransform)FindInScene(scene, "Btn_Bag").transform;
            Vector2 size = rect.sizeDelta;
            Vector2 referenceSize = new Vector2(1920f, 1080f);
            Vector2 oldAnchor = rect.anchorMin;
            Vector2 offset = new Vector2((oldAnchor.x - 0.5f) * referenceSize.x, (oldAnchor.y - 0.5f) * referenceSize.y);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = rect.anchoredPosition + offset;
            rect.sizeDelta = size;
            Save(scene);
        }

        private static void DeleteLobbyEventSystem()
        {
            Scene scene = Open("Lobby");
            UnityEngine.Object.DestroyImmediate(FindInScene(scene, "EventSystem"));
            Save(scene);
        }

        private static void RemoveField02FromBuildList()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path.EndsWith("/Field_02.unity"))
                {
                    continue;
                }
                scenes.Add(scene);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SaveConfigTimeScale()
        {
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Settings/GameConfig.asset");
            config.timeScale = 3f;
            EditorUtility.SetDirty(config);
        }

        private static void ChangeRockShader()
        {
            Material rock = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Rock.mat");
            rock.shader = Shader.Find("Standard");
            EditorUtility.SetDirty(rock);
        }

        private static void MiswireShopSlotIcon()
        {
            // 상점 칸 프리팹의 Icon 그림 칸에 작업 중 임시로 넣은 몬스터 그림이 남아 있고,
            // 스크립트의 Icon 칸은 구매 버튼의 Image 에 잘못 연결된 경우입니다.
            string path = "Assets/Prefabs/UI/UI_ShopSlot.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            UnityEngine.UI.Image iconImage = contents.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image buyImage = contents.transform.Find("Btn_Buy").GetComponent<UnityEngine.UI.Image>();
            Sprite monsterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Icons/Monster/mon_100001.png");
            if (monsterSprite == null)
            {
                throw new InvalidOperationException("몬스터 그림을 찾을 수 없습니다");
            }
            iconImage.sprite = monsterSprite;
            SerializedObject view = new SerializedObject(contents.GetComponent<ShopSlotView>());
            view.FindProperty("icon").objectReferenceValue = buyImage;
            view.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
