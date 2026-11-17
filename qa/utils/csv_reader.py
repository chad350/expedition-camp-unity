"""게임용 CSV 를 UTF-8 로 읽습니다."""

import csv
import io
import os

from utils.config_reader import project_path


class TableReadError(Exception):
    pass


def read_table(table_name):
    """CSV 를 읽어서 행 목록을 반환합니다.

    행마다 "_row" 키에 CSV 행 번호(머리 행이 1)를 넣어 둡니다.
    UTF-8 로 읽을 수 없으면 TableReadError 를 냅니다.
    """
    path = os.path.join(project_path("data"), table_name + ".csv")
    with open(path, "rb") as csv_file:
        raw = csv_file.read()
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as error:
        line_number = raw[:error.start].count(b"\n") + 1
        raise TableReadError("%s.csv %d행을 UTF-8 로 읽을 수 없습니다" % (table_name, line_number))

    reader = csv.DictReader(io.StringIO(text))
    rows = []
    row_number = 1
    for row in reader:
        row_number += 1
        row["_row"] = row_number
        rows.append(row)
    return rows
