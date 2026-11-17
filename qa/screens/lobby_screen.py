"""로비 화면 객체입니다."""

from screens.base_screen import BaseScreen

BUTTONS = ["Btn_Bag", "Btn_Dex", "Btn_Shop", "Btn_Settings"]


class LobbyScreen(BaseScreen):
    def button_states(self):
        states = {}
        for name in BUTTONS:
            response = self.client.get_object(name)
            if response.status_code != 200:
                states[name] = None
                continue
            states[name] = response.json()
        return states

    def open_stage_select(self):
        self.click("NPC_Board")
        found = self.client.wait_for_object("Popup_StageSelect", 5)
        return found is not None

    def start_stage(self, stage_id, scene_name):
        self.click("Btn_Stage_%d" % stage_id)
        return self.client.wait_for_scene(scene_name, 20)

    def open_bag(self):
        self.click("Btn_Bag")
        found = self.client.wait_for_object("Popup_Bag", 5)
        return found is not None
