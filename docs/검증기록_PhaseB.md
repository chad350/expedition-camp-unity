# 검증기록 · Phase B

## 1. 검증 환경과 방법

| 항목 | 내용 |
|---|---|
| 일자 | 2026-11-17 |
| 장비 · Unity | Apple M1 Max · macOS 26.6.2 · Unity 6000.3.20f1 |
| 브랜치 생성 | `python3 tools/rebuild_break_branches.py` (16개 114초, 기준 커밋 4491869) |
| 증상 확인 | 브랜치마다 임시 PlayMode 테스트(`expedition-camp-unity_강사용/verify_temp/BreakChecks/`)를 복사해서 batchmode 로 실행하고, 실행 후 작업 폴더를 되돌렸습니다.<br>임시 테스트는 브랜치에 커밋하지 않았습니다. |
| 대조 확인 | 같은 6개 테스트를 main 에서 실행해서 증상이 나타나지 않는지 확인했습니다. |
| 실행 스크립트 | `expedition-camp-unity_강사용/verify_breaks.py` (시스템 python3 로 실행) |

## 2. 16개 확인 결과

16개 모두 기준 커밋 4491869 에서 만든 브랜치로 확인했습니다.
이 문서를 커밋한 뒤 같은 스크립트로 최신 main 에서 다시 만들었고, 브랜치마다 실수 커밋의 diff 가 확인한 브랜치와 같은 것을 비교했습니다.
같은 확인을 main 에서 실행한 결과 16개 모두 증상이 나타나지 않았습니다.

| 브랜치 | 확인 항목 | 결과 |
|---|---|---|
| break/01-meta | Lobby 를 Play 했을 때 NPC_Shop 의 Sprite | 재현 · NPC_Shop Sprite 없음, NPC_Dex 정상<br>프로젝트를 열 때 meta 경고 1건 |
| break/02-script | 1-1 스테이지 시작 3초 동안 몬스터 상태 | 재현 · 몬스터 1마리만 생성 · Monster 컴포넌트 없음 · 이동 0.00<br>Missing Script 경고 여러 건 · `NullReferenceException` (`FieldManager.cs:158`) 1건 |
| break/03-null | 웨이브 3개를 끝냈을 때 결과 창 | 재현 · 결과 창 비활성 · `NullReferenceException` (`FieldManager.cs:243`) 1건 |
| break/04-override | 1웨이브 게 몬스터 3마리의 2초 이동 거리 | 재현 · `Monster_Crab_Bridge(Clone)` Move Speed 0 · 이동 0.00<br>다른 2마리 이동 2.99 · 2.04 |
| break/05-apply | 자동 전투를 끄고 3초 동안 플레이어 체력 | 재현 · 몬스터 Attack Range 30 · 플레이어 체력 1000 → 772 · 몬스터가 처음 위치에 머묾 |
| break/06-tag | 자동 전투(무적 · 배속 5)로 1-1 스테이지 진행 | 재현 · 4.4초 만에 3웨이브 도달, 이후 15초 동안 끝나지 않음<br>토끼 몬스터 2마리 Tag Untagged · Tag Monster 개수 0 |
| break/07-layer | 처치 직후 드롭 아이템 Layer 와 획득 | 재현 · 드롭 아이템 6개 모두 Hidden Layer · 카메라가 그리지 않음 · 획득은 정상 |
| break/08-trigger | 드롭 아이템을 모은 뒤 위치와 획득 | 재현 · 아이템이 플레이어 앞 1.1 ~ 1.4 거리에서 멈춤 · 결과 골드 0 · 아이템 0종<br>`[Field] 획득하지 못한 드롭 아이템` 경고 3건 |
| break/09-sorting | Lobby 정렬 순서 | 재현 · Background 10 · NPC 5 · NPC 이름 6<br>화면 `docs/screenshots/phaseB/09_break_09-sorting_Lobby_1920x1080.png` |
| break/10-anchor | 1920 · 2340 너비에서 가방 · 도감 버튼 위치 | 재현 · 1920 에서 가방 40 ~ 220 · 도감 260 ~ 440<br>2340 에서 가방 250 ~ 430 으로 도감과 겹침<br>화면 `10_break_10-anchor_Lobby_2340x1080.png` |
| break/11-eventsystem | Lobby 의 EventSystem | 재현 · EventSystem 없음 |
| break/12-buildlist | 로비에서 401006 출정 | 재현 · 3초 뒤에도 Lobby 에 머물고 로딩 화면 유지<br>`Scene 'Field_02' couldn't be loaded ...` 에러 1건 |
| break/13-spawnkey | 1-1 스테이지를 치트로 15초 진행 | 재현 · 웨이브 2 에서 멈춤 · `[Field] spawn point not found: Spawn_03` 경고 1건 |
| break/14-csv-excel | Boot 실행 | 재현 · Boot 에 머묾 · 데이터 오류 창 "string · 2행 · ko" |
| break/15-config | Lobby 의 Time.timeScale | 재현 · 3 |
| break/16-material | Field_01 Rock_01 재질 | 재현 · Shader Standard · URP 패스 없음 · 분홍색<br>화면 `16_break_16-material_Field_01_1920x1080.png` |

main 대조 화면은 같은 폴더의 `*_main_*.png` 입니다.

## 3. break/02 확인 중 생긴 혼동

- 첫 시도에서 `Monster.cs` 와 `Monster.cs.meta` 를 함께 `MonsterUnit` 으로 옮겼습니다.
GUID 가 유지되어 증상이 없었습니다.
- 그 뒤 파일만 옮기는 방식과 클래스 이름까지 바꾸는 방식을 확인했을 때도 증상이 없었습니다.
첫 시도에서 Unity Library 가 `MonsterUnit.cs` 경로와 예전 GUID 를 기억했기 때문으로 판단합니다.
- 16개를 다시 만든 마지막 확인에서는 새 GUID 가 붙어 계획서 표와 같은 증상이 나타났습니다.
- 커밋 0f73dcf 의 메시지에 적은 "Unity 6000.3 이 예전 GUID 로 다시 연결한다"는 설명은 이 혼동에서 나온 판단이라 맞지 않습니다.
break/02 정의는 계획서대로 "파일 이름만 변경" 입니다.

## 4. Phase B 진행 중 main 에 반영한 수정

| 커밋 | 내용 | 이유 |
|---|---|---|
| 6c83776 | `tools/rebuild_break_branches.py` · `tools/break/BreakApplier.cs` 추가 | 지시서 2장 (main 을 고치면 break 브랜치를 다시 만드는 스크립트) |
| fdcf856 | 재생성 스크립트가 커밋하지 않은 새 파일(untracked)은 무시하도록 변경 | 확인 화면 폴더 때문에 재생성이 멈췄습니다. |
| 5e67297 | Player · Monster_Base 의 Character Controller Min Move Distance 를 0 으로 변경<br>장애물에 걸려 움직이지 못하면 0.8초 동안 75도 돌아가는 이동 추가 (`Scripts/Field/Steering.cs`)<br>자동 전투만으로 1스테이지를 끝내는 PlayMode 테스트 추가 | break/04 · 06 확인 중 몬스터가 거의 움직이지 않는 문제를 찾았습니다.<br>프레임이 매우 짧으면 프레임당 이동 거리가 Min Move Distance(0.001)보다 작아 이동이 무시됐습니다.<br>바위를 사이에 두고 플레이어와 몬스터가 서로를 향해 바위를 밀면서 둘 다 멈추는 경우도 있었습니다. |

5e67297 적용 후 1 ~ 10스테이지를 자동 전투(무적 · 배속 5)로 실행해서 10개 모두 끝나는 것을 확인했습니다.
PlayMode 테스트는 5건 모두 통과했고, Seed 1234 드롭 결과는 바뀌지 않았습니다.
15챕터 전환 세션은 이 수정이 들어간 main 에서 시작해야 합니다.
