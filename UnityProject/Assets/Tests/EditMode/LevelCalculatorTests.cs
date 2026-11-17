using System.Collections.Generic;
using ExpeditionCamp;
using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class LevelCalculatorTests
    {
        private List<Dictionary<string, string>> _levelRows;

        [SetUp]
        public void LoadLevelExp()
        {
            GameData.Load();
            _levelRows = GameData.LevelExp;
        }

        [Test]
        public void GetLevelReturnsOneAtZeroExp()
        {
            Assert.AreEqual(1, LevelCalculator.GetLevel(0, _levelRows));
        }

        [Test]
        public void GetLevelReturnsSecondLevelAtExactRequiredExp()
        {
            Assert.AreEqual(2, LevelCalculator.GetLevel(120, _levelRows));
        }

        [Test]
        public void GetLevelKeepsFirstLevelBelowRequiredExp()
        {
            Assert.AreEqual(1, LevelCalculator.GetLevel(119, _levelRows));
        }

        [Test]
        public void GetLevelReturnsThirtySevenAtRequiredExp()
        {
            Assert.AreEqual(37, LevelCalculator.GetLevel(52100, _levelRows));
        }

        [Test]
        public void GetLevelStopsAtMaxPlayerLevel()
        {
            int maxLevel = GameData.GetConstInt("MAX_PLAYER_LEVEL", 60);

            Assert.AreEqual(maxLevel, LevelCalculator.GetLevel(99999999, _levelRows));
        }
    }
}
