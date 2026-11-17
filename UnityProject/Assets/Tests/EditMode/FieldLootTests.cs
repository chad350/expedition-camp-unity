using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class FieldLootTests
    {
        [Test]
        public void GoldIsCountedSeparatelyFromItems()
        {
            GameData.Load();
            FieldLoot loot = new FieldLoot();

            loot.AddItem(203001, 50);
            loot.AddItem(201002, 1);
            loot.AddItem(201002, 2);

            Assert.AreEqual(50, loot.Gold);
            Assert.AreEqual(1, loot.ItemOrder.Count);
            Assert.AreEqual(3, loot.GetCount(201002));
        }
    }
}
