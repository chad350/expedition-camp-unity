using ExpeditionCamp;
using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class InventoryTests
    {
        private const int MaterialItemId = 201001;
        private const int PotionItemId = 202001;

        [SetUp]
        public void ClearInventory()
        {
            GameData.Load();
            Inventory.Clear();
        }

        [Test]
        public void AddItemKeepsTotalCountWhenSameItemAddedTwice()
        {
            Inventory.AddItem(MaterialItemId, 1);
            Inventory.AddItem(MaterialItemId, 1);

            Assert.AreEqual(2, Inventory.GetCount(MaterialItemId),
                "같은 아이템을 2번 추가하면 개수가 2여야 합니다");
        }

        [Test]
        public void AddItemStopsAtMaxStack()
        {
            int maxStack = Inventory.GetMaxStack(PotionItemId);
            Inventory.AddItem(PotionItemId, maxStack);
            Inventory.AddItem(PotionItemId, 5);

            Assert.AreEqual(maxStack, Inventory.GetCount(PotionItemId),
                "최대 중첩 수를 넘는 개수는 버려야 합니다");
        }

        [Test]
        public void GetCountReturnsZeroForItemNeverAdded()
        {
            Assert.AreEqual(0, Inventory.GetCount(MaterialItemId));
        }

        [Test]
        public void ClearRemovesEveryItem()
        {
            Inventory.AddItem(MaterialItemId, 3);
            Inventory.Clear();

            Assert.AreEqual(0, Inventory.GetItemIds().Count);
        }
    }
}
