"""QA 연결 컴포넌트(http://127.0.0.1:7700)에 요청을 보내는 클라이언트입니다."""

import time

import requests


class GameClient:
    def __init__(self, port):
        self.base_url = "http://127.0.0.1:%d" % port

    def get(self, path):
        response = requests.get(self.base_url + path, timeout=15)
        return response

    def post(self, path, body):
        response = requests.post(self.base_url + path, json=body, timeout=30)
        return response

    def wait_for_health(self, timeout):
        deadline = time.time() + timeout
        while time.time() < deadline:
            try:
                response = self.get("/health")
                if response.status_code == 200:
                    return True
            except requests.RequestException:
                pass
            time.sleep(0.5)
        return False

    def health(self):
        response = self.get("/health")
        return response.json()

    def get_object(self, name):
        response = self.get("/object?name=" + name)
        return response

    def get_text(self, name):
        response = self.get("/text?name=" + name)
        body = response.json()
        return body.get("text")

    def click(self, name):
        response = self.post("/click", {"name": name})
        return response

    def wait_for_object(self, name, timeout):
        """이름이 name 인 오브젝트가 active 상태가 될 때까지 기다립니다."""
        deadline = time.time() + timeout
        while time.time() < deadline:
            response = self.get_object(name)
            if response.status_code == 200:
                body = response.json()
                if body.get("active"):
                    return body
            time.sleep(0.3)
        return None

    def scene(self):
        response = self.get("/scene")
        return response.json()

    def wait_for_scene(self, scene_name, timeout):
        deadline = time.time() + timeout
        while time.time() < deadline:
            current = self.scene()
            if current.get("scene") == scene_name:
                return current
            time.sleep(0.3)
        return None

    def count(self, tag):
        response = self.get("/count?tag=" + tag)
        body = response.json()
        return body.get("count")

    def cheat(self, name, value=None):
        body = {"name": name}
        if value is not None:
            body["value"] = str(value)
        response = self.post("/cheat", body)
        return response

    def field(self):
        response = self.get("/field")
        return response

    def inventory(self):
        response = self.get("/inventory")
        return response.json()

    def last_drop(self):
        response = self.get("/last_drop")
        return response.json()

    def screenshot(self, path):
        try:
            response = self.post("/screenshot", {"path": path})
            return response.status_code == 200
        except requests.RequestException:
            return False
