"""main 브랜치 위에 실수 재현 브랜치(break/*)를 다시 만듭니다.

main 을 고친 뒤 실행하면 break 브랜치를 main 의 최신 커밋에서 새로 만들고,
브랜치마다 실수 1가지를 적용한 커밋 1개를 만듭니다.
기존 break 브랜치는 덮어씁니다.

사용 방법 (저장소 최상위 폴더에서 실행, Unity 에디터는 닫아 둡니다)
    Windows  py tools\\rebuild_break_branches.py
    macOS    python3 tools/rebuild_break_branches.py
    일부만   python3 tools/rebuild_break_branches.py --only 01,03
    Unity 경로 지정  python3 tools/rebuild_break_branches.py --unity "C:\\Program Files\\Unity\\Hub\\Editor\\6000.3.20f1\\Editor\\Unity.exe"
"""

import argparse
import os
import platform
import shutil
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PROJECT = os.path.join(ROOT, "UnityProject")
APPLIER_SOURCE = os.path.join(ROOT, "tools", "break", "BreakApplier.cs")
TEMP_FOLDER = os.path.join(PROJECT, "Assets", "Editor", "BreakTemp")
LOG_FOLDER = os.path.join(ROOT, "logs")

MAC_UNITY = "/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity"
WINDOWS_UNITY = r"C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe"

NPC_DIR = "UnityProject/Assets/Sprites/NPC"
STRING_CSV = "UnityProject/Assets/StreamingAssets/data/string.csv"
FIELD_01 = "UnityProject/Assets/Scenes/Field_01.unity"
LOBBY = "UnityProject/Assets/Scenes/Lobby.unity"

# (번호, 브랜치 이름, 적용 방식, 커밋할 경로, 커밋 메시지)
BREAKS = [
    ("01", "break/01-meta", "file", [NPC_DIR], "chore: rename npc image files"),
    ("02", "break/02-script", "file", ["UnityProject/Assets/Scripts/Field"], "refactor: rename monster script file"),
    ("03", "break/03-null", "unity", [FIELD_01], "chore: update Field_01 scene"),
    ("04", "break/04-override", "unity", [FIELD_01], "chore: tune monster placement in Field_01"),
    ("05", "break/05-apply", "unity", ["UnityProject/Assets/Prefabs/Monster_Base.prefab"], "chore: apply monster prefab overrides"),
    ("06", "break/06-tag", "unity", ["UnityProject/Assets/Resources/Monsters/Monster_Rabbit.prefab"], "chore: update rabbit monster prefab"),
    ("07", "break/07-layer", "unity", ["UnityProject/Assets/Prefabs/DropItem.prefab"], "chore: update drop item prefab layer"),
    ("08", "break/08-trigger", "unity", ["UnityProject/Assets/Prefabs/DropItem.prefab"], "chore: update drop item collider"),
    ("09", "break/09-sorting", "unity", [LOBBY], "chore: adjust lobby background"),
    ("10", "break/10-anchor", "unity", [LOBBY], "chore: adjust lobby button layout"),
    ("11", "break/11-eventsystem", "unity", [LOBBY], "chore: clean up lobby hierarchy"),
    ("12", "break/12-buildlist", "unity", ["UnityProject/ProjectSettings/EditorBuildSettings.asset"], "chore: update build scene list"),
    ("13", "break/13-spawnkey", "unity", [FIELD_01], "chore: rename spawn points in Field_01"),
    ("14", "break/14-csv-excel", "file", [STRING_CSV], "chore: update string table"),
    ("15", "break/15-config", "unity", ["UnityProject/Assets/Settings/GameConfig.asset"], "chore: update game config"),
    ("16", "break/16-material", "unity", ["UnityProject/Assets/Materials/Rock.mat"], "chore: update rock material"),
]


def git(*arguments):
    result = subprocess.run(["git"] + list(arguments), cwd=ROOT, capture_output=True, text=True)
    if result.returncode != 0:
        print(result.stdout)
        print(result.stderr)
        raise SystemExit("git %s 실행에 실패했습니다" % " ".join(arguments))
    return result.stdout.strip()


def default_unity():
    if platform.system() == "Windows":
        return WINDOWS_UNITY
    return MAC_UNITY


def apply_file_break(break_id):
    if break_id == "01":
        old_path = os.path.join(ROOT, NPC_DIR, "npc_shop.png")
        new_path = os.path.join(ROOT, NPC_DIR, "npc_shopkeeper.png")
        os.rename(old_path, new_path)
        return
    if break_id == "02":
        folder = os.path.join(ROOT, "UnityProject", "Assets", "Scripts", "Field")
        os.rename(os.path.join(folder, "Monster.cs"), os.path.join(folder, "MonsterUnit.cs"))
        return
    if break_id == "14":
        path = os.path.join(ROOT, STRING_CSV)
        with open(path, encoding="utf-8") as csv_file:
            text = csv_file.read()
        with open(path, "w", encoding="cp949", errors="replace", newline="") as csv_file:
            csv_file.write(text.replace("\n", "\r\n"))
        return
    raise SystemExit("파일 방식으로 정의되지 않은 번호입니다: " + break_id)


def apply_unity_break(break_id, unity_path):
    os.makedirs(TEMP_FOLDER, exist_ok=True)
    shutil.copyfile(APPLIER_SOURCE, os.path.join(TEMP_FOLDER, "BreakApplier.cs"))
    os.makedirs(LOG_FOLDER, exist_ok=True)
    log_path = os.path.join(LOG_FOLDER, "break_%s.log" % break_id)
    command = [
        unity_path, "-batchmode", "-nographics", "-projectPath", PROJECT,
        "-executeMethod", "ExpeditionCamp.EditorTools.BreakApplier.Apply",
        "-breakId", break_id, "-quit", "-logFile", log_path,
    ]
    try:
        result = subprocess.run(command)
    finally:
        shutil.rmtree(TEMP_FOLDER, ignore_errors=True)
        meta_path = TEMP_FOLDER + ".meta"
        if os.path.exists(meta_path):
            os.remove(meta_path)
    if result.returncode != 0:
        raise SystemExit("Unity 실행에 실패했습니다 (종료 코드 %d) · 로그 %s" % (result.returncode, log_path))


def rebuild(spec, unity_path, base):
    break_id, branch, kind, paths, message = spec
    git("checkout", "-q", "-B", branch, base)
    if kind == "file":
        apply_file_break(break_id)
    else:
        apply_unity_break(break_id, unity_path)

    for path in paths:
        git("add", "-A", "--", path)
    staged = git("diff", "--cached", "--name-status")
    if staged == "":
        raise SystemExit("%s 에서 바뀐 파일이 없습니다" % branch)
    git("commit", "-q", "-m", message + "\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>")
    git("reset", "-q", "--hard")
    git("clean", "-q", "-fd", "--", "UnityProject/Assets", "UnityProject/ProjectSettings")
    print("%s 커밋을 만들었습니다" % branch)
    for line in staged.splitlines():
        print("    " + line)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--only", default="", help="쉼표로 나눈 번호 목록 (예: 01,03)")
    parser.add_argument("--unity", default=default_unity(), help="Unity 실행 파일 경로")
    parser.add_argument("--base", default="main", help="break 브랜치를 만들 기준 브랜치")
    options = parser.parse_args()

    if git("status", "--porcelain", "--untracked-files=no") != "":
        raise SystemExit("커밋하지 않은 변경이 있습니다. 먼저 커밋하거나 되돌려 주세요.")

    selected = []
    wanted = [item.strip() for item in options.only.split(",") if item.strip() != ""]
    for spec in BREAKS:
        if len(wanted) == 0 or spec[0] in wanted:
            selected.append(spec)

    start_branch = git("rev-parse", "--abbrev-ref", "HEAD")
    try:
        for spec in selected:
            rebuild(spec, options.unity, options.base)
    finally:
        git("checkout", "-q", start_branch)
    print("break 브랜치 %d개를 만들었습니다" % len(selected))


if __name__ == "__main__":
    sys.exit(main())
