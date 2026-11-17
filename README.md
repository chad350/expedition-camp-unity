# 원정대 캠프 (Expedition Camp)

## 1. 저장소 구성

| 경로 | 내용 |
|---|---|
| `UnityProject/` | Unity 프로젝트 (Unity 6000.3.20f1 · URP · Input System) |
| `design_data/` | 기획 원본 엑셀 7개 (Monster · Item · Stage · Common · String · Shop · Field) |
| `tools/export_tables.py` | 기획 원본 엑셀을 게임용 CSV 로 내보내는 변환 도구 |
| `docs/` | 값의 출처표 · 연습 씬 정답표 · 검증 기록 · 스크린샷 |
| `.gitignore` | Library · Temp · Logs · obj · Builds · UserSettings 를 제외합니다. |

## 2. 프로젝트 열기

- Unity Hub 에서 Add > Add project from disk 를 선택하고 `UnityProject` 폴더를 고릅니다.
- 버전 칸에 6000.3.20f1 이 표시되는지 확인하고 프로젝트를 엽니다.
- 첫 실행 때 Library 폴더를 만드는 동안 창을 닫지 않습니다.
- `Assets/Scenes/Boot.unity` 를 열고 Play 하면 Lobby 로 이동합니다.

게임 씬은 Boot · Lobby · Field_01 · Field_02 이고, 1단계 연습 씬 6개는 `Assets/Scenes/Practice/` 에 있습니다.
연습 씬은 빌드 씬 목록에 넣지 않았습니다.

## 3. 게임 진행

- 로비에서 출정 게시판 NPC 를 클릭하고 스테이지를 고릅니다.
- 필드에서는 WASD 로 이동하고, 가장 가까운 몬스터를 자동으로 공격합니다.
- 자동 전투를 켜면 플레이어가 가장 가까운 몬스터에게 이동합니다.
- 웨이브를 모두 끝내면 결과 창이 나오고, 확인 버튼을 누르면 보상을 저장하고 로비로 돌아갑니다.
- 1챕터 1 ~ 10스테이지만 플레이할 수 있습니다.

## 4. 브랜치

| 브랜치 | 내용 |
|---|---|
| `main` | 결함 X<br>모든 검사와 테스트를 통과하는 상태입니다. |
| `break/*` | 실수 재현 브랜치 (Phase B 에서 추가) |

## 5. 기획 원본을 CSV 로 내보내기

처음 1번만 openpyxl 을 설치합니다.

Windows

```
py -m pip install openpyxl
```

macOS

```
python3 -m pip install openpyxl
```

저장소 최상위 폴더에서 변환 도구를 실행합니다.

Windows

```
py tools\export_tables.py
```

macOS

```
python3 tools/export_tables.py
```

결과 CSV 는 `UnityProject/Assets/StreamingAssets/data/` 에 저장됩니다.
CSV 를 엑셀이나 메모장에서 직접 저장하지 않고, 엑셀 원본을 고친 뒤 변환 도구를 실행합니다.

## 6. QA 기능

| 기능 | 사용 방법 |
|---|---|
| 치트 패널 | F1 |
| 디버그 표시 | F2 |
| 고정 난수 | `Assets/Settings/QAConfig.asset` 의 Random Seed, 실행 인자 `-qaSeed 1234` (실행 인자가 우선) |
| QA 연결 컴포넌트 | `http://127.0.0.1:7700/` (실행 인자 `-qaPort` 로 변경) |

세 기능은 스크립팅 정의 `QA_BRIDGE` 가 있을 때만 컴파일됩니다.
에디터의 Project Settings > Player > Scripting Define Symbols 에는 `QA_BRIDGE` 가 들어 있습니다.

| QA 연결 요청 | 하는 일 |
|---|---|
| `GET /health` | 연결 확인 · 현재 씬 |
| `GET /hierarchy` · `GET /object?name=` · `GET /text?name=` | 화면 오브젝트 조회 (UI 와 Collider2D 가 있는 2D 오브젝트) |
| `POST /click` | 이름으로 클릭 |
| `POST /reset` | 세이브를 초기화하고 Lobby 를 불러옴 |
| `POST /screenshot` | 화면 저장 |
| `GET /scene` · `POST /scene` | 현재 씬 조회 · 스테이지 이동 (`{"stage_id": 401001}`, 잠긴 스테이지 409) |
| `GET /count?tag=Monster` | 활성 오브젝트 개수 |
| `POST /cheat` | 치트 실행 (`kill_all` · `add_gold` · `add_item` · `invincible` · `speed` · `reset_save`, 모르는 치트 404) |
| `GET /inventory` | 골드 · 레벨 · 가방 내용 |
| `GET /last_drop` | 마지막 드롭 판정 (group_id · roll · item_ids) |

## 7. 빌드 · 테스트 (명령줄)

Unity 실행 파일 경로는 아래와 같습니다.

Windows

```
"C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe"
```

macOS

```
/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity
```

| 작업 | 추가 인자 |
|---|---|
| 테스트용 빌드 (QA_BRIDGE 포함, `Builds/Test/`) | `-batchmode -nographics -projectPath UnityProject -executeMethod ExpeditionCamp.EditorTools.BuildScript.BuildForTest -quit` |
| 출시 빌드 (QA_BRIDGE 제외, `Builds/Release/`) | `-batchmode -nographics -projectPath UnityProject -executeMethod ExpeditionCamp.EditorTools.BuildScript.BuildForRelease -quit` |
| EditMode 테스트 | `-batchmode -nographics -projectPath UnityProject -runTests -testPlatform EditMode -testResults reports/editmode.xml` |
| PlayMode 테스트 | `-batchmode -projectPath UnityProject -runTests -testPlatform PlayMode -testResults reports/playmode.xml` |
| 씬 · 프리팹 다시 생성 | `-batchmode -nographics -projectPath UnityProject -executeMethod ExpeditionCamp.EditorTools.ProjectBuilder.BuildAll -quit` |

`-runTests` 와 `-quit` 을 함께 넣으면 테스트가 실행되지 않습니다.

## 8. 로그 · 세이브 위치

| 파일 | Windows | macOS |
|---|---|---|
| 빌드 로그 | `%USERPROFILE%\AppData\LocalLow\302lab\ExpeditionCamp\Player.log` | `~/Library/Logs/302lab/ExpeditionCamp/Player.log` |
| 세이브 | `%USERPROFILE%\AppData\LocalLow\302lab\ExpeditionCamp\save.json` | `~/Library/Application Support/302lab/ExpeditionCamp/save.json` |

세이브가 손상되면 같은 폴더에 `save.bak.json` 을 만들고 새 세이브로 시작합니다.

## 9. 리소스 출처

| 리소스 | 위치 | 출처 · 라이선스 |
|---|---|---|
| 몬스터 일러스트 40장 · 아이템 아이콘 60장 · 속성 아이콘 5장 · 키아트 · 화면 배경 2장 | `Assets/Resources/Icons/` · `Assets/Resources/UI/` | 15챕터 몬스터 도감 저장소에서 제작한 리소스 (자체 제작) |
| 해독제 아이콘 `item_202016.png` | `Assets/Resources/Icons/Item/` | `item_202001.png` 의 색을 바꾼 파일 |
| NPC 이미지 3장 | `Assets/Sprites/NPC/` | 몬스터 일러스트 mon_100009 · mon_100015 · mon_100030 복사본 |
| 플레이어 이미지 | `Assets/Resources/Icons/Monster/mon_100001.png` | 몬스터 일러스트 100001 (불꽃여우) |
| 그림자 · 원 · 연습 소품 이미지 | `Assets/Art/` · `Assets/Sprites/Props/` | 자체 제작 (파이썬으로 생성) |
| Kenney UI Pack 2.0 | `Assets/ThirdParty/Kenney/` | Kenney (www.kenney.nl) · CC0 · `License.txt` 포함 |
| Noto Sans KR | `Assets/Fonts/` | Google Fonts · SIL Open Font License 1.1 · `Assets/ThirdParty/NotoSansKR/OFL.txt` 포함 |
| TextMesh Pro 기본 리소스 | `Assets/TextMesh Pro/` | Unity uGUI 패키지 |
| 바닥 · 바위 · 나무 · 다리 | 씬 안 | Unity 기본 도형 + URP Lit 재질 |

## 10. QA 자동화

| 경로 | 내용 |
|---|---|
| `qa/tests/test_tables.py` | 데이터 검사 (CSV 읽기 · 타입 · 중복 · 범위 · 참조 · 원본 엑셀 비교) |
| `qa/tests/test_project_files.py` | 프로젝트 파일 검사 (meta 짝 · GUID 참조 · 빌드 씬 목록 · SpawnPoint 이름 · 프리팹 · 씬 · 설정 · 재질 규칙) |
| `qa/tests/test_artifact.py` · `qa/tests/test_smoke.py` | 빌드 산출물 검사 · 스모크 테스트 |
| `qa/rules/*.yaml` | 검사 기준 값 |
| `Jenkinsfile` | 0 준비 · 1 데이터 검사 · 2 프로젝트 파일 검사 |

실행 방법은 `docs/검증기록_3단계자동화.md` 6절에 있습니다.

