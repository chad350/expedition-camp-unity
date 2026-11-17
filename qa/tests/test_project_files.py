"""프로젝트 파일 검사입니다. Unity 를 실행하지 않고 씬 · 프리팹 · 설정 파일을 직접 읽습니다.
Jenkins 2단계에서 실행합니다.
"""

import os

import pytest

from checks import asset_checks, meta_checks, prefab_checks, scene_checks
from utils import unity_yaml
from utils.config_reader import load_rules, project_path

PROJECT_RULES = load_rules("project.yaml")
PREFAB_RULES = load_rules("prefabs.yaml")
SCENE_RULES = load_rules("scenes.yaml")
ASSET_RULES = load_rules("assets.yaml")


def report(problems):
    return "\n" + "\n".join(problems)


def unity_file(unity_project, relative_path):
    return os.path.join(unity_project, relative_path)


def scene_file(unity_project, scene_name):
    return os.path.join(unity_project, PROJECT_RULES["scenes_dir"], scene_name + ".unity")


# 파일과 meta

def test_every_asset_has_meta(unity_project):
    problems = meta_checks.find_assets_without_meta(project_path("assets"), PROJECT_RULES["meta_check_skip"])
    assert problems == [], report(problems)


def test_every_meta_has_asset(unity_project):
    problems = meta_checks.find_metas_without_asset(project_path("assets"), PROJECT_RULES["meta_check_skip"])
    assert problems == [], report(problems)


def test_guid_references_exist(unity_project):
    problems = meta_checks.find_missing_guid_references(project_path("assets"), PROJECT_RULES["external_guids"],
                                                        PROJECT_RULES["guid_check_skip"])
    assert problems == [], report(problems)


# 씬과 테이블 연결

def test_stage_scenes_are_in_build_list(tables):
    build_scenes = scene_checks.read_build_scenes(project_path("build_settings"))
    problems = scene_checks.find_scenes_missing_from_build(tables["stage"], build_scenes)
    assert problems == [], report(problems)


def test_spawn_point_keys_exist_in_scene(tables, unity_project):
    spawn_guid = unity_yaml.guid_of(unity_file(unity_project, PROJECT_RULES["spawn_point_prefab"]))
    scenes_dir = os.path.join(unity_project, PROJECT_RULES["scenes_dir"])
    problems = scene_checks.find_missing_spawn_points(tables["field_object"], tables["stage"], scenes_dir, spawn_guid)
    assert problems == [], report(problems)


# 프리팹 규칙

@pytest.mark.parametrize("prefab", sorted(PREFAB_RULES["plain"].keys()), ids=os.path.basename)
def test_prefab_rules(unity_project, prefab):
    layers = unity_yaml.layer_names(project_path("tag_manager"))
    problems = prefab_checks.check_plain_prefab(unity_file(unity_project, prefab), PREFAB_RULES["plain"][prefab], layers)
    assert problems == [], report(problems)


@pytest.mark.parametrize("prefab", PREFAB_RULES["variants"]["files"], ids=os.path.basename)
def test_variant_overrides(unity_project, prefab):
    problems = prefab_checks.check_variant(unity_file(unity_project, prefab), PREFAB_RULES["variants"])
    assert problems == [], report(problems)


# 씬 규칙

@pytest.mark.parametrize("scene_name", SCENE_RULES["event_system_scenes"])
def test_scene_has_event_system(unity_project, scene_name):
    found = scene_checks.has_event_system(scene_file(unity_project, scene_name))
    assert found, "%s 씬에 EventSystem 이 없습니다. UI 버튼이 클릭에 반응하지 않습니다." % scene_name


def test_lobby_background_is_behind_npc(unity_project):
    rule = SCENE_RULES["sorting"]
    order = scene_checks.sorting_order(scene_file(unity_project, rule["scene"]), rule["background"])
    assert order is not None, "%s 씬에서 %s 를 찾을 수 없습니다" % (rule["scene"], rule["background"])
    assert order <= rule["max_background_order"], \
        "%s 의 Order in Layer 가 %d 입니다. NPC(5) 보다 뒤에 그려지려면 %d 이하여야 합니다" % (rule["background"], order, rule["max_background_order"])


@pytest.mark.parametrize("button", sorted(SCENE_RULES["anchors"]["buttons"].keys()))
def test_lobby_button_anchor(unity_project, button):
    rule = SCENE_RULES["anchors"]
    anchor = scene_checks.rect_anchor(scene_file(unity_project, rule["scene"]), button)
    assert anchor is not None, "%s 씬에서 %s 를 찾을 수 없습니다" % (rule["scene"], button)
    expected = tuple(rule["buttons"][button])
    assert anchor[0] == expected and anchor[1] == expected, \
        "%s 앵커가 Min %s · Max %s 입니다 (기준 %s)" % (button, anchor[0], anchor[1], expected)


@pytest.mark.parametrize("scene_name", SCENE_RULES["monster_instances"]["scenes"])
def test_monster_instances_keep_prefab_values(unity_project, scene_name):
    rule = SCENE_RULES["monster_instances"]
    folder = unity_file(unity_project, rule["prefab_folder"])
    source_guids = []
    for file_name in sorted(os.listdir(folder)):
        if file_name.endswith(".prefab"):
            source_guids.append(unity_yaml.guid_of(os.path.join(folder, file_name)))
    problems = scene_checks.find_forbidden_overrides(scene_file(unity_project, scene_name), source_guids, rule["allowed_overrides"])
    assert problems == [], report(problems)


@pytest.mark.parametrize("rule", SCENE_RULES["required_references"], ids=lambda rule: rule["scene"] + "." + rule["component"])
def test_required_references_are_set(unity_project, rule):
    problems = scene_checks.find_empty_references(scene_file(unity_project, rule["scene"]), rule["object"], rule["component"], rule["fields"])
    assert problems == [], report(problems)


# 설정 파일과 재질

@pytest.mark.parametrize("asset", sorted(ASSET_RULES["config_files"].keys()), ids=os.path.basename)
def test_config_values(unity_project, asset):
    problems = asset_checks.find_config_differences(unity_file(unity_project, asset), ASSET_RULES["config_files"][asset])
    assert problems == [], report(problems)


def test_material_shaders(unity_project):
    problems = asset_checks.find_wrong_shaders(unity_file(unity_project, ASSET_RULES["materials_dir"]), ASSET_RULES["allowed_shader_guids"])
    assert problems == [], report(problems)
