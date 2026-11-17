"""파일과 .meta 짝, GUID 참조를 검사합니다."""

import os
import re

from utils.unity_yaml import read_meta_guid, relative_to

GUID_PATTERN = re.compile(r"guid: ([0-9a-f]{32})")
UNITY_TEXT_EXTENSIONS = (".unity", ".prefab", ".asset", ".mat", ".controller", ".anim", ".spriteatlasv2")


def is_skipped(path, assets_dir, skip):
    relative = relative_to(path, os.path.dirname(assets_dir))
    for prefix in skip:
        if relative == prefix or relative.startswith(prefix + "/") or relative == prefix + ".meta":
            return True
    return False


def walk_assets(assets_dir, skip=()):
    """Assets 아래 모든 파일과 폴더 경로를 반환합니다. skip 에 있는 경로는 뺍니다."""
    paths = []
    for folder, sub_folders, files in os.walk(assets_dir):
        for name in sub_folders:
            paths.append(os.path.join(folder, name))
        for name in files:
            if name == ".DS_Store":
                continue
            paths.append(os.path.join(folder, name))
    kept = []
    for path in paths:
        if not is_skipped(path, assets_dir, skip):
            kept.append(path)
    return kept


def find_assets_without_meta(assets_dir, skip=()):
    """Assets 아래 파일 · 폴더 중 .meta 짝이 없는 것을 찾아 문제 목록(문자열)을 반환합니다."""
    raise NotImplementedError


def find_metas_without_asset(assets_dir, skip=()):
    """Assets 아래 .meta 중 짝이 되는 파일 · 폴더가 없는 것을 찾아 문제 목록을 반환합니다."""
    raise NotImplementedError


def collect_asset_guids(assets_dir):
    guids = {}
    for path in walk_assets(assets_dir):
        if not path.endswith(".meta"):
            continue
        asset_path = path[:-len(".meta")]
        if not os.path.exists(asset_path):
            continue
        guids[read_meta_guid(path)] = asset_path
    return guids


def find_missing_guid_references(assets_dir, allowed_guids, skip=()):
    """씬 · 프리팹 · 에셋 파일이 참조하는 GUID 중 Assets 의 meta 에도 allowed_guids 에도 없는 것을 찾아 문제 목록을 반환합니다."""
    raise NotImplementedError
