"""필드 화면 객체입니다. 필드 몬스터는 UI 가 아니라서 /field · /count · /cheat 를 사용합니다."""

import math
import time

from screens.base_screen import BaseScreen


class FieldScreen(BaseScreen):
    def state(self):
        response = self.client.field()
        assert response.status_code == 200, "필드 상태를 조회하지 못했습니다 · %s" % response.text
        return response.json()

    def wait_for_monsters(self, expected, timeout):
        deadline = time.time() + timeout
        count = None
        while time.time() < deadline:
            count = self.client.count("Monster")
            if count == expected:
                return count
            time.sleep(0.3)
        return count

    def monster_moves(self, seconds):
        """seconds 동안 몬스터마다 움직인 거리를 반환합니다."""
        before = self.state()
        time.sleep(seconds)
        after = self.state()
        positions = {}
        for index, monster in enumerate(before["monsters"]):
            positions[index] = monster
        moves = []
        for index, monster in enumerate(after["monsters"]):
            start = positions.get(index)
            if start is None:
                continue
            distance = math.hypot(monster["x"] - start["x"], monster["z"] - start["z"])
            moves.append((monster["name"], round(distance, 2), monster["move_speed"]))
        return before, after, moves

    def clear_with_cheat(self, timeout):
        """몬스터 전부 처치 치트를 반복해서 결과 창이 나올 때까지 기다립니다."""
        deadline = time.time() + timeout
        while time.time() < deadline:
            self.client.cheat("kill_all")
            if self.is_visible("Popup_Result"):
                return True
            time.sleep(0.5)
        return False
