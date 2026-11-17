"""Unity 씬 · 프리팹 · 에셋 파일(텍스트 YAML)을 읽는 함수 모음입니다.

Unity 파일은 "--- !u!클래스번호 &파일ID" 로 시작하는 문서 여러 개로 되어 있습니다.
PyYAML 이 읽지 못하는 머리 행을 정리한 뒤 문서마다 따로 읽습니다.
"""

import os
import re

import yaml

HEADER_PATTERN = re.compile(r"^--- !u!(\d+) &(-?\d+)")

CLASS_GAME_OBJECT = 1
CLASS_TRANSFORM = 4
CLASS_MONO_BEHAVIOUR = 114
CLASS_RECT_TRANSFORM = 224
CLASS_PREFAB_INSTANCE = 1001


class UnityDocument:
    def __init__(self, class_id, file_id, type_name, data):
        self.class_id = class_id
        self.file_id = file_id
        self.type_name = type_name
        self.data = data


def load_documents(path):
    """Unity 파일을 읽어서 UnityDocument 목록을 반환합니다."""
    with open(path, encoding="utf-8") as unity_file:
        lines = unity_file.read().splitlines()

    documents = []
    header = None
    body = []
    for line in lines:
        match = HEADER_PATTERN.match(line)
        if match is not None:
            if header is not None:
                documents.append(make_document(header, body))
            header = match
            body = []
            continue
        if line.startswith("%"):
            continue
        body.append(line)
    if header is not None:
        documents.append(make_document(header, body))
    return documents


def make_document(header, body):
    class_id = int(header.group(1))
    file_id = header.group(2)
    loaded = yaml.safe_load("\n".join(body))
    type_name = ""
    data = {}
    if isinstance(loaded, dict) and len(loaded) > 0:
        type_name = list(loaded.keys())[0]
        data = loaded[type_name] or {}
    return UnityDocument(class_id, file_id, type_name, data)


def read_meta_guid(meta_path):
    with open(meta_path, encoding="utf-8") as meta_file:
        for line in meta_file:
            if line.startswith("guid:"):
                return line.split(":", 1)[1].strip()
    return ""


def guid_of(asset_path):
    return read_meta_guid(asset_path + ".meta")


def find_game_objects(documents, name):
    found = []
    for document in documents:
        if document.class_id == CLASS_GAME_OBJECT and document.data.get("m_Name") == name:
            found.append(document)
    return found


def document_by_id(documents, file_id):
    for document in documents:
        if document.file_id == str(file_id):
            return document
    return None


def components_of(documents, game_object):
    components = []
    for entry in game_object.data.get("m_Component", []):
        file_id = entry["component"]["fileID"]
        component = document_by_id(documents, file_id)
        if component is not None:
            components.append(component)
    return components


def component_of_class(documents, game_object, class_id):
    for component in components_of(documents, game_object):
        if component.class_id == class_id:
            return component
    return None


def script_component(documents, game_object, class_name):
    """m_EditorClassIdentifier 가 class_name 으로 끝나는 MonoBehaviour 를 찾습니다."""
    for component in components_of(documents, game_object):
        if component.class_id != CLASS_MONO_BEHAVIOUR:
            continue
        identifier = component.data.get("m_EditorClassIdentifier") or ""
        if identifier.endswith(class_name):
            return component
    return None


def prefab_instances(documents):
    found = []
    for document in documents:
        if document.class_id == CLASS_PREFAB_INSTANCE:
            found.append(document)
    return found


def instance_source_guid(instance):
    source = instance.data.get("m_SourcePrefab") or {}
    return source.get("guid", "")


def instance_modifications(instance):
    modification = instance.data.get("m_Modification") or {}
    return modification.get("m_Modifications") or []


def instance_name(instance):
    for item in instance_modifications(instance):
        if item.get("propertyPath") == "m_Name":
            return str(item.get("value"))
    return ""


def layer_names(tag_manager_path):
    """TagManager.asset 을 읽어서 {Layer 이름: 번호} 를 반환합니다."""
    documents = load_documents(tag_manager_path)
    layers = documents[0].data.get("layers", [])
    names = {}
    for index, name in enumerate(layers):
        if name:
            names[name] = index
    return names


def relative_to(path, root):
    return os.path.relpath(path, root).replace("\\", "/")
