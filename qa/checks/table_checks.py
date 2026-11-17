"""데이터 테이블(CSV) 검사 함수 모음입니다.

함수마다 문제 목록(문자열)을 반환하고, 문제가 없으면 빈 목록을 반환합니다.
"""


def find_empty_values(table_name, rows, columns):
    problems = []
    for row in rows:
        for column in columns:
            value = row.get(column)
            if value is None or value.strip() == "":
                problems.append("%s %d행 %s 칸이 비어 있습니다" % (table_name, row["_row"], column))
    return problems


def find_not_integer(table_name, rows, columns):
    problems = []
    for row in rows:
        for column in columns:
            value = row.get(column, "")
            try:
                int(value)
            except ValueError:
                problems.append("%s %d행 %s 값 %r 이 정수가 아닙니다" % (table_name, row["_row"], column, value))
    return problems


def find_duplicates(table_name, rows, columns):
    problems = []
    seen = {}
    for row in rows:
        key_parts = []
        for column in columns:
            key_parts.append(row.get(column, ""))
        key = tuple(key_parts)
        if key in seen:
            problems.append("%s %d행과 %d행의 %s 값 %s 이 같습니다" % (table_name, seen[key], row["_row"], "+".join(columns), "/".join(key)))
        else:
            seen[key] = row["_row"]
    return problems


def find_out_of_range(table_name, rows, column, minimum, maximum):
    problems = []
    for row in rows:
        value = int(row[column])
        if minimum is not None and value < minimum:
            problems.append("%s %d행 %s 값 %d 이 %d 보다 작습니다" % (table_name, row["_row"], column, value, minimum))
        if maximum is not None and value > maximum:
            problems.append("%s %d행 %s 값 %d 이 %d 보다 큽니다" % (table_name, row["_row"], column, value, maximum))
    return problems


def find_missing_references(table_name, rows, column, target_name, target_rows, target_column, ignore_values):
    targets = set()
    for target in target_rows:
        targets.add(target[target_column])
    problems = []
    for row in rows:
        value = row[column]
        if value in ignore_values:
            continue
        if value not in targets:
            problems.append("%s %d행 %s 값 %s 이 %s 의 %s 에 없습니다" % (table_name, row["_row"], column, value, target_name, target_column))
    return problems


def find_wrong_random_one_sum(rows, total):
    sums = {}
    first_row = {}
    for row in rows:
        if row["drop_type"] != "RANDOM_ONE":
            continue
        group = row["drop_group_id"]
        sums[group] = sums.get(group, 0) + int(row["prob"])
        if group not in first_row:
            first_row[group] = row["_row"]
    problems = []
    for group in sums:
        if sums[group] != total:
            problems.append("drop_group %s (%d행부터) RANDOM_ONE 확률 합계가 %d 입니다 (기준 %d)" % (group, first_row[group], sums[group], total))
    return problems


def find_field_monsters_without_prefab(field_rows, monster_rows):
    prefab_keys = {}
    for monster in monster_rows:
        prefab_keys[monster["monster_id"]] = monster.get("prefab_key", "")
    problems = []
    for row in field_rows:
        key = prefab_keys.get(row["monster_id"], "")
        if key == "":
            problems.append("field_object %d행 monster_id %s 은 prefab_key 가 없는 몬스터입니다" % (row["_row"], row["monster_id"]))
    return problems


def find_csv_differences(temp_dir):
    """tools/export_tables.py 를 temp_dir 로 실행한 결과와 게임 폴더 CSV 를 비교합니다."""
    import os
    import subprocess
    import sys

    from utils.config_reader import project_path, project_root

    tool = os.path.join(project_root(), "tools", "export_tables.py")
    result = subprocess.run([sys.executable, tool, "--out-dir", temp_dir], capture_output=True, text=True)
    if result.returncode != 0:
        return ["변환 도구가 실패했습니다 (원본 엑셀에 타입이 맞지 않는 값이 있습니다)", result.stdout[-1500:]]

    problems = []
    data_dir = project_path("data")
    for file_name in sorted(os.listdir(temp_dir)):
        if not file_name.endswith(".csv"):
            continue
        with open(os.path.join(temp_dir, file_name), "rb") as exported_file:
            exported = exported_file.read()
        game_path = os.path.join(data_dir, file_name)
        if not os.path.exists(game_path):
            problems.append("%s 가 게임 폴더에 없습니다" % file_name)
            continue
        with open(game_path, "rb") as game_file:
            current = game_file.read()
        if exported != current:
            problems.append("%s 가 원본 엑셀 변환 결과와 다릅니다 (CSV 를 직접 저장했거나 변환 도구를 실행하지 않았습니다)" % file_name)
    return problems
