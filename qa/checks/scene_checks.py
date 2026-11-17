"""빌드 씬 목록 · SpawnPoint 이름 · 씬 규칙을 검사합니다."""

import os

from utils import unity_yaml


def read_build_scenes(build_settings_path):
    """EditorBuildSettings.asset 에서 켜져 있는(enabled 1) 씬 경로 목록을 반환합니다."""
    raise NotImplementedError


def find_scenes_missing_from_build(stage_rows, build_scenes):
    """stage 시트의 scene_name 중 빌드 씬 목록에 없는 것을 찾아 문제 목록을 반환합니다."""
    raise NotImplementedError


def spawn_point_names(scene_path, spawn_prefab_guid):
    documents = unity_yaml.load_documents(scene_path)
    names = []
    for instance in unity_yaml.prefab_instances(documents):
        if unity_yaml.instance_source_guid(instance) == spawn_prefab_guid:
            names.append(unity_yaml.instance_name(instance))
    return names


def find_missing_spawn_points(field_rows, stage_rows, scenes_dir, spawn_prefab_guid):
    scene_of_stage = {}
    for row in stage_rows:
        scene_of_stage[row["stage_id"]] = row["scene_name"]

    names_by_scene = {}
    problems = []
    for row in field_rows:
        scene_name = scene_of_stage.get(row["stage_id"], "")
        if scene_name not in names_by_scene:
            scene_path = os.path.join(scenes_dir, scene_name + ".unity")
            names_by_scene[scene_name] = spawn_point_names(scene_path, spawn_prefab_guid)
        if row["spawn_point_key"] not in names_by_scene[scene_name]:
            problems.append("field_object %d행 spawn_point_key %s 이 %s 씬에 없습니다 (씬의 SpawnPoint: %s)"
                            % (row["_row"], row["spawn_point_key"], scene_name, ", ".join(names_by_scene[scene_name])))
    return problems


def has_event_system(scene_path):
    documents = unity_yaml.load_documents(scene_path)
    for document in documents:
        if document.class_id != unity_yaml.CLASS_MONO_BEHAVIOUR:
            continue
        identifier = document.data.get("m_EditorClassIdentifier") or ""
        if identifier.endswith("EventSystems.EventSystem"):
            return True
    return False


def sorting_order(scene_path, object_name):
    """씬에 직접 있는 오브젝트의 Sprite Renderer Order in Layer 를 반환합니다."""
    documents = unity_yaml.load_documents(scene_path)
    found = unity_yaml.find_game_objects(documents, object_name)
    if len(found) == 0:
        return None
    renderer = unity_yaml.component_of_class(documents, found[0], 212)
    if renderer is None:
        return None
    return renderer.data.get("m_SortingOrder", 0)


def rect_anchor(scene_path, object_name):
    """씬에 직접 있는 UI 오브젝트의 (anchorMin, anchorMax) 를 반환합니다."""
    documents = unity_yaml.load_documents(scene_path)
    found = unity_yaml.find_game_objects(documents, object_name)
    if len(found) == 0:
        return None
    rect = unity_yaml.component_of_class(documents, found[0], unity_yaml.CLASS_RECT_TRANSFORM)
    anchor_min = rect.data["m_AnchorMin"]
    anchor_max = rect.data["m_AnchorMax"]
    return (anchor_min["x"], anchor_min["y"]), (anchor_max["x"], anchor_max["y"])


def find_forbidden_overrides(scene_path, source_guids, allowed_paths):
    """source_guids 프리팹의 씬 인스턴스에서 허용 목록 밖의 오버라이드를 찾습니다."""
    documents = unity_yaml.load_documents(scene_path)
    problems = []
    for instance in unity_yaml.prefab_instances(documents):
        if unity_yaml.instance_source_guid(instance) not in source_guids:
            continue
        name = unity_yaml.instance_name(instance)
        for item in unity_yaml.instance_modifications(instance):
            path = item.get("propertyPath", "")
            allowed = False
            for prefix in allowed_paths:
                if path == prefix or path.startswith(prefix + "."):
                    allowed = True
            if not allowed:
                problems.append("%s 의 %s 인스턴스가 %s 를 %s 로 바꿨습니다"
                                % (os.path.basename(scene_path), name, path, item.get("value")))
    return problems


def find_empty_references(scene_path, object_name, class_name, fields):
    """object_name 오브젝트의 class_name 컴포넌트에서 None 인 참조 칸을 찾습니다."""
    documents = unity_yaml.load_documents(scene_path)
    found = unity_yaml.find_game_objects(documents, object_name)
    scene_name = os.path.basename(scene_path)
    if len(found) == 0:
        return ["%s 에서 %s 오브젝트를 찾을 수 없습니다" % (scene_name, object_name)]
    component = unity_yaml.script_component(documents, found[0], class_name)
    if component is None:
        return ["%s 의 %s 에 %s 컴포넌트가 없습니다" % (scene_name, object_name, class_name)]
    problems = []
    for field in fields:
        value = component.data.get(field) or {}
        if value.get("fileID", 0) == 0:
            problems.append("%s > %s > %s 의 %s 칸이 비어 있습니다 (None)" % (scene_name, object_name, class_name, field))
    return problems
