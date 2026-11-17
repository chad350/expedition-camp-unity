# 검증기록 · Phase B

## 1. 검증 환경과 방법

| 항목 | 내용 |
|---|---|
| 일자 | 2026-11-17 |
| 장비 · Unity | Apple M1 Max · macOS 26.6.2 · Unity 6000.3.20f1 |
| 브랜치 생성 | `python3 tools/rebuild_break_branches.py --only 01,03,04,06,11,13` (6개 42초) |
| 증상 확인 | 브랜치마다 임시 PlayMode 테스트(`expedition-camp-unity_강사용/verify_temp/BreakChecks/`)를 복사해서 batchmode 로 실행하고, 실행 후 작업 폴더를 되돌렸습니다.<br>임시 테스트는 브랜치에 커밋하지 않았습니다. |
| 대조 확인 | 같은 6개 테스트를 main 에서 실행해서 증상이 나타나지 않는지 확인했습니다. |
| 실행 스크립트 | `expedition-camp-unity_강사용/verify_breaks.py` (시스템 python3 로 실행) |

## 2. 수업용 6개 확인 결과

| 브랜치 | 확인 항목 | 결과 | main 대조 |
|---|---|---|---|
| break/01-meta | Lobby 를 Play 했을 때 NPC_Shop 의 Sprite 가 비어 있는지 | 재현 · NPC_Shop Sprite 없음, NPC_Dex 는 정상<br>프로젝트를 열 때 meta 경고 1건 | 증상 없음 |
| break/03-null | 1-1 스테이지 웨이브 3개를 끝냈을 때 결과 창이 뜨는지 | 재현 · 스테이지 종료 처리 후 결과 창 비활성<br>`NullReferenceException` (`FieldManager.cs:243`) 1건 | 증상 없음 |
| break/04-override | 1웨이브 게 몬스터 3마리의 2초 이동 거리 | 재현 · `Monster_Crab_Bridge(Clone)` Move Speed 0 · 이동 0.00<br>다른 게 몬스터 2마리 Move Speed 1.8 · 이동 2.30 · 2.59 | 증상 없음 |
| break/06-tag | 자동 전투(무적 · 배속 5)로 1-1 스테이지를 진행했을 때 끝나는지 | 재현 · 4.4초 만에 3웨이브 도달, 이후 15초 동안 끝나지 않음<br>남은 몬스터 `Monster_Rabbit(Clone)` 2마리 · Tag Untagged · Tag Monster 개수 0 | 증상 없음 |
| break/11-eventsystem | Lobby 에 EventSystem 이 있는지 | 재현 · EventSystem 없음 · Btn_Bag 은 있음 | 증상 없음 |
| break/13-spawnkey | 1-1 스테이지를 치트로 15초 진행했을 때 웨이브 | 재현 · 웨이브 2 에서 멈춤<br>`[Field] spawn point not found: Spawn_03` 경고 1건 | 증상 없음 |

## 3. Phase B 진행 중 main 에 반영한 수정

| 커밋 | 내용 | 이유 |
|---|---|---|
| 6c83776 | `tools/rebuild_break_branches.py` · `tools/break/BreakApplier.cs` 추가 | 지시서 2장 (main 을 고치면 break 브랜치를 다시 만드는 스크립트) |
| 5e67297 | Player · Monster_Base 의 Character Controller Min Move Distance 를 0 으로 변경<br>장애물에 걸려 움직이지 못하면 0.8초 동안 75도 돌아가는 이동 추가 (`Scripts/Field/Steering.cs`)<br>자동 전투만으로 1스테이지를 끝내는 PlayMode 테스트 추가 | break/04 · 06 확인 중 몬스터가 거의 움직이지 않는 문제를 찾았습니다.<br>프레임이 매우 짧으면 프레임당 이동 거리가 Min Move Distance(0.001)보다 작아 이동이 무시됐습니다.<br>바위를 사이에 두고 플레이어와 몬스터가 서로를 향해 바위를 밀면서 둘 다 멈추는 경우도 있었습니다. |

5e67297 적용 후 1 ~ 10스테이지를 자동 전투(무적 · 배속 5)로 실행해서 10개 모두 끝나는 것을 확인했습니다.
PlayMode 테스트는 5건 모두 통과했고, Seed 1234 드롭 결과는 바뀌지 않았습니다.
15챕터 전환 세션은 이 수정이 들어간 main 에서 시작해야 합니다.
