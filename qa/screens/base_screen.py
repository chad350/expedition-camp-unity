"""화면 객체의 공통 부분입니다."""


class BaseScreen:
    def __init__(self, client):
        self.client = client

    def is_visible(self, name):
        response = self.client.get_object(name)
        if response.status_code != 200:
            return False
        body = response.json()
        return body.get("active", False)

    def click(self, name):
        response = self.client.click(name)
        assert response.status_code == 200, "%s 클릭 실패 · %d · %s" % (name, response.status_code, response.text)
        return response.json()
