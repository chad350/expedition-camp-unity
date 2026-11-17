using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ExpeditionCamp.Tests
{
    public class GameFlowTests
    {
        private const float SceneTimeoutSecond = 15f;
        private const float StageTimeoutSecond = 60f;

        [SetUp]
        public void ResetState()
        {
            if (File.Exists(SaveSystem.SavePath))
            {
                File.Delete(SaveSystem.SavePath);
            }
            SaveSystem.ClearForTest();
            GameSession.ResetSession();
            DropLog.Clear();
            GameData.Reload();
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        private static IEnumerator WaitForScene(string sceneName)
        {
            float elapsed = 0f;
            while (SceneManager.GetActiveScene().name != sceneName && elapsed < SceneTimeoutSecond)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name, sceneName + " 씬으로 이동하지 않았습니다");
        }

        private static IEnumerator PlayStageWithKillAll(int stageId, List<int> wavesSeen)
        {
            GameSession.CurrentStageId = stageId;
            SceneManager.LoadScene(GameSession.GetSceneName(stageId));
            yield return WaitForScene(GameSession.GetSceneName(stageId));

            float elapsed = 0f;
            while (FieldManager.Current == null && elapsed < SceneTimeoutSecond)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            FieldManager field = FieldManager.Current;
            Assert.IsNotNull(field, "FieldManager 를 찾지 못했습니다");

            elapsed = 0f;
            while (!field.IsFinished && elapsed < StageTimeoutSecond)
            {
                if (field.CurrentWave > 0 && !wavesSeen.Contains(field.CurrentWave))
                {
                    wavesSeen.Add(field.CurrentWave);
                }
                field.KillAll();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.IsTrue(field.IsFinished, "스테이지가 끝나지 않았습니다 · 현재 웨이브 " + field.CurrentWave);
        }

        [UnityTest]
        public IEnumerator BootMovesToLobby()
        {
            SceneManager.LoadScene("Boot");
            yield return WaitForScene("Lobby");

            Assert.IsNull(GameData.LoadError);
            Assert.IsTrue(File.Exists(SaveSystem.SavePath), "로비 진입 때 세이브 파일을 만들지 않았습니다");
        }

        [UnityTest]
        public IEnumerator FirstStageRunsThreeWavesAndShowsResult()
        {
            List<int> wavesSeen = new List<int>();
            yield return PlayStageWithKillAll(401001, wavesSeen);

            FieldManager field = FieldManager.Current;
            Assert.AreEqual(3, field.TotalWaves);
            CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, wavesSeen);
            Assert.IsTrue(field.IsCleared);
            GameObject popup = GameObject.Find("Popup_Result");
            Assert.IsNotNull(popup, "결과 창이 표시되지 않았습니다");
            Assert.IsTrue(popup.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator ResultItemCountsMatchBagAfterConfirm()
        {
            List<int> wavesSeen = new List<int>();
            yield return PlayStageWithKillAll(401001, wavesSeen);

            FieldManager field = FieldManager.Current;
            FieldLoot loot = field.Loot;
            Assert.Greater(loot.ItemOrder.Count + loot.Gold, 0, "획득한 보상이 없습니다");
            Dictionary<int, int> expected = new Dictionary<int, int>();
            foreach (int itemId in loot.ItemOrder)
            {
                expected[itemId] = loot.GetCount(itemId);
            }
            int expectedGold = loot.Gold;

            field.ConfirmResult();
            yield return WaitForScene("Lobby");

            foreach (KeyValuePair<int, int> pair in expected)
            {
                Assert.AreEqual(pair.Value, Inventory.GetCount(pair.Key), "아이템 " + pair.Key + " 개수가 결과 창과 다릅니다");
            }
            Assert.AreEqual(expected.Count, Inventory.GetItemIds().Count);
            Assert.AreEqual(expectedGold, SaveSystem.Data.gold);
            Assert.IsTrue(SaveSystem.Data.cleared_stages.Contains(401001));
            Assert.IsTrue(SaveSystem.Data.unlocked_stages.Contains(401002));
        }

        [UnityTest]
        public IEnumerator DoubleConfirmGivesRewardOnce()
        {
            List<int> wavesSeen = new List<int>();
            yield return PlayStageWithKillAll(401001, wavesSeen);

            FieldLoot loot = FieldManager.Current.Loot;
            Dictionary<int, int> expected = new Dictionary<int, int>();
            foreach (int itemId in loot.ItemOrder)
            {
                expected[itemId] = loot.GetCount(itemId);
            }
            int expectedGold = loot.Gold;
            int expectedExp = loot.Exp;

            Button confirm = GameObject.Find("Btn_ResultConfirm").GetComponent<Button>();
            confirm.onClick.Invoke();
            confirm.onClick.Invoke();
            yield return WaitForScene("Lobby");

            Assert.AreEqual(expectedGold, SaveSystem.Data.gold, "골드가 두 번 들어왔습니다");
            foreach (KeyValuePair<int, int> pair in expected)
            {
                Assert.AreEqual(pair.Value, Inventory.GetCount(pair.Key), "아이템 " + pair.Key + " 개수가 결과 창과 다릅니다");
            }
            Assert.AreEqual(expectedExp, SaveSystem.Data.exp, "경험치가 두 번 들어왔습니다");
        }

        [UnityTest]
        public IEnumerator AutoBattleClearsFirstStageWithoutCheats()
        {
            GameSession.Invincible = true;
            GameSession.SetSpeedMultiplier(5f);
            GameSession.CurrentStageId = 401001;
            SceneManager.LoadScene("Field_01");
            yield return WaitForScene("Field_01");

            float elapsed = 0f;
            while ((FieldManager.Current == null || !FieldManager.Current.IsFinished) && elapsed < 90f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.IsNotNull(FieldManager.Current);
            Debug.Log("[Test] 자동 전투 1스테이지 " + elapsed.ToString("0.0") + "초 · 웨이브 " + FieldManager.Current.CurrentWave);
            Assert.IsTrue(FieldManager.Current.IsFinished, "자동 전투로 스테이지가 끝나지 않았습니다 · 웨이브 " + FieldManager.Current.CurrentWave);
            Assert.IsTrue(FieldManager.Current.IsCleared);
        }

        [UnityTest]
        public IEnumerator SameSeedGivesSameDrops()
        {
            List<string> first = new List<string>();
            List<string> second = new List<string>();

            GameRandom.SetSeed(1234);
            DropLog.Clear();
            yield return PlayStageWithKillAll(401001, new List<int>());
            foreach (DropRecord record in DropLog.History)
            {
                first.Add(Describe(record));
            }

            GameRandom.SetSeed(1234);
            DropLog.Clear();
            yield return PlayStageWithKillAll(401001, new List<int>());
            foreach (DropRecord record in DropLog.History)
            {
                second.Add(Describe(record));
            }

            Debug.Log("[Test] seed 1234 drops: " + string.Join(" | ", first.ToArray()));
            Assert.Greater(first.Count, 0);
            CollectionAssert.AreEqual(first, second);
        }

        private static string Describe(DropRecord record)
        {
            List<string> parts = new List<string>();
            for (int index = 0; index < record.item_ids.Length; index++)
            {
                parts.Add(record.item_ids[index] + "x" + record.counts[index]);
            }
            return record.group_id + ":" + string.Join(",", record.rolls) + ":" + string.Join(",", parts.ToArray());
        }
    }
}
