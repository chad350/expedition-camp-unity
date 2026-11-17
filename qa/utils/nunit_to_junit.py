"""Unity Test Framework 결과(NUnit 3 XML)를 Jenkins 가 읽는 JUnit XML 로 바꿉니다.

사용 방법 (qa 폴더에서 실행)
    python utils/nunit_to_junit.py ../reports/unity_editmode.xml ../reports/junit_editmode.xml
"""

import sys
import xml.etree.ElementTree as ET


def convert(nunit_path, junit_path, suite_name):
    root = ET.parse(nunit_path).getroot()
    suite = ET.Element("testsuite")
    suite.set("name", suite_name)
    total = 0
    failures = 0
    skipped = 0
    for case in root.iter("test-case"):
        total += 1
        testcase = ET.SubElement(suite, "testcase")
        testcase.set("classname", case.get("classname", suite_name))
        testcase.set("name", case.get("name", ""))
        testcase.set("time", case.get("duration", "0"))
        result = case.get("result", "")
        if result == "Failed":
            failures += 1
            failure = ET.SubElement(testcase, "failure")
            message = case.find("./failure/message")
            stack = case.find("./failure/stack-trace")
            if message is not None and message.text:
                failure.set("message", message.text.strip()[:500])
            if stack is not None and stack.text:
                failure.text = stack.text
        elif result == "Skipped":
            skipped += 1
            ET.SubElement(testcase, "skipped")
    suite.set("tests", str(total))
    suite.set("failures", str(failures))
    suite.set("skipped", str(skipped))
    ET.ElementTree(suite).write(junit_path, encoding="utf-8", xml_declaration=True)
    print("JUnit 변환: %s · 테스트 %d · 실패 %d" % (junit_path, total, failures))


if __name__ == "__main__":
    convert(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else "Unity")
