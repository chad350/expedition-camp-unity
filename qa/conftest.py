"""pytest 공통 Fixture 입니다."""

import os
import socket
import subprocess
import time

import pytest

from utils import config_reader
from utils.csv_reader import TableReadError, read_table
from utils.game_client import GameClient

TABLE_NAMES = ["monster", "skill", "item", "drop_group", "stage", "stage_wave",
               "level_exp", "const", "enum", "string", "shop", "field_object"]


@pytest.fixture(scope="session")
def config():
    return config_reader.load_config()


class LoadedTables:
    def __init__(self):
        self.rows = {}
        self.errors = []

    def __getitem__(self, name):
        return self.rows[name]


@pytest.fixture(scope="session")
def tables():
    """모든 CSV 를 읽습니다. 읽지 못한 표는 빈 목록으로 두고 errors 에 이유를 남깁니다."""
    loaded = LoadedTables()
    for name in TABLE_NAMES:
        try:
            loaded.rows[name] = read_table(name)
        except TableReadError as error:
            loaded.rows[name] = []
            loaded.errors.append(str(error))
    return loaded


@pytest.fixture(scope="session")
def unity_project():
    return config_reader.project_path("unity_project")


def port_in_use(port):
    connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    connection.settimeout(1)
    in_use = connection.connect_ex(("127.0.0.1", port)) == 0
    connection.close()
    return in_use


@pytest.fixture(scope="session")
def game(config):
    """테스트용 빌드를 실행하고 연결된 GameClient 를 반환합니다."""
    executable = config_reader.game_executable("builds_test")
    assert os.path.exists(executable), "테스트용 빌드가 없습니다: %s" % executable

    save_file = config_reader.os_value("save_file")
    if os.path.exists(save_file):
        os.remove(save_file)

    port = config["game"]["port"]
    if port_in_use(port):
        pytest.fail("%d 포트를 이미 다른 프로그램이 사용하고 있습니다. 실행 중인 게임을 모두 종료한 뒤 다시 실행해 주세요." % port)
    command = [executable, "-qaSeed", str(config["game"]["seed"]), "-qaPort", str(port),
               "-screen-width", "1280", "-screen-height", "720", "-screen-fullscreen", "0"]
    process = subprocess.Popen(command)
    client = GameClient(port)
    connected = client.wait_for_health(config["game"]["startup_timeout"])
    if not connected:
        process.terminate()
        pytest.fail("게임이 %d초 안에 QA 연결에 응답하지 않았습니다" % config["game"]["startup_timeout"])

    yield client

    process.terminate()
    try:
        process.wait(timeout=20)
    except subprocess.TimeoutExpired:
        process.kill()
    time.sleep(1)


@pytest.hookimpl(hookwrapper=True)
def pytest_runtest_makereport(item, call):
    outcome = yield
    report = outcome.get_result()
    if report.when != "call" or not report.failed:
        return
    if "game" not in item.fixturenames:
        return
    client = item.funcargs["game"]
    path = os.path.join(config_reader.reports_dir("screenshots"), "FAILED_" + item.name + ".png")
    client.screenshot(path)
