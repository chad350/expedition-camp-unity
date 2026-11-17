# 검증기록 · 공통 UI 색 적용

## 1. 가져온 내용

참고: `/Users/chad/Codex/EliceQA/expedition-camp-unity-redesign` 의 `VisualRedesign.Style` · `SkinButton`

| 항목 | 리디자인 방식 | 이 저장소에 넣은 방식 |
|---|---|---|
| 적용 대상 | `Prefabs` · `Prefabs/Monsters` 전체 프리팹과 게임 씬 | `Prefabs/UI` 프리팹과 게임 씬 4개<br>몬스터 · 플레이어 프리팹과 연습 씬은 제외했습니다. |
| 글자 색 | 모든 글자를 크림색으로 바꿉니다. | 기본 글자 색(0.96, 0.96, 0.98)인 글자만 크림색으로 바꿉니다. |
| 패널 | `Panel_` 로 시작하면 Sprite 를 지우고 색을 바꿉니다. | Kenney Sprite 를 쓰는 이미지만 Sprite 를 지웁니다.<br>가방 · 도감 · 상점의 배경 그림은 그대로 둡니다. |
| 버튼 | 모든 버튼 | Kenney 버튼만 바꿉니다.<br>결과 창 확인 버튼은 결과 창 묶음의 금색을 유지합니다. |
| 프리팹 인스턴스 | 씬에서 오버라이드를 기록합니다. | 씬에서는 프리팹 인스턴스를 건드리지 않습니다.<br>프리팹을 고쳐서 씬에 반영합니다. |

## 2. 색 규칙

`Assets/Editor/UiTheme.cs` 에 모았습니다.
결과 창 생성 코드(`PrefabBuilder.ResultLayout`)도 같은 색을 씁니다.

| 대상 | 색 |
|---|---|
| 패널 · 상단 바 · HUD · 아이템 칸 · 상점 줄 · 도감 카드 안쪽 | Ink (0.065, 0.14, 0.14) |
| 버튼 기본 | Panel (0.105, 0.205, 0.20) |
| 버튼 마우스 올림 · 선택 | (0.19, 0.34, 0.31) |
| 버튼 누름 · 자동 전투 체크 | Gold (0.79, 0.62, 0.32) |
| 버튼 비활성 | (0.12, 0.17, 0.17, 0.55) |
| 팝업 뒤 어둡게 | (0.015, 0.035, 0.035, 0.88) |
| 글자 | Cream (0.96, 0.92, 0.80) |
| Boot 데이터 오류 창 | 붉은 색 유지, Sprite 만 지움 |

리디자인처럼 패널과 버튼을 같은 Panel 색으로 하면 설정 · 출정 게시판 창에서 닫기 버튼이 패널과 구분되지 않았습니다.
그래서 패널은 Ink, 버튼은 Panel 로 나눴습니다.

## 3. 수정 내용

| 파일 | 변경 |
|---|---|
| `Assets/Editor/UiTheme.cs` | 색 규칙과 적용 함수를 추가했습니다. |
| `Assets/Editor/PrefabBuilder.cs` | `BuildAll` 에서 UI 프리팹을 만든 뒤 `UiTheme.ApplyToUiPrefabs` 를 실행합니다. |
| `Assets/Editor/GameSceneBuilder.cs` | 게임 씬을 저장하기 전에 `UiTheme.ApplyToScene` 을 실행합니다. |
| `Prefabs/UI` 9개 | 결과 창을 뺀 UI 프리팹에 적용했습니다.<br>오브젝트 추가 · 삭제 없이 색 · Sprite · 버튼 색 값만 바뀌었습니다. |
| `Boot` · `Lobby` · `Field_01` · `Field_02` | 씬에 직접 있는 UI 에 적용했습니다.<br>오브젝트 추가 · 삭제 없이 값만 바뀌었습니다. |
| `Assets/Tests/EditMode/UiThemeTests.cs` | UI 프리팹과 게임 씬에 Kenney Sprite 가 남아 있으면 실패하는 검사 5건을 추가했습니다. |

## 4. 확인 결과

| 확인 | 결과 |
|---|---|
| EditMode 전체 | 46개 통과 |
| PlayMode 전체 | 5개 통과 |
| QA 데이터 · 프로젝트 파일 검사 | 강사용 `qa_complete` 로 67개 통과<br>저장소의 `qa/` 는 수강생 초안이라 4개가 `NotImplementedError` 로 멈춥니다. |
| 화면 | `docs/screenshots/theme/before.png` · `after.png`<br>로비 · 가방 · 도감 · 상점 · 출정 게시판 · 설정 · 필드 HUD 7장을 한 장에 모았습니다. |

## 5. 확인하지 못한 것

| 항목 | 내용 |
|---|---|
| 스모크 테스트 | 테스트용 빌드를 만들지 않아 `test_smoke.py` 는 실행하지 않았습니다. |
| 마우스 올림 · 누름 색 | batchmode 화면 캡처로는 확인할 수 없습니다. |
| 연습 씬 | 적용 대상에서 뺐으므로 Kenney 버튼 그대로입니다. |
