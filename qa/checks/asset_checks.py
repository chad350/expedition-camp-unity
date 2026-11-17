"""설정 파일(ScriptableObject)과 재질을 검사합니다."""

import os

from utils import unity_yaml


def find_config_differences(asset_path, expected_values):
    documents = unity_yaml.load_documents(asset_path)
    data = documents[0].data
    name = os.path.basename(asset_path)
    problems = []
    for field in expected_values:
        expected = expected_values[field]
        actual = data.get(field)
        if actual != expected:
            problems.append("%s %s 값이 %s 입니다 (기준 %s)" % (name, field, actual, expected))
    return problems


def find_wrong_shaders(materials_dir, allowed_shader_guids):
    problems = []
    for file_name in sorted(os.listdir(materials_dir)):
        if not file_name.endswith(".mat"):
            continue
        path = os.path.join(materials_dir, file_name)
        documents = unity_yaml.load_documents(path)
        material = None
        for document in documents:
            if document.type_name == "Material":
                material = document
        shader = material.data.get("m_Shader") or {}
        guid = shader.get("guid", "")
        if guid not in allowed_shader_guids:
            problems.append("%s 의 Shader 가 프로젝트 렌더링 방식(URP)과 다릅니다 (guid %s · fileID %s)"
                            % (file_name, guid, shader.get("fileID")))
    return problems
