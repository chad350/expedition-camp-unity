using System.Collections.Generic;
using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class DataSchemaTests
    {
        [Test]
        public void ValidateReportsRowAndColumnForNonIntegerValue()
        {
            string text = "shop_slot_id,item_id,price,currency_item_id,sort_order,is_visible\n601001,202001,1OO,203001,10,TRUE\n";
            List<Dictionary<string, string>> rows = DataLoader.ParseCsv(text);

            DataLoadError error = DataSchema.Validate("shop", text, rows);

            Assert.IsNotNull(error);
            Assert.AreEqual("shop", error.Sheet);
            Assert.AreEqual(2, error.Row);
            Assert.AreEqual("price", error.Column);
        }

        [Test]
        public void ValidateReportsMissingColumn()
        {
            string text = "stage_id,spawn_point_key,wave_no,monster_id\n401001,Spawn_01,1,100002\n";
            List<Dictionary<string, string>> rows = DataLoader.ParseCsv(text);

            DataLoadError error = DataSchema.Validate("field_object", text, rows);

            Assert.IsNotNull(error);
            Assert.AreEqual("count", error.Column);
        }

        [Test]
        public void ExportedTablesPassValidation()
        {
            GameData.Reload();

            Assert.IsNull(GameData.LoadError, GameData.LoadError == null ? "" : GameData.LoadError.ToMessage());
        }

        [Test]
        public void ShopHasEightVisibleRowsAndPotionCostsHundred()
        {
            GameData.Reload();
            List<Dictionary<string, string>> rows = GameData.GetVisibleShopRows();

            Assert.AreEqual(8, rows.Count);
            Assert.AreEqual(202001, DataLoader.ReadInt(rows[0], "item_id", 0));
            Assert.AreEqual(100, DataLoader.ReadInt(rows[0], "price", 0));
        }

        [Test]
        public void OnlyFirstTenStagesArePlayable()
        {
            GameData.Reload();

            Assert.IsTrue(GameData.IsPlayableStage(401001));
            Assert.IsTrue(GameData.IsPlayableStage(401010));
            Assert.IsFalse(GameData.IsPlayableStage(402001));
        }
    }
}
