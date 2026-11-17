"""config/config.yaml 을 읽고 경로를 만들어 주는 함수 모음입니다."""

import os
import platform

import yaml

QA_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CONFIG_PATH = os.path.join(QA_DIR, "config", "config.yaml")
RULES_DIR = os.path.join(QA_DIR, "rules")

_config = None


def load_config():
    global _config
    if _config is None:
        with open(CONFIG_PATH, encoding="utf-8") as config_file:
            _config = yaml.safe_load(config_file)
    return _config


def load_rules(file_name):
    path = os.path.join(RULES_DIR, file_name)
    with open(path, encoding="utf-8") as rules_file:
        return yaml.safe_load(rules_file)


def is_windows():
    return platform.system() == "Windows"


def project_root():
    config = load_config()
    return os.path.abspath(os.path.join(QA_DIR, config["project_root"]))


def project_path(key):
    config = load_config()
    relative = config["paths"][key]
    return os.path.join(project_root(), relative)


def unity_path():
    config = load_config()
    if is_windows():
        return config["unity"]["windows"]
    return config["unity"]["mac"]


def os_value(section):
    config = load_config()
    if is_windows():
        value = config[section]["windows"]
    else:
        value = config[section]["mac"]
    return os.path.expanduser(value)


def game_executable(kind):
    """kind 는 builds_test 또는 builds_release 입니다."""
    config = load_config()
    build_dir = project_path(kind)
    name = config["game"]["product_name"]
    if is_windows():
        return os.path.join(build_dir, name + ".exe")
    return os.path.join(build_dir, name + ".app", "Contents", "MacOS", name)


def runtime_dll(kind):
    config = load_config()
    build_dir = project_path(kind)
    name = config["game"]["product_name"]
    if is_windows():
        managed = os.path.join(build_dir, name + "_Data", "Managed")
    else:
        managed = os.path.join(build_dir, name + ".app", "Contents", "Resources", "Data", "Managed")
    return os.path.join(managed, "ExpeditionCamp.Runtime.dll")


def reports_dir(*parts):
    path = os.path.join(project_path("reports"), *parts)
    os.makedirs(path, exist_ok=True)
    return path
