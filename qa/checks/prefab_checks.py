"""프리팹 규칙(Tag · Layer · Is Trigger · 기준 값)을 검사합니다."""

import os

from utils import unity_yaml


def root_game_object(documents):
    """부모가 없는 Transform 의 GameObject 를 찾습니다."""
    for document in documents:
        if document.class_id not in (unity_yaml.CLASS_TRANSFORM, unity_yaml.CLASS_RECT_TRANSFORM):
            continue
        father = document.data.get("m_Father") or {}
        if father.get("fileID", 0) == 0:
            game_object = unity_yaml.document_by_id(documents, document.data["m_GameObject"]["fileID"])
            if game_object is not None:
                return game_object
    return None


def all_game_objects(documents):
    found = []
    for document in documents:
        if document.class_id == unity_yaml.CLASS_GAME_OBJECT:
            found.append(document)
    return found


def check_plain_prefab(prefab_path, rule, layers):
    """Variant 가 아닌 프리팹의 Tag · Layer · Collider 규칙을 검사합니다."""
    documents = unity_yaml.load_documents(prefab_path)
    name = os.path.basename(prefab_path)
    problems = []
    root = root_game_object(documents)
    if root is None:
        return ["%s 의 루트 오브젝트를 찾을 수 없습니다" % name]

    tag = root.data.get("m_TagString")
    if "tag" in rule and tag != rule["tag"]:
        problems.append("%s 루트 Tag 가 %s 입니다 (기준 %s)" % (name, tag, rule["tag"]))

    if "layer" in rule:
        expected = layers[rule["layer"]]
        targets = [root]
        if rule.get("children_same_layer"):
            targets = all_game_objects(documents)
        for game_object in targets:
            if game_object.data.get("m_Layer") != expected:
                problems.append("%s 의 %s Layer 번호가 %s 입니다 (기준 %s · %d)"
                                % (name, game_object.data.get("m_Name"), game_object.data.get("m_Layer"), rule["layer"], expected))

    if "is_trigger" in rule:
        collider = None
        for component in unity_yaml.components_of(documents, root):
            if "m_IsTrigger" in component.data:
                collider = component
        if collider is None:
            problems.append("%s 루트에 Collider 가 없습니다" % name)
        else:
            value = collider.data.get("m_IsTrigger") == 1
            if value != rule["is_trigger"]:
                problems.append("%s %s Is Trigger 가 %s 입니다 (기준 %s)" % (name, collider.type_name, value, rule["is_trigger"]))

    script_values = rule.get("script_values") or {}
    for class_name in script_values:
        component = unity_yaml.script_component(documents, root, class_name)
        if component is None:
            problems.append("%s 루트에 %s 컴포넌트가 없습니다 (Missing Script 일 수 있습니다)" % (name, class_name))
            continue
        for field in script_values[class_name]:
            expected = script_values[class_name][field]
            actual = component.data.get(field)
            if actual != expected:
                problems.append("%s %s.%s 값이 %s 입니다 (기준 %s)" % (name, class_name, field, actual, expected))
    return problems


def check_variant(prefab_path, rule):
    """Variant 프리팹이 허용 목록 밖의 값을 바꿨는지 검사합니다."""
    documents = unity_yaml.load_documents(prefab_path)
    name = os.path.basename(prefab_path)
    problems = []
    instances = unity_yaml.prefab_instances(documents)
    if len(instances) == 0:
        return ["%s 은 Variant 가 아닙니다" % name]
    allowed_paths = rule["allowed_overrides"]
    for item in unity_yaml.instance_modifications(instances[0]):
        path = item.get("propertyPath", "")
        allowed = False
        for prefix in allowed_paths:
            if path == prefix or path.startswith(prefix + "."):
                allowed = True
        if not allowed:
            problems.append("%s Variant 가 %s 를 %s 로 바꿨습니다" % (name, path, item.get("value")))
    return problems
