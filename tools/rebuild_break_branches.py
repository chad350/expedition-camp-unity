"""main 브랜치 위에 실수 재현 브랜치(break/*)를 다시 만듭니다.

main 을 고친 뒤 실행하면 break 브랜치를 main 의 최신 커밋에서 새로 만들고,
브랜치마다 실수 1가지를 적용한 커밋 1개를 만듭니다.
기존 break 브랜치는 덮어씁니다.

사용 방법 (저장소 최상위 폴더에서 실행, Unity 에디터는 닫아 둡니다)
    Windows  py tools\\rebuild_break_branches.py
    macOS    python3 tools/rebuild_break_branches.py
    일부만   python3 tools/rebuild_break_branches.py --only 01,02
    06 · 07 은 엑셀을 고치므로 openpyxl 이 있는 파이썬으로 실행합니다 (예: tools/.venv/bin/python)
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

STRING_CSV = "UnityProject/Assets/StreamingAssets/data/string.csv"
FIELD_01 = "UnityProject/Assets/Scenes/Field_01.unity"
LOBBY = "UnityProject/Assets/Scenes/Lobby.unity"
MONSTER_CSV = "UnityProject/Assets/StreamingAssets/data/monster.csv"
ITEM_CSV = "UnityProject/Assets/StreamingAssets/data/item.csv"

# (번호, 브랜치 이름, 적용 방식, 커밋할 경로, 커밋 메시지)
# 01 ~ 08 은 2단계, 09 ~ 16 은 3단계에서 씁니다 (2026-11-19 번호 정리, 이전 번호는 docs/실수재현_정답표.md 7절).
BREAKS = [
    ("01", "break/01-sorting", "unity", [LOBBY], "chore: adjust lobby background"),
    ("02", "break/02-null", "unity", [FIELD_01], "chore: update Field_01 scene"),
    ("03", "break/03-anchor", "unity", [LOBBY], "chore: adjust lobby button layout"),
    ("04", "break/04-material", "unity", ["UnityProject/Assets/Materials/Rock.mat"], "chore: update rock material"),
    ("05", "break/05-csv-excel", "file", [STRING_CSV], "chore: update string table"),
    ("06", "break/06-dropref", "file", ["design_data/Monster.xlsx", MONSTER_CSV], "chore: update monster table"),
    ("07", "break/07-iconpath", "file", ["design_data/Item.xlsx", ITEM_CSV], "chore: update item table"),
    ("08", "break/08-shopicon", "unity", ["UnityProject/Assets/Prefabs/UI/UI_ShopSlot.prefab"], "chore: update shop slot layout"),
    ("09", "break/09-override", "unity", [FIELD_01], "chore: tune monster placement in Field_01"),
    ("10", "break/10-apply", "unity", ["UnityProject/Assets/Prefabs/Monster_Base.prefab"], "chore: apply monster prefab overrides"),
    ("11", "break/11-tag", "unity", ["UnityProject/Assets/Resources/Monsters/Monster_Rabbit.prefab"], "chore: update rabbit monster prefab"),
    ("12", "break/12-layer", "unity", ["UnityProject/Assets/Prefabs/DropItem.prefab"], "chore: update drop item prefab layer"),
    ("13", "break/13-trigger", "unity", ["UnityProject/Assets/Prefabs/DropItem.prefab"], "chore: update drop item collider"),
    ("14", "break/14-eventsystem", "unity", [LOBBY], "chore: clean up lobby hierarchy"),
    ("15", "break/15-buildlist", "unity", ["UnityProject/ProjectSettings/EditorBuildSettings.asset"], "chore: update build scene list"),
    ("16", "break/16-config", "unity", ["UnityProject/Assets/Settings/GameConfig.asset"], "chore: update game config"),
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
    if break_id == "05":
        path = os.path.join(ROOT, STRING_CSV)
        with open(path, encoding="utf-8") as csv_file:
            text = csv_file.read()
        with open(path, "w", encoding="cp949", errors="replace", newline="") as csv_file:
            csv_file.write(text.replace("\n", "\r\n"))
        return
    if break_id == "06":
        # 물방울게의 드롭 그룹 ID 에서 0 하나가 빠진 오타입니다 (엑셀과 CSV 를 함께 바꿉니다).
        edit_design_cell("Monster.xlsx", "monster", "monster_id", 100002, "drop_group_id", 30002)
        return
    if break_id == "07":
        # 소형 회복 물약 아이콘 경로에 몬스터 그림 경로를 잘못 붙여 넣은 경우입니다.
        edit_design_cell("Item.xlsx", "item", "item_id", 202001, "icon_path", "Icons/Monster/mon_100004")
        return
    raise SystemExit("파일 방식으로 정의되지 않은 번호입니다: " + break_id)


def edit_design_cell(file_name, sheet_name, key_column, key_value, column, new_value):
    """기획 원본 엑셀의 칸 1개를 바꾸고 변환 도구로 CSV 를 다시 만듭니다."""
    try:
        from openpyxl import load_workbook
    except ImportError:
        raise SystemExit("openpyxl 이 필요합니다. tools/.venv 의 파이썬으로 실행해 주세요 (변환 도구 실행 파일을 한 번 실행하면 만들어집니다).")
    path = os.path.join(ROOT, "design_data", file_name)
    workbook = load_workbook(path)
    sheet = workbook[sheet_name]
    columns = {}
    for cell in sheet[1]:
        if cell.value is not None:
            columns[str(cell.value).strip()] = cell.column
    key_index = columns[key_column]
    target_index = columns[column]
    changed = 0
    for row in range(2, sheet.max_row + 1):
        if str(sheet.cell(row, key_index).value).strip() == str(key_value):
            sheet.cell(row, target_index).value = new_value
            changed += 1
    if changed != 1:
        raise SystemExit("%s %s 시트에서 %s %s 행을 1개 찾지 못했습니다 (%d개)" % (file_name, sheet_name, key_column, key_value, changed))
    workbook.save(path)
    result = subprocess.run([sys.executable, os.path.join(ROOT, "tools", "export_tables.py")], cwd=ROOT)
    if result.returncode != 0:
        raise SystemExit("변환 도구 실행에 실패했습니다")


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
