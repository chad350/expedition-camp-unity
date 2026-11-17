using System.Collections.Generic;
using ExpeditionCamp;
using NUnit.Framework;

namespace ExpeditionCamp.Tests
{
    public class DataLoaderTests
    {
        [Test]
        public void ParseLineReadsPlainFields()
        {
            List<string> values = DataLoader.ParseLine("100001,STR_MON_100001_NAME,FIRE,N");

            Assert.AreEqual(4, values.Count);
            Assert.AreEqual("100001", values[0]);
            Assert.AreEqual("FIRE", values[2]);
        }

        [Test]
        public void ParseLineKeepsCommaInsideQuotes()
        {
            List<string> values = DataLoader.ParseLine("STR_ITEM_202001_DESC,\"체력을 20% 회복합니다. 전투 중, 이동 중 모두 사용할 수 있습니다.\",ok");

            Assert.AreEqual(3, values.Count);
            Assert.AreEqual("체력을 20% 회복합니다. 전투 중, 이동 중 모두 사용할 수 있습니다.", values[1]);
            Assert.AreEqual("ok", values[2]);
        }

        [Test]
        public void ParseLineKeepsEscapedQuote()
        {
            List<string> values = DataLoader.ParseLine("key,\"어민들은 이 고래를 \"\"별을 먹는 자\"\"라고 부릅니다.\",ok");

            Assert.AreEqual(3, values.Count);
            Assert.AreEqual("어민들은 이 고래를 \"별을 먹는 자\"라고 부릅니다.", values[1]);
        }

        [Test]
        public void ParseCsvReturnsEmptyListForEmptyText()
        {
            List<Dictionary<string, string>> rows = DataLoader.ParseCsv("");

            Assert.AreEqual(0, rows.Count);
        }

        [Test]
        public void ParseCsvReadsHeaderAndRows()
        {
            List<Dictionary<string, string>> rows = DataLoader.ParseCsv("key,ko\nSTR_A,가\nSTR_B,나\n");

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("가", rows[0]["ko"]);
            Assert.AreEqual("STR_B", rows[1]["key"]);
        }

        [Test]
        public void LoadTableKeepsCommaInsideQuotedDescription()
        {
            List<Dictionary<string, string>> rows = DataLoader.LoadTable("string");

            Dictionary<string, string> target = null;
            foreach (Dictionary<string, string> row in rows)
            {
                if (DataLoader.ReadText(row, "key") == "STR_ITEM_202001_DESC")
                {
                    target = row;
                    break;
                }
            }

            Assert.IsNotNull(target, "STR_ITEM_202001_DESC 행을 찾지 못했습니다");
            Assert.AreEqual("체력을 20% 회복합니다. 전투 중, 이동 중 모두 사용할 수 있습니다.",
                DataLoader.ReadText(target, "ko"));
            Assert.AreEqual("Restores 20% HP. Usable in battle, or while moving.",
                DataLoader.ReadText(target, "en"));
        }

        [Test]
        public void GetStringReturnsKeyWhenStringIsMissing()
        {
            string value = GameData.GetString("STR_MON_999999_NAME");

            Assert.AreEqual("STR_MON_999999_NAME", value);
        }

        [Test]
        public void GetStringReturnsKoreanTextForExistingKey()
        {
            string value = GameData.GetString("STR_MON_100001_NAME");

            Assert.AreEqual("불꽃여우", value);
        }
    }
}
