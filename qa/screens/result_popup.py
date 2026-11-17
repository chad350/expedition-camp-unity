"""결과 창 화면 객체입니다."""

from screens.base_screen import BaseScreen


class ResultPopup(BaseScreen):
    def items(self):
        """결과 창의 아이템 {item_id: 개수} 를 반환합니다."""
        response = self.client.get("/hierarchy")
        objects = response.json()["objects"]
        items = {}
        for entry in objects:
            name = entry["name"]
            if not name.startswith("ResultCount_"):
                continue
            item_id = int(name.split("_")[1])
            items[item_id] = int(entry["text"].lstrip("x"))
        return items

    def gold_text(self):
        return self.client.get_text("Txt_ResultGold")

    def confirm(self):
        self.click("Btn_ResultConfirm")
        return self.client.wait_for_scene("Lobby", 20)
