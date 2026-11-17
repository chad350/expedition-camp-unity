"""데이터 테이블 검사입니다. Jenkins 1단계에서 실행합니다."""

import pytest

from checks import table_checks
from utils.config_reader import load_rules

RULES = load_rules("tables.yaml")


def report(problems):
    return "\n" + "\n".join(problems)


def test_all_tables_are_utf8(tables):
    assert tables.errors == [], report(tables.errors)


@pytest.mark.parametrize("table_name", sorted(RULES["required"].keys()))
def test_required_values(tables, table_name):
    problems = table_checks.find_empty_values(table_name, tables[table_name], RULES["required"][table_name])
    assert problems == [], report(problems)


@pytest.mark.parametrize("table_name", sorted(RULES["integer"].keys()))
def test_integer_columns(tables, table_name):
    problems = table_checks.find_not_integer(table_name, tables[table_name], RULES["integer"][table_name])
    assert problems == [], report(problems)


@pytest.mark.parametrize("rule", RULES["unique"], ids=lambda rule: rule["table"] + "." + "+".join(rule["columns"]))
def test_unique_values(tables, rule):
    problems = table_checks.find_duplicates(rule["table"], tables[rule["table"]], rule["columns"])
    assert problems == [], report(problems)


@pytest.mark.parametrize("rule", RULES["ranges"], ids=lambda rule: rule["table"] + "." + rule["column"])
def test_value_ranges(tables, rule):
    problems = table_checks.find_out_of_range(rule["table"], tables[rule["table"]], rule["column"], rule["min"], rule["max"])
    assert problems == [], report(problems)


@pytest.mark.parametrize("rule", RULES["references"], ids=lambda rule: rule["table"] + "." + rule["column"])
def test_references(tables, rule):
    problems = table_checks.find_missing_references(rule["table"], tables[rule["table"]], rule["column"],
                                                    rule["target"], tables[rule["target"]], rule["target_column"], rule["ignore"])
    assert problems == [], report(problems)


def test_random_one_probability_sum(tables):
    problems = table_checks.find_wrong_random_one_sum(tables["drop_group"], RULES["prob_total"])
    assert problems == [], report(problems)


def test_field_monsters_have_prefab(tables):
    problems = table_checks.find_field_monsters_without_prefab(tables["field_object"], tables["monster"])
    assert problems == [], report(problems)


def test_csv_matches_design_data(tmp_path):
    """원본 엑셀을 임시 폴더로 변환해서, 저장소의 CSV 와 내용이 같은지 비교합니다.

    CSV 를 직접 고치거나 변환 도구를 실행하지 않고 커밋하면 실패합니다.
    """
    problems = table_checks.find_csv_differences(str(tmp_path))
    assert problems == [], report(problems)
