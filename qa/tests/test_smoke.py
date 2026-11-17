"""스모크 테스트입니다. 테스트용 빌드를 실행하고 기본 흐름을 확인합니다.
Jenkins 5단계에서 실행합니다. 테스트는 위에서부터 순서대로 실행됩니다.
"""

import os
import shutil

from screens.bag_popup import BagPopup
from screens.field_screen import FieldScreen
from screens.lobby_screen import BUTTONS, LobbyScreen
from screens.result_popup import ResultPopup
from utils import config_reader
from utils.config_reader import load_rules

RULES = load_rules("smoke.yaml")
shared = {}


def save_screenshot(game, name):
    path = os.path.join(config_reader.reports_dir("screenshots"), name + ".png")
    game.screenshot(path)


def first_wave_count(tables):
    total = 0
    for row in tables["field_object"]:
        if int(row["stage_id"]) == RULES["first_stage"] and int(row["wave_no"]) == 1:
            total += int(row["count"])
    return total


def test_01_game_starts_in_lobby(game):
    lobby = game.wait_for_scene("Lobby", 20)
    save_screenshot(game, "smoke_01_lobby")
    assert lobby is not None, "20초 안에 Lobby 로 넘어가지 않았습니다 · 현재 씬 %s" % game.scene()["scene"]


def test_02_lobby_buttons_are_clickable(game):
    lobby = LobbyScreen(game)
    states = lobby.button_states()
    problems = []
    for name in BUTTONS:
        state = states[name]
        if state is None:
            problems.append("%s 를 찾을 수 없습니다" % name)
        elif not state["active"] or not state["interactable"]:
            problems.append("%s active %s · interactable %s" % (name, state["active"], state["interactable"]))
    assert problems == [], "\n".join(problems)


def test_03_enter_first_stage(game):
    lobby = LobbyScreen(game)
    opened = lobby.open_stage_select()
    save_screenshot(game, "smoke_03_stage_select")
    assert opened, "출정 게시판을 클릭해도 스테이지 선택 창이 열리지 않았습니다"
    scene = lobby.start_stage(RULES["first_stage"], RULES["first_scene"])
    save_screenshot(game, "smoke_03_field")
    assert scene is not None, "%s 씬으로 이동하지 않았습니다 · 현재 %s" % (RULES["first_scene"], game.scene())


def test_04_first_wave_monster_count(game, tables):
    field = FieldScreen(game)
    expected = first_wave_count(tables)
    count = field.wait_for_monsters(expected, 10)
    save_screenshot(game, "smoke_04_wave1")
    assert count == expected, "1웨이브 몬스터가 %s마리입니다 (field_object 기준 %d마리)" % (count, expected)


def test_05_monsters_approach_and_player_is_safe(game):
    field = FieldScreen(game)
    before, after, moves = field.monster_moves(RULES["approach_seconds"])
    save_screenshot(game, "smoke_05_approach")
    problems = []
    for name, distance, speed in moves:
        if distance < RULES["min_move"]:
            problems.append("%s 가 %d초 동안 %.2f 만큼만 움직였습니다 (Move Speed %s)" % (name, RULES["approach_seconds"], distance, speed))
    if after["player_hp"] < after["player_max_hp"]:
        problems.append("필드 진입 %d초 만에 플레이어 체력이 %d / %d 입니다" % (RULES["approach_seconds"], after["player_hp"], after["player_max_hp"]))
    assert problems == [], "\n".join(problems)


def test_06_clear_stage_and_result(game):
    field = FieldScreen(game)
    cleared = field.clear_with_cheat(60)
    save_screenshot(game, "smoke_06_result")
    assert cleared, "몬스터를 모두 처치해도 결과 창이 나오지 않았습니다 · 필드 상태 %s" % game.field().text[:300]
    result = ResultPopup(game)
    items = result.items()
    shared["result_items"] = items
    expected = {}
    for item_id in RULES["expected_items"]:
        expected[int(item_id)] = RULES["expected_items"][item_id]
    assert items == expected, "결과 창 아이템 %s · 기대 %s (Seed %d)" % (items, expected, 1234)
    gold = result.gold_text()
    assert gold == "골드 +%d" % RULES["expected_gold"], "결과 창 골드 %s · 기대 %d" % (gold, RULES["expected_gold"])


def test_07_bag_matches_result(game):
    result = ResultPopup(game)
    lobby_scene = result.confirm()
    assert lobby_scene is not None, "결과 창 확인 후 로비로 돌아가지 않았습니다"
    lobby = LobbyScreen(game)
    opened = lobby.open_bag()
    save_screenshot(game, "smoke_07_bag")
    assert opened, "가방 버튼을 눌러도 가방 창이 열리지 않았습니다"
    bag_items = BagPopup(game).items()
    assert bag_items == shared.get("result_items"), "가방 %s · 결과 창 %s" % (bag_items, shared.get("result_items"))


def test_08_player_log_has_no_exception(game):
    log_path = config_reader.os_value("player_log")
    assert os.path.exists(log_path), "Player.log 가 없습니다: %s" % log_path
    shutil.copyfile(log_path, os.path.join(config_reader.reports_dir(), "Player.log"))
    with open(log_path, encoding="utf-8", errors="replace") as log_file:
        lines = log_file.read().splitlines()
    exceptions = []
    for line in lines:
        if "Exception" in line:
            exceptions.append(line)
    assert exceptions == [], "Player.log Exception %d건\n%s" % (len(exceptions), "\n".join(exceptions[:5]))
