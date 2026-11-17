"""Unity 를 batchmode 로 실행합니다. Jenkins 3 · 4단계에서 사용합니다.

사용 방법 (qa 폴더에서 실행)
    python utils/unity_runner.py tests EditMode
    python utils/unity_runner.py tests PlayMode
    python utils/unity_runner.py build test
    python utils/unity_runner.py build release --build-number 12

종료 코드
    0 성공 · 1 실패 (컴파일 오류 · 테스트 실패 · 빌드 실패 · 시간 초과)
"""

import argparse
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from utils import config_reader  # noqa: E402

TIMEOUT_SECOND = 1800
UNITY_EXIT_MEANING = {
    0: "성공",
    1: "실패 (컴파일 오류 · 빌드 실패 · 실행 오류)",
    2: "테스트 중 실패한 테스트가 있습니다",
    3: "실행 인자 오류",
}


def clean_environment():
    """Unity 빌드 도구가 읽지 못하는 환경 변수를 뺍니다.

    한글이 들어간 경로에서 파이썬을 실행하면 "_" 변수에 깨진 글자가 들어가는 경우가 있고,
    Unity 는 이 값을 읽다가 컴파일을 멈춥니다.
    """
    environment = {}
    for key in os.environ:
        value = os.environ[key]
        if key == "_" or key.startswith("__PYVENV") or key == "VIRTUAL_ENV":
            continue
        try:
            (key + value).encode("utf-8")
        except UnicodeEncodeError:
            continue
        environment[key] = value
    environment["LANG"] = "en_US.UTF-8"
    environment["LC_ALL"] = "en_US.UTF-8"
    return environment


def run_unity(arguments, log_name):
    log_path = os.path.join(config_reader.reports_dir(), log_name)
    command = [config_reader.unity_path(), "-batchmode", "-projectPath", config_reader.project_path("unity_project"),
               "-logFile", log_path] + arguments
    print("Unity 실행: " + " ".join(arguments))
    try:
        result = subprocess.run(command, timeout=TIMEOUT_SECOND, env=clean_environment())
        exit_code = result.returncode
    except subprocess.TimeoutExpired:
        print("Unity 가 %d초 안에 끝나지 않았습니다" % TIMEOUT_SECOND)
        return 1, log_path
    print("Unity 종료 코드 %d · %s" % (exit_code, UNITY_EXIT_MEANING.get(exit_code, "알 수 없는 코드")))
    return exit_code, log_path


def print_error_lines(log_path):
    if not os.path.exists(log_path):
        print("로그 파일이 없습니다: " + log_path)
        return
    with open(log_path, encoding="utf-8", errors="replace") as log_file:
        lines = log_file.read().splitlines()
    shown = 0
    for line in lines:
        if "error CS" in line or "Exception:" in line or "[BuildScript]" in line:
            print("  " + line[:300])
            shown += 1
        if shown >= 20:
            break


def run_tests(platform):
    xml_path = os.path.join(config_reader.reports_dir(), "unity_%s.xml" % platform.lower())
    if os.path.exists(xml_path):
        os.remove(xml_path)
    arguments = ["-runTests", "-testPlatform", platform, "-testResults", xml_path]
    if platform == "EditMode":
        arguments.append("-nographics")
    exit_code, log_path = run_unity(arguments, "unity_%s.log" % platform.lower())
    if not os.path.exists(xml_path):
        print("테스트 결과 XML 이 만들어지지 않았습니다. 컴파일 오류인지 로그를 확인해 주세요.")
        print_error_lines(log_path)
        return 1
    if exit_code != 0:
        print_error_lines(log_path)
        return 1
    print("테스트 결과: " + xml_path)
    return 0


def run_build(kind, build_number):
    method = "ExpeditionCamp.EditorTools.BuildScript.BuildForTest"
    if kind == "release":
        method = "ExpeditionCamp.EditorTools.BuildScript.BuildForRelease"
    arguments = ["-nographics", "-executeMethod", method, "-buildNumber", str(build_number), "-quit"]
    exit_code, log_path = run_unity(arguments, "unity_build_%s.log" % kind)
    print_error_lines(log_path)
    if exit_code != 0:
        return 1
    return 0


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("command", choices=["tests", "build"])
    parser.add_argument("target", help="tests 는 EditMode 또는 PlayMode, build 는 test 또는 release")
    parser.add_argument("--build-number", type=int, default=0)
    options = parser.parse_args()
    if options.command == "tests":
        return run_tests(options.target)
    return run_build(options.target, options.build_number)


if __name__ == "__main__":
    sys.exit(main())
