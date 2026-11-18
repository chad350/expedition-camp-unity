# 검증기록 · Phase A

2026-11-18 Chad 결정으로 연습 씬 P1 ~ P6 과 `docs/연습씬_정답표.md` 를 저장소에서 지웠습니다.
아래 연습 씬 확인 내용은 기록으로만 남깁니다.

## 1. 검증 환경

| 항목 | 값 |
|---|---|
| 일자 | 2026-11-17 |
| 장비 | Apple M1 Max · macOS 26.6.2 |
| Unity | 6000.3.20f1 (`/Applications/Unity/Hub/Editor/6000.3.20f1/`) |
| 패키지 | URP 17.3.0 · Input System 1.19.0 · Test Framework 1.6.0 · uGUI 2.0.0 |
| 파이썬 | 강사용 폴더 `.venv` (openpyxl · pillow · requests) |
| 로그 위치 | 저장소 `logs/` (git 제외) |

## 2. 확인 결과

| 확인 항목 | 실행 방법 | 결과 | 스크린샷 경로 |
|---|---|---|---|
| 기획 원본 생성 | `expedition-camp-unity_강사용/build_design_data.py` | 15챕터 엑셀 5개에 열 · 행을 추가하고 Shop.xlsx · Field.xlsx 를 만들었습니다. | |
| 변환 도구 | `python3 tools/export_tables.py` | CSV 12개를 내보냈습니다.<br>monster 40 · skill 56 · item 61 · drop_group 169 · stage 30 · stage_wave 204 · level_exp 60 · const 18 · enum 37 · string 347 · shop 8 · field_object 58 | |
| 씬 · 프리팹 생성 | batchmode `-executeMethod ExpeditionCamp.EditorTools.ProjectBuilder.BuildAll` | 종료 코드 0, 10 ~ 14초<br>게임 씬 4개 · 연습 씬 6개 · 게임 프리팹 22개 · 연습 프리팹 4개 · 재질 18개 · 설정 파일 2개를 만들었습니다. | `scene_*_1920x1080.png` |
| 생성 스크립트 재실행 결과 | BuildAll 을 2번 실행하고 결과 비교 | 프리팹 · 재질 · 설정 파일은 바이트까지 같습니다.<br>씬 파일은 오브젝트 · 컴포넌트 · 값이 같고 Unity가 매번 새로 정하는 내부 번호(fileID)만 달라집니다.<br>그래서 다시 생성하면 git 에 씬 파일 변경이 표시됩니다. | |
| Library 첫 생성 | Library 없는 상태에서 첫 batchmode 실행 | 패키지 가져오기와 컴파일까지 약 55초 | |
| EditMode 테스트 | batchmode `-runTests -testPlatform EditMode` | 38건 전부 통과 · 12초<br>15챕터 테스트 32건 + 데이터 검사 5건 + 결과 보상 1건 | |
| PlayMode · Boot 에서 Lobby 이동 | batchmode `-runTests -testPlatform PlayMode` (`GameFlowTests.BootMovesToLobby`) | 통과 · 로비 진입 때 save.json 생성 확인 | |
| PlayMode · 1스테이지 웨이브 3개 · 결과 창 | `GameFlowTests.FirstStageRunsThreeWavesAndShowsResult` | 통과 · 웨이브 1 · 2 · 3 진행 후 Popup_Result 표시 | |
| PlayMode · 결과 획득 개수와 가방 개수 | `GameFlowTests.ResultItemCountsMatchBagAfterConfirm` | 통과 · 결과 창 확인 후 아이템 3종 개수 · 골드 · 클리어 · 다음 스테이지 해금이 세이브와 같음 | |
| PlayMode · Seed 1234 2번 실행 | `GameFlowTests.SameSeedGivesSameDrops` | 통과 · 드롭 판정 8건이 2번 모두 같음<br>300002:8958 · 300002:9467 · 300002:9487 · 300010:5207 · 300010:4173 · 300002:4645 · 300003:5854 · 300003:1254 | |
| 테스트용 빌드 | `-executeMethod ExpeditionCamp.EditorTools.BuildScript.BuildForTest -buildNumber 3` | 종료 코드 0 · 25 ~ 48초<br>`Builds/Test/ExpeditionCamp.app` · build_info.json qa_bridge true | |
| 출시 빌드 | `-executeMethod ExpeditionCamp.EditorTools.BuildScript.BuildForRelease -buildNumber 3` | 종료 코드 0 · 25 ~ 30초<br>build_info.json qa_bridge false<br>빌드가 끝나면 Project Settings 의 QA_BRIDGE 정의를 원래대로 되돌립니다. | |
| 스모크 1 · 빌드 실행 후 연결 대기 | `expedition-camp-unity_강사용/smoke_phaseA.py` (`-qaSeed 1234 -qaPort 7700`) | 통과 · `/health` scene Lobby | `smoke_01_lobby.png` |
| 스모크 2 · 로비 버튼 4개 | `GET /object?name=Btn_Bag` · Btn_Dex · Btn_Shop · Btn_Settings | 통과 · 4개 모두 200 · active true · interactable true | |
| 스모크 3 · 출정 게시판 · 1스테이지 | `POST /click` NPC_Board → `POST /click` Btn_Stage_401001 → `GET /scene` | 통과 · scene Field_01 · stage_id 401001 | `smoke_03_stage_select.png` · `smoke_03_field.png` |
| 스모크 4 · 필드 몬스터 수 | `GET /count?tag=Monster` | 통과 · 3 (field_object 401001 1웨이브 count 합계 3) | `smoke_04_wave1.png` |
| 스모크 5 · 몬스터 전부 처치 · 결과 창 | `POST /cheat {"name": "kill_all"}` 반복 → `GET /object?name=Popup_Result` | 통과 · 골드 +517 · 201010 x4 · 201002 x2 · 201003 x3<br>PlayMode Seed 1234 기록과 같음<br>`/last_drop` group 300003 · roll 1254 | `smoke_05_result.png` |
| 스모크 6 · 가방 조회 | `POST /click` Btn_ResultConfirm → `GET /inventory` | 통과 · 가방 201010 x4 · 201002 x2 · 201003 x3 · gold 517 | `smoke_06_bag.png` |
| 스모크 7 · Player.log | `~/Library/Logs/302lab/ExpeditionCamp/Player.log` 에서 Exception 검색 | 통과 · 0건 | |
| 출시 빌드 7700 포트 | `verify_builds_phaseA.py` 로 출시 빌드 실행 10초 후 127.0.0.1:7700 접속 | 통과 · 접속 거부<br>로비 진입 로그는 있고 `[QABridge]` · `[Cheat]` 로그 0건 | |
| 출시 빌드 F1 · F2 | 빌드 DLL 의 형식 이름 검사 (`strings ExpeditionCamp.Runtime.dll`) | 통과 · 출시 빌드에는 QABridge · CheatCommands · QATools 형식이 0건, 테스트용 빌드에는 있음<br>키 입력 확인은 4장 참고 | |
| 세이브 손상 | save.json 을 `{"gold": 100, "bag": [` 로 바꾸고 테스트용 빌드 실행 | 통과 · save.bak.json 생성 · 경고 로그 · 새 세이브 gold 0 · Lobby 진입 | |
| 데이터 오류 화면 | 테스트용 빌드 안의 string.csv 를 CP949 로 저장하고 실행 | 통과 · Boot 에 머물고 Popup_DataError 표시<br>시트 이름 string · 행 번호 2 · 열 이름 ko | `boot_data_error_cp949.png` |
| 연습 씬 6개 | 임시 EditMode · PlayMode 테스트 7건 (`expedition-camp-unity_강사용/verify_temp/`) | 7건 전부 통과<br>세부 결과는 `docs/연습씬_정답표.md` | `scene_P1_Editor_1920x1080.png` ~ `scene_P6_Data_1920x1080.png` · `scene_P5_Display_2340x1080.png` |
| P1 meta 실수 재현 | Unity를 끈 상태에서 `barrel.png` 를 `barrel_old.png` 로 바꾸고 batchmode 로 열기 | 재현 · 새 GUID meta 생성, P1 씬은 예전 GUID 참조 1건 유지 (Missing)<br>파일과 meta 를 되돌린 뒤 원래 GUID 로 복구 | |

스크린샷 경로는 모두 `docs/screenshots/phaseA/` 아래에 있습니다.
`scene_*.png` 는 에디터 카메라 렌더링이라, 게임 실행 중에만 바뀌는 값(체력바 · 연습 몬스터 범위 표시)은 시작 상태로 보입니다.

## 3. 작업 중 고친 문제

| 문제 | 원인 | 조치 |
|---|---|---|
| 드롭 아이템이 플레이어에게 모이지 않음 | 아이템 이동을 Update 에서 Rigidbody.MovePosition 으로 계산해서, 화면 프레임이 물리 프레임보다 많으면 거의 움직이지 않았습니다. | 이동 계산을 FixedUpdate 로 옮겼습니다. |
| 필드 색이 하얗게 보임 | 색 공간이 Gamma 였고 조명이 강했습니다. | 색 공간을 Linear 로 바꾸고 조명 · 주변광을 낮췄습니다. |
| 로비 NPC 가 배경과 구분되지 않음 | 키아트에 몬스터가 많이 그려져 있습니다. | 배경 Sprite Renderer 색을 어둡게(0.36) 하고 NPC 크기를 1.3배로 키웠습니다. |
| P6 NullReference 버튼에서 다른 에러 이름이 나옴 | 인스펙터 칸 None 은 에디터에서 UnassignedReferenceException 으로 표시됩니다. | 이름으로 찾은 결과가 없는 경우로 바꿔서 NullReferenceException 이 나오게 했습니다. |

## 4. Phase A 보고 뒤 반영한 결정 (2026-11-17)

| 결정 | 반영 내용 | 다시 확인한 결과 |
|---|---|---|
| 몬스터 프리팹 이름을 일러스트에 맞춤 | Monster_Slime 을 Monster_Crab 으로, Monster_Mushroom 을 Monster_Rabbit 으로 바꿨습니다.<br>git mv 로 옮겨서 프리팹 GUID 는 그대로입니다.<br>Field_01 배치 몬스터 이름은 Monster_Crab_Bridge 입니다.<br>monster 시트 `prefab_key` 도 같은 이름으로 바꿨습니다. | EditMode 38건 · PlayMode 4건 · 스모크 7단계 · 빌드 확인 3가지가 모두 통과했습니다.<br>Seed 1234 드롭 결과는 바뀌지 않았습니다. |
| 추가 데이터 행 유지 | const 3행 · 해독제 1행 · 문자열 2행을 그대로 둡니다. | |
| 가방 버튼 앵커 왼쪽 아래 유지 | Phase B 의 break/10 은 왼쪽 아래에서 가운데로 바꾸는 실수로 만듭니다. | |
| Noto Sans KR 라이선스 | Google Fonts 배포 파일의 `OFL.txt` 를 `Assets/ThirdParty/NotoSansKR/` 에 넣었습니다.<br>폰트 파일은 기존 OTF 를 그대로 사용합니다. | 압축 파일의 Regular TTF 와 저장소 OTF 의 글꼴 이름이 모두 Noto Sans KR · Regular 입니다. |

## 5. 이 맥에서 확인하지 못한 항목

| 항목 | 이유 | 확인 방법 |
|---|---|---|
| 테스트용 빌드에서 F1 · F2 키 입력 | macOS 가 터미널의 키 입력 전송을 허용하지 않았습니다. | 테스트용 빌드를 실행하고 F1 · F2 를 눌러 치트 패널 · 디버그 표시가 나오는지 확인합니다. |
| 출시 빌드에서 F1 · F2 키 입력 | 같은 이유입니다.<br>코드가 빌드에 없는 것은 DLL 검사로 확인했습니다. | 출시 빌드를 실행하고 F1 · F2 를 눌러 아무것도 나오지 않는지 확인합니다. |
| Windows 빌드 · Player.log 경로 | Windows 장비가 없습니다. | `%USERPROFILE%\AppData\LocalLow\302lab\ExpeditionCamp\Player.log` 가 생성되는지 확인합니다. |
| Windows 에서 QA 연결 컴포넌트 | HttpListener 의 127.0.0.1 권한은 15챕터에서도 미확인입니다. | 테스트용 빌드 실행 후 `http://127.0.0.1:7700/health` 를 조회합니다. |
