"""빌드 산출물 검사입니다. Jenkins 4단계에서 빌드 2종을 만든 뒤 실행합니다."""

import json
import os
import socket
import subprocess
import time

import pytest

from utils import config_reader

QA_TYPE_NAMES = ["QABridge", "CheatCommands", "QATools"]


def read_build_info(kind):
    path = os.path.join(config_reader.project_path(kind), "build_info.json")
    assert os.path.exists(path), "build_info.json 이 없습니다: %s" % path
    with open(path, encoding="utf-8") as info_file:
        return json.load(info_file)


def dll_contains(kind, word):
    path = config_reader.runtime_dll(kind)
    assert os.path.exists(path), "게임 코드 DLL 이 없습니다: %s" % path
    with open(path, "rb") as dll_file:
        data = dll_file.read()
    return word.encode("utf-8") in data


def test_test_build_includes_qa_bridge():
    info = read_build_info("builds_test")
    assert info["qa_bridge"] is True, "테스트용 빌드의 build_info.json qa_bridge 가 %s 입니다" % info["qa_bridge"]


def test_release_build_excludes_qa_bridge():
    info = read_build_info("builds_release")
    assert info["qa_bridge"] is False, "출시 빌드의 build_info.json qa_bridge 가 %s 입니다" % info["qa_bridge"]


@pytest.mark.parametrize("type_name", QA_TYPE_NAMES)
def test_qa_code_only_in_test_build(type_name):
    assert dll_contains("builds_test", type_name), "테스트용 빌드에 %s 가 없습니다" % type_name
    assert not dll_contains("builds_release", type_name), "출시 빌드에 %s 가 들어 있습니다" % type_name


def test_release_build_does_not_open_qa_port(config):
    executable = config_reader.game_executable("builds_release")
    assert os.path.exists(executable), "출시 빌드가 없습니다: %s" % executable
    process = subprocess.Popen([executable, "-batchmode", "-nographics", "-qaPort", str(config["game"]["port"])])
    try:
        time.sleep(8)
        connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        connection.settimeout(1)
        opened = connection.connect_ex(("127.0.0.1", config["game"]["port"])) == 0
        connection.close()
    finally:
        process.terminate()
        process.wait(timeout=20)
    assert not opened, "출시 빌드가 %d 포트를 열었습니다" % config["game"]["port"]
