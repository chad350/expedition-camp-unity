"""가방 팝업 화면 객체입니다."""

from screens.base_screen import BaseScreen


class BagPopup(BaseScreen):
    def items(self):
        response = self.client.get("/hierarchy")
        objects = response.json()["objects"]
        items = {}
        for entry in objects:
            name = entry["name"]
            if not name.startswith("BagCount_"):
                continue
            item_id = int(name.split("_")[1])
            items[item_id] = int(entry["text"].lstrip("x"))
        return items
