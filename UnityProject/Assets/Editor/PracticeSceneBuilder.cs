using ExpeditionCamp.Practice;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.EditorTools
{
    public static class PracticeSceneBuilder
    {
        public const string ScenesFolder = "Assets/Scenes/Practice";
        public const string PracticeFolder = "Assets/Practice";
        public const string MaterialFolder = PracticeFolder + "/Materials";
        public const string PrefabFolder = PracticeFolder + "/Prefabs";
        public const string PropsFolder = "Assets/Sprites/Props";

        [MenuItem("ExpeditionCamp/Rebuild Practice Scenes")]
        public static void BuildAll()
        {
            EditorUtil.EnsureFolder(ScenesFolder);
            BuildMaterials();
            BuildPrefabs();
            BuildP1();
            BuildP2();
            BuildP3();
            BuildP4();
            BuildP5();
            BuildP6();
            AssetDatabase.SaveAssets();
            Debug.Log("[PracticeBuilder] 연습 씬 6개를 만들었습니다");
        }

        private static Material Mat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + name + ".mat");
        }

        private static Sprite Prop(string fileName)
        {
            return EditorUtil.LoadSprite(PropsFolder + "/" + fileName + ".png");
        }

        private static void BuildMaterials()
        {
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Ground.mat", new Color(0.55f, 0.66f, 0.45f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Stone.mat", new Color(0.6f, 0.6f, 0.64f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Wood.mat", new Color(0.62f, 0.42f, 0.24f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Red.mat", new Color(0.85f, 0.25f, 0.22f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Blue.mat", new Color(0.25f, 0.45f, 0.85f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Yellow.mat", new Color(0.95f, 0.80f, 0.25f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Leaves.mat", new Color(0.25f, 0.55f, 0.25f));
            EditorUtil.LitMaterial(MaterialFolder + "/Practice_Purple.mat", new Color(0.55f, 0.35f, 0.80f));
            EditorUtil.ShaderMaterial(MaterialFolder + "/Practice_OldShader.mat", "Standard", new Color(0.8f, 0.8f, 0.8f));
        }

        private static Scene NewScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.58f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.40f, 0.40f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.20f, 0.18f);
            return scene;
        }

        private static void Save(Scene scene, string sceneName)
        {
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/" + sceneName + ".unity");
        }

        private static Camera MainCamera(Vector3 position, Vector3 euler, bool orthographic, float size)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.62f, 0.76f, 0.90f);
            camera.orthographic = orthographic;
            camera.orthographicSize = size;
            camera.fieldOfView = 45f;
            cameraObject.transform.position = position;
            cameraObject.transform.rotation = Quaternion.Euler(euler);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void AddLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static TMP_Text WorldLabel(string name, Transform parent, string content, Vector3 localPosition, float size)
        {
            GameObject labelObject = new GameObject(name);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            TextMeshPro text = labelObject.AddComponent<TextMeshPro>();
            text.font = EditorUtil.Font;
            text.text = content;
            text.fontSize = size;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.1f, 0.1f, 0.12f);
            text.rectTransform.sizeDelta = new Vector2(6f, 2f);
            text.sortingOrder = 10;
            return text;
        }

        // 연습용 프리팹: 게임 프리팹과 공유하지 않습니다.

        private static void BuildPrefabs()
        {
            GameObject tree = new GameObject("Tree_Practice");
            GameObject trunk = EditorUtil.Primitive(PrimitiveType.Cylinder, "Trunk", tree.transform, new Vector3(0f, 1f, 0f),
                new Vector3(0.4f, 1f, 0.4f), Mat("Practice_Wood"), 0);
            GameObject leaves = EditorUtil.Primitive(PrimitiveType.Sphere, "Leaves", tree.transform, new Vector3(0f, 2.5f, 0f),
                new Vector3(1.8f, 1.6f, 1.8f), Mat("Practice_Leaves"), 0);
            Object.DestroyImmediate(leaves.GetComponent<Collider>());
            Object.DestroyImmediate(trunk.GetComponent<Collider>());
            EditorUtil.SavePrefab(tree, PrefabFolder + "/Tree_Practice.prefab");

            GameObject monster = new GameObject("Monster_Practice");
            GameObject body = EditorUtil.Primitive(PrimitiveType.Capsule, "Body", monster.transform, new Vector3(0f, 1f, 0f),
                Vector3.one, Mat("Practice_Red"), 0);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            monster.AddComponent<CapsuleCollider>().center = new Vector3(0f, 1f, 0f);
            SpriteRenderer circle = EditorUtil.SpriteObject("RangeCircle", monster.transform,
                EditorUtil.LoadSprite(PrefabBuilder.CircleSprite), new Vector3(0f, 0.03f, 0f), 1f, 0, 0);
            circle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            circle.color = new Color(1f, 0.3f, 0.3f, 0.25f);
            circle.drawMode = SpriteDrawMode.Simple;
            TMP_Text label = WorldLabel("Label", monster.transform, "", new Vector3(0f, 2.6f, 0f), 3f);
            PracticeMonsterStats stats = monster.AddComponent<PracticeMonsterStats>();
            stats.moveSpeed = 2f;
            stats.attackRange = 2f;
            stats.attackInterval = 1.5f;
            EditorUtil.SetField(stats, "rangeCircle", circle.transform);
            EditorUtil.SetField(stats, "label", label);
            label.gameObject.AddComponent<Billboard>();
            EditorUtil.SavePrefab(monster, PrefabFolder + "/Monster_Practice.prefab");

            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Monster_Practice.prefab");
            GameObject fire = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            fire.name = "Monster_Practice_Fire";
            fire.transform.Find("Body").GetComponent<MeshRenderer>().sharedMaterial = Mat("Practice_Yellow");
            EditorUtil.SavePrefab(fire, PrefabFolder + "/Monster_Practice_Fire.prefab");

            GameObject ice = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            ice.name = "Monster_Practice_Ice";
            ice.transform.Find("Body").GetComponent<MeshRenderer>().sharedMaterial = Mat("Practice_Blue");
            ice.GetComponent<PracticeMonsterStats>().attackRange = 1.2f;
            EditorUtil.SavePrefab(ice, PrefabFolder + "/Monster_Practice_Ice.prefab");
        }

        // P1_Editor

        private static void BuildP1()
        {
            Scene scene = NewScene();
            MainCamera(new Vector3(0f, 6f, -11f), new Vector3(28f, 0f, 0f), false, 5f);
            AddLight();

            EditorUtil.Primitive(PrimitiveType.Cube, "Ground", null, new Vector3(0f, -0.5f, 0f), new Vector3(16f, 1f, 10f), Mat("Practice_Ground"), 0);
            EditorUtil.SpriteObject("Crate_01", null, Prop("crate"), new Vector3(-5f, 0.8f, 1f), 0.6f, 0, 0);
            EditorUtil.SpriteObject("Crate_02", null, Prop("crate"), new Vector3(-3.6f, 0.8f, 1f), 0.6f, 0, 0);
            EditorUtil.SpriteObject("Crate_03", null, Prop("crate"), new Vector3(-4.3f, 2.2f, 1f), 0.6f, 0, 0);
            EditorUtil.SpriteObject("Barrel", null, Prop("barrel"), new Vector3(-1.5f, 0.9f, 1f), 0.7f, 0, 0);
            EditorUtil.SpriteObject("Signpost", null, Prop("signpost"), new Vector3(1f, 1.1f, 1f), 0.8f, 0, 0);
            EditorUtil.SpriteObject("Lantern", null, Prop("lantern"), new Vector3(5.5f, 1f, 1f), 0.7f, 0, 0);
            EditorUtil.Primitive(PrimitiveType.Sphere, "Rock_Big", null, new Vector3(3.2f, 0.6f, 2.5f), new Vector3(1.8f, 1.2f, 1.6f), Mat("Practice_Stone"), 0);
            EditorUtil.Primitive(PrimitiveType.Sphere, "Rock_Small", null, new Vector3(4.4f, 0.3f, 0f), new Vector3(0.8f, 0.6f, 0.8f), Mat("Practice_Stone"), 0);
            EditorUtil.Primitive(PrimitiveType.Cube, "Pillar", null, new Vector3(-6.5f, 1.5f, 3f), new Vector3(0.8f, 3f, 0.8f), Mat("Practice_Stone"), 0);
            Save(scene, "P1_Editor");
        }

        // P2_Objects

        private static void BuildP2()
        {
            Scene scene = NewScene();
            MainCamera(new Vector3(0f, 7f, -12f), new Vector3(30f, 0f, 0f), false, 5f);
            AddLight();
            EditorUtil.Primitive(PrimitiveType.Cube, "Ground", null, new Vector3(0f, -0.5f, 0f), new Vector3(20f, 1f, 12f), Mat("Practice_Ground"), 0);

            GameObject wagon = EditorUtil.Empty("Wagon", null, new Vector3(-4f, 0f, 0f));
            EditorUtil.Primitive(PrimitiveType.Cube, "Wagon_Body", wagon.transform, new Vector3(0f, 1.1f, 0f), new Vector3(3f, 1f, 1.6f), Mat("Practice_Wood"), 0);
            WheelAt("Wheel_FL", wagon.transform, new Vector3(1f, 0.5f, -0.9f));
            WheelAt("Wheel_FR", wagon.transform, new Vector3(1f, 0.5f, 0.9f));
            WheelAt("Wheel_RL", wagon.transform, new Vector3(-1f, 0.5f, -0.9f));
            WheelAt("Wheel_RR", wagon.transform, new Vector3(-1f, 0.5f, 0.9f));

            GameObject lamp = EditorUtil.Primitive(PrimitiveType.Cylinder, "Lamp_Post", null, new Vector3(0f, 1.2f, 2f), new Vector3(0.3f, 1.2f, 0.3f), Mat("Practice_Yellow"), 0);
            lamp.SetActive(false);

            GameObject fenceGroup = EditorUtil.Empty("Fence_Group", null, new Vector3(3f, 0f, 2f));
            EditorUtil.Primitive(PrimitiveType.Cube, "Fence_01", fenceGroup.transform, new Vector3(0f, 0.5f, 0f), new Vector3(2.5f, 1f, 0.2f), Mat("Practice_Wood"), 0);
            fenceGroup.SetActive(false);

            EditorUtil.Primitive(PrimitiveType.Cube, "Box_Scale", null, new Vector3(1.5f, 0.5f, -1.5f), Vector3.zero, Mat("Practice_Red"), 0);

            EditorUtil.Primitive(PrimitiveType.Cylinder, "Flag", null, new Vector3(60f, 1.5f, 0f), new Vector3(0.2f, 1.5f, 0.2f), Mat("Practice_Blue"), 0);

            GameObject cart = EditorUtil.Empty("Cart_Group", null, new Vector3(5.5f, 0f, -1f));
            EditorUtil.Primitive(PrimitiveType.Cube, "Hay_Bale", cart.transform, new Vector3(0f, 0.5f, 0f), new Vector3(1.4f, 1f, 1f), Mat("Practice_Yellow"), 0);
            cart.transform.localScale = Vector3.zero;

            Save(scene, "P2_Objects");
        }

        private static void WheelAt(string name, Transform parent, Vector3 position)
        {
            GameObject wheel = EditorUtil.Primitive(PrimitiveType.Cylinder, name, parent, position, new Vector3(0.9f, 0.1f, 0.9f), Mat("Practice_Stone"), 0);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        // P3_Components

        private static void BuildP3()
        {
            Scene scene = NewScene();
            MainCamera(new Vector3(0f, 0f, -10f), Vector3.zero, true, 5.4f);

            SpriteRenderer tower = EditorUtil.SpriteObject("Windmill_Tower", null, Prop("signpost"), new Vector3(-5f, -1.1f, 0f), 1.2f, 0, 0);
            tower.color = new Color(0.8f, 0.7f, 0.6f);
            GameObject windmill = EditorUtil.Empty("Windmill", null, new Vector3(-5f, 0.8f, 0f));
            for (int index = 0; index < 4; index++)
            {
                SpriteRenderer blade = EditorUtil.SpriteObject("Windmill_Blade_0" + (index + 1), windmill.transform, Prop("crate"),
                    Quaternion.Euler(0f, 0f, index * 90f) * new Vector3(0f, 1.1f, 0f), 1f, 1, 0);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, index * 90f);
                blade.transform.localScale = new Vector3(0.25f, 0.8f, 1f);
            }
            Rotator rotator = windmill.AddComponent<Rotator>();
            rotator.rotateSpeed = 30f;
            rotator.axis = Vector3.forward;

            GameObject monster = EditorUtil.Empty("Monster_Target", null, new Vector3(4.5f, 1f, 0f));
            SpriteRenderer body = monster.AddComponent<SpriteRenderer>();
            body.sprite = EditorUtil.LoadSprite("Assets/Resources/Icons/Monster/mon_100002.png");
            body.sortingOrder = 2;
            monster.transform.localScale = Vector3.one * 1.2f;

            Canvas canvas = EditorUtil.ScreenCanvas("Canvas", null, 0, 1f);
            TMP_Text hpText = EditorUtil.Text("Txt_MonsterHp", canvas.transform, "HP 100 / 100", 44f, TextAlignmentOptions.Center);
            EditorUtil.Place(hpText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-180f, -60f), new Vector2(600f, 80f));
            hpText.color = new Color(0.1f, 0.1f, 0.12f);
            PracticeHealth health = monster.AddComponent<PracticeHealth>();
            EditorUtil.SetField(health, "hpText", hpText);
            EditorUtil.SetField(health, "body", body);

            Button damage = EditorUtil.Button("Btn_Damage", "Txt_Damage", canvas.transform, "체력 감소", new Vector2(320f, 110f));
            EditorUtil.Place((RectTransform)damage.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-380f, 60f), new Vector2(320f, 110f));
            EditorUtil.AddClick(damage, health.TakeDamage);

            Button sound = EditorUtil.Button("Btn_Sound", "Txt_Sound", canvas.transform, "효과음", new Vector2(320f, 110f));
            EditorUtil.Place((RectTransform)sound.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(320f, 110f));
            SoundButton soundButton = sound.gameObject.AddComponent<SoundButton>();
            EditorUtil.AddClick(sound, soundButton.Play);
            GameObject speaker = new GameObject("Speaker");
            speaker.AddComponent<AudioSource>().playOnAwake = false;

            Button result = EditorUtil.Button("Btn_Result", "Txt_Result", canvas.transform, "결과 창", new Vector2(320f, 110f));
            EditorUtil.Place((RectTransform)result.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(380f, 60f), new Vector2(320f, 110f));
            ResultButton resultButton = result.gameObject.AddComponent<ResultButton>();
            Image panel = EditorUtil.Panel("Panel_Result", canvas.transform, new Color(0.15f, 0.17f, 0.24f, 0.95f), null);
            EditorUtil.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(700f, 300f));
            TMP_Text panelText = EditorUtil.Text("Txt_ResultMessage", panel.transform, "결과 창이 열렸습니다", 44f, TextAlignmentOptions.Center);
            EditorUtil.Stretch(panelText.rectTransform, Vector2.zero, Vector2.zero);
            panel.gameObject.SetActive(false);
            EditorUtil.SetField(resultButton, "resultPanel", panel.gameObject);
            EditorUtil.AddClick(result, resultButton.Show);
            result.enabled = false;

            EditorUtil.EventSystemObject();
            Save(scene, "P3_Components");
        }

        // P4_Prefab

        private static void BuildP4()
        {
            Scene scene = NewScene();
            MainCamera(new Vector3(0f, 11f, -14f), new Vector3(38f, 0f, 0f), false, 5f);
            AddLight();
            EditorUtil.Primitive(PrimitiveType.Cube, "Ground", null, new Vector3(0f, -0.5f, 2f), new Vector3(26f, 1f, 16f), Mat("Practice_Ground"), 0);

            GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Tree_Practice.prefab");
            Transform trees = EditorUtil.Empty("Trees", null, Vector3.zero).transform;
            for (int index = 0; index < 8; index++)
            {
                GameObject tree = EditorUtil.Instance(treePrefab, "Tree_0" + (index + 1), trees);
                tree.transform.localPosition = new Vector3(-10.5f + index * 3f, 0f, 7f);
                if (index == 2 || index == 5)
                {
                    tree.transform.localScale = Vector3.one * 1.4f;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(tree.transform);
                }
            }

            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Monster_Practice.prefab");
            GameObject icePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Monster_Practice_Ice.prefab");
            Transform monsters = EditorUtil.Empty("Monsters", null, Vector3.zero).transform;

            GameObject monsterA = EditorUtil.Instance(basePrefab, "Monster_A", monsters);
            monsterA.transform.localPosition = new Vector3(-8f, 0f, 0f);
            GameObject monsterB = EditorUtil.Instance(basePrefab, "Monster_B", monsters);
            monsterB.transform.localPosition = new Vector3(-4f, 0f, 0f);
            GameObject monsterC = EditorUtil.Instance(basePrefab, "Monster_C", monsters);
            monsterC.transform.localPosition = new Vector3(0f, 0f, 0f);
            monsterC.GetComponent<PracticeMonsterStats>().attackRange = 1.5f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(monsterC.GetComponent<PracticeMonsterStats>());
            GameObject monsterD = EditorUtil.Instance(basePrefab, "Monster_D", monsters);
            monsterD.transform.localPosition = new Vector3(4f, 0f, 0f);
            PrefabUtility.UnpackPrefabInstance(monsterD, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            GameObject monsterE = EditorUtil.Instance(icePrefab, "Monster_E", monsters);
            monsterE.transform.localPosition = new Vector3(8f, 0f, 0f);

            Save(scene, "P4_Prefab");
        }

        // P5_Display

        private static void BuildP5()
        {
            Scene scene = NewScene();
            Camera camera = MainCamera(new Vector3(0f, 2.5f, -10f), new Vector3(8f, 0f, 0f), false, 5f);
            int hidden = LayerMask.NameToLayer("Hidden");
            camera.cullingMask = ~(1 << hidden);
            AddLight();

            GameObject minimapObject = new GameObject("Camera_Minimap");
            Camera minimap = minimapObject.AddComponent<Camera>();
            minimap.orthographic = true;
            minimap.orthographicSize = 7f;
            minimap.depth = 1f;
            minimap.rect = new Rect(0.39f, 0.72f, 0.22f, 0.26f);
            minimap.clearFlags = CameraClearFlags.SolidColor;
            minimap.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            minimapObject.transform.position = new Vector3(0f, 20f, 2f);
            minimapObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            EditorUtil.Primitive(PrimitiveType.Cube, "Ground", null, new Vector3(0f, -0.5f, 2f), new Vector3(20f, 1f, 12f), Mat("Practice_Ground"), 0);

            Transform sprites = EditorUtil.Empty("Sprites2D", null, Vector3.zero).transform;
            SpriteRenderer back = EditorUtil.SpriteObject("Sprite_Background", sprites, EditorUtil.LoadSprite("Assets/Resources/UI/ui_bg_dex.png"),
                new Vector3(-5f, 2.2f, 1f), 0.22f, 0, 0);
            back.sortingOrder = 0;
            SpriteRenderer character = EditorUtil.SpriteObject("Sprite_Character", sprites, EditorUtil.LoadSprite("Assets/Resources/Icons/Monster/mon_100007.png"),
                new Vector3(-5.6f, 1.8f, 1f), 0.6f, -1, 0);
            character.sortingOrder = -1;
            EditorUtil.SpriteObject("Sprite_Tree", sprites, Prop("signpost"), new Vector3(-3.6f, 1.6f, 1f), 0.6f, 2, 0);
            EditorUtil.SpriteObject("Sprite_Front", sprites, Prop("crate"), new Vector3(-4.6f, 0.9f, 1f), 0.4f, 3, 0);

            EditorUtil.Primitive(PrimitiveType.Cube, "Cube_Visible", null, new Vector3(0.5f, 0.6f, 2f), Vector3.one * 1.2f, Mat("Practice_Blue"), 0);
            EditorUtil.Primitive(PrimitiveType.Cube, "Cube_Hidden", null, new Vector3(2.5f, 0.6f, 2f), Vector3.one * 1.2f, Mat("Practice_Purple"), hidden);
            EditorUtil.Primitive(PrimitiveType.Sphere, "Sphere_Pink", null, new Vector3(4.5f, 0.7f, 2f), Vector3.one * 1.4f, Mat("Practice_OldShader"), 0);

            GameObject crate = EditorUtil.Primitive(PrimitiveType.Cube, "Crate_SmallCollider", null, new Vector3(6.5f, 0.75f, 2f), Vector3.one * 1.5f, Mat("Practice_Wood"), 0);
            crate.GetComponent<BoxCollider>().size = new Vector3(0.4f, 0.4f, 0.4f);
            GameObject ball = EditorUtil.Primitive(PrimitiveType.Sphere, "Ball_Drop", null, new Vector3(6.5f, 4.5f, 2f), Vector3.one * 0.6f, Mat("Practice_Red"), 0);
            ball.AddComponent<Rigidbody>();

            Canvas canvas = EditorUtil.ScreenCanvas("Canvas", null, 0, 0f);
            P5Button("Btn_Start", canvas.transform, "시작", new Vector2(0.5f, 0f), new Vector2(0f, 40f));
            P5Button("Btn_Option", canvas.transform, "옵션", new Vector2(1f, 1f), new Vector2(-40f, -40f));
            P5Button("Btn_Shop", canvas.transform, "상점", new Vector2(0f, 1f), new Vector2(40f, -40f));
            P5Button("Btn_Bag", canvas.transform, "가방", new Vector2(0.5f, 0.5f), new Vector2(560f, -530f));

            Save(scene, "P5_Display");
        }

        private static void P5Button(string name, Transform parent, string label, Vector2 anchor, Vector2 position)
        {
            Button button = EditorUtil.Button(name, "Txt_" + name.Substring(4), parent, label, new Vector2(220f, 110f));
            RectTransform rect = (RectTransform)button.transform;
            Vector2 pivot = new Vector2(anchor.x, anchor.y);
            if (name == "Btn_Bag")
            {
                pivot = new Vector2(0.5f, 0f);
            }
            EditorUtil.Place(rect, anchor, pivot, position, new Vector2(220f, 110f));
            ClickCounter counter = button.gameObject.AddComponent<ClickCounter>();
            EditorUtil.SetField(counter, "label", button.GetComponentInChildren<TMP_Text>());
            EditorUtil.SetString(counter, "caption", label);
            EditorUtil.AddClick(button, counter.Count);
        }

        // P6_Data

        private static void BuildP6()
        {
            Scene scene = NewScene();
            MainCamera(new Vector3(0f, 0f, -10f), Vector3.zero, true, 5.4f);

            SpriteRenderer spinner = EditorUtil.SpriteObject("Spinner", null, Prop("crate"), new Vector3(6f, 2.5f, 0f), 0.6f, 0, 0);

            Canvas canvas = EditorUtil.ScreenCanvas("Canvas", null, 0, 1f);
            TMP_Text configText = EditorUtil.Text("Txt_Config", canvas.transform, "", 40f, TextAlignmentOptions.TopLeft);
            EditorUtil.Place(configText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(900f, 120f));
            configText.color = new Color(0.1f, 0.1f, 0.12f);
            GameObject configObject = new GameObject("ConfigView");
            ConfigView configView = configObject.AddComponent<ConfigView>();
            EditorUtil.SetField(configView, "config", ProjectSetup.GameConfigAsset);
            EditorUtil.SetField(configView, "valueText", configText);
            EditorUtil.SetField(configView, "spinner", spinner.transform);

            Image shopPanel = EditorUtil.Panel("Panel_ShopList", canvas.transform, new Color(0.18f, 0.20f, 0.28f, 0.95f), null);
            EditorUtil.Place(shopPanel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, -60f), new Vector2(760f, 720f));
            TMP_Text shopTitle = EditorUtil.Text("Txt_ShopListTitle", shopPanel.transform, "상점 목록 (shop.csv)", 40f, TextAlignmentOptions.Center);
            EditorUtil.Place(shopTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(700f, 60f));
            RectTransform list = EditorUtil.Rect("List_ShopRows", shopPanel.transform);
            EditorUtil.Stretch(list, new Vector2(30f, 20f), new Vector2(-30f, -90f));
            VerticalLayoutGroup layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            TMP_Text rowTemplate = EditorUtil.Text("Txt_ShopRowTemplate", shopPanel.transform, "", 34f, TextAlignmentOptions.Left);
            rowTemplate.rectTransform.sizeDelta = new Vector2(700f, 56f);
            rowTemplate.gameObject.SetActive(false);
            ShopListView shopView = shopPanel.gameObject.AddComponent<ShopListView>();
            EditorUtil.SetField(shopView, "listRoot", list);
            EditorUtil.SetField(shopView, "rowPrefab", rowTemplate);

            GameObject removed = new GameObject("Temp_Marker");
            GameObject errorObject = new GameObject("ErrorButtons");
            ErrorButtons errors = errorObject.AddComponent<ErrorButtons>();
            EditorUtil.SetField(errors, "removedOnStart", removed);
            EditorUtil.SetString(errors, "sceneName", "Shop_Old");

            Button nullButton = EditorUtil.Button("Btn_NullReference", "Txt_NullReference", canvas.transform, "에러 1 · 빈 칸", new Vector2(420f, 110f));
            EditorUtil.Place((RectTransform)nullButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 300f), new Vector2(420f, 110f));
            EditorUtil.AddClick(nullButton, errors.RaiseNullReference);
            Button missingButton = EditorUtil.Button("Btn_MissingReference", "Txt_MissingReference", canvas.transform, "에러 2 · 삭제된 대상", new Vector2(420f, 110f));
            EditorUtil.Place((RectTransform)missingButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 170f), new Vector2(420f, 110f));
            EditorUtil.AddClick(missingButton, errors.RaiseMissingReference);
            Button sceneButton = EditorUtil.Button("Btn_LoadScene", "Txt_LoadScene", canvas.transform, "에러 3 · 씬 불러오기", new Vector2(420f, 110f));
            EditorUtil.Place((RectTransform)sceneButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(420f, 110f));
            EditorUtil.AddClick(sceneButton, errors.LoadMissingScene);

            EditorUtil.EventSystemObject();
            Save(scene, "P6_Data");
        }
    }
}
