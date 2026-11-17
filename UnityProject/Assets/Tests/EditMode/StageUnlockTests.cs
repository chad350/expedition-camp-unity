using System.Collections.Generic;
using ExpeditionCamp;
using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class StageUnlockTests
    {
        private List<Dictionary<string, string>> _stageRows;

        [SetUp]
        public void LoadStages()
        {
            GameData.Load();
            _stageRows = GameData.Stages;
        }

        [Test]
        public void IsUnlockedOpensFirstStageOfFirstChapter()
        {
            HashSet<int> cleared = new HashSet<int>();

            Assert.IsTrue(StageUnlock.IsUnlocked(401001, cleared, _stageRows),
                "1챕터 1스테이지는 처음부터 해금되어야 합니다");
        }

        [Test]
        public void IsUnlockedBlocksStageWhenPreviousStageIsNotCleared()
        {
            HashSet<int> cleared = new HashSet<int>();

            Assert.IsFalse(StageUnlock.IsUnlocked(401002, cleared, _stageRows));
        }

        [Test]
        public void IsUnlockedOpensStageWhenPreviousStageIsCleared()
        {
            HashSet<int> cleared = new HashSet<int>();
            cleared.Add(401001);

            Assert.IsTrue(StageUnlock.IsUnlocked(401002, cleared, _stageRows));
        }

        [Test]
        public void IsUnlockedOpensNextChapterWhenLastStageIsCleared()
        {
            HashSet<int> cleared = new HashSet<int>();
            cleared.Add(401010);

            Assert.IsTrue(StageUnlock.IsUnlocked(402001, cleared, _stageRows));
        }

        [Test]
        public void IsUnlockedReturnsFalseForUnknownStage()
        {
            HashSet<int> cleared = new HashSet<int>();

            Assert.IsFalse(StageUnlock.IsUnlocked(409999, cleared, _stageRows));
        }
    }
}
