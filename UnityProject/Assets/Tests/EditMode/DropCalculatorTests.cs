using System.Collections.Generic;
using ExpeditionCamp;
using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class DropCalculatorTests
    {
        private const int LavaGolemDropGroupId = 300014;

        private List<Dictionary<string, string>> _dropRows;

        [SetUp]
        public void LoadDropRows()
        {
            GameData.Load();
            _dropRows = GameData.GetDropRows(LavaGolemDropGroupId);
            Assert.Greater(_dropRows.Count, 0, "드롭 그룹 300014 행을 찾지 못했습니다");
        }

        [TestCase(0, 201014)]
        [TestCase(2499, 201014)]
        [TestCase(2500, 201013)]
        [TestCase(6999, 201013)]
        [TestCase(7000, 201012)]
        [TestCase(9999, 201012)]
        public void SelectRandomOneReturnsItemForRoll(int roll, int expectedItemId)
        {
            int itemId = DropCalculator.SelectRandomOne(_dropRows, roll);

            Assert.AreEqual(expectedItemId, itemId,
                "roll " + roll + " 에서 선택된 아이템이 기대값과 다릅니다");
        }

        [Test]
        public void IsIndependentDroppedGivesItemBelowProbability()
        {
            Assert.IsTrue(DropCalculator.IsIndependentDropped(6000, 5999));
        }

        [Test]
        public void IsIndependentDroppedSkipsItemAtProbability()
        {
            Assert.IsFalse(DropCalculator.IsIndependentDropped(6000, 6000));
        }

        [Test]
        public void CalculateDropsAlwaysGivesFixedItem()
        {
            List<DropResult> results = DropCalculator.CalculateDrops(_dropRows, FixedRandom);

            bool goldFound = false;
            foreach (DropResult result in results)
            {
                if (result.ItemId == 203001)
                {
                    goldFound = true;
                    Assert.GreaterOrEqual(result.Count, 120);
                    Assert.LessOrEqual(result.Count, 160);
                }
            }
            Assert.IsTrue(goldFound, "확정 드롭인 골드가 결과에 없습니다");
        }

        [Test]
        public void CalculateDropsGivesOnlyOneRandomOneItem()
        {
            List<DropResult> results = DropCalculator.CalculateDrops(_dropRows, FixedRandom);

            int materialCount = 0;
            foreach (DropResult result in results)
            {
                if (result.ItemId == 201012 || result.ItemId == 201013 || result.ItemId == 201014)
                {
                    materialCount += 1;
                }
            }
            Assert.AreEqual(1, materialCount, "RANDOM_ONE 그룹에서 아이템이 1개만 지급되어야 합니다");
        }

        private static int FixedRandom(int minInclusive, int maxExclusive)
        {
            return minInclusive;
        }
    }
}
