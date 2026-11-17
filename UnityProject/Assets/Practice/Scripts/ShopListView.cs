using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ExpeditionCamp.Practice
{
    public class ShopListView : MonoBehaviour
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private TMP_Text rowPrefab;

        private void Start()
        {
            List<Dictionary<string, string>> rows = DataLoader.LoadTable("shop");
            List<Dictionary<string, string>> visible = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in rows)
            {
                if (DataLoader.ReadBool(row, "is_visible"))
                {
                    visible.Add(row);
                }
            }
            visible.Sort((left, right) =>
                DataLoader.ReadInt(left, "sort_order", 0).CompareTo(DataLoader.ReadInt(right, "sort_order", 0)));

            int order = 1;
            foreach (Dictionary<string, string> row in visible)
            {
                int itemId = DataLoader.ReadInt(row, "item_id", 0);
                string itemName = GameData.GetString(DataLoader.ReadText(GameData.GetItem(itemId), "name_key"));
                TMP_Text line = Instantiate(rowPrefab, listRoot);
                line.gameObject.name = "Txt_ShopRow_" + order;
                line.text = order + ". " + itemName + "  " + DataLoader.ReadInt(row, "price", 0) + " 골드";
                line.gameObject.SetActive(true);
                order++;
            }
            Debug.Log("[Practice] shop.csv 에서 " + visible.Count + "행을 읽었습니다");
        }
    }
}
