# Bed War — 복셀 Chunk 메시 생성 (발췌)

## 어떤 게임인가

블록을 부수고 쌓는 마인크래프트류 게임에 침대를 지키는 규칙을 얹은 **멀티플레이 게임**입니다. Photon PUN2로 방을 만들어 함께 접속하고, 블록을 설치하거나 파괴한 결과가 다른 플레이어에게도 반영됩니다.

이 폴더에는 제가 작성한 스크립트만 발췌해 두었습니다. Photon과 JsonDotNet, 그리고 프로젝트에 포함된 모델·텍스처 리소스는 포함하지 않았습니다.

## 왜 만들었나 — 개별 오브젝트로는 감당이 되지 않았습니다

처음에는 블록 하나를 게임 오브젝트 하나로 처리하였습니다. 눈에 보이는 대로 만든 방식이었지만 블록이 조금만 늘어나도 성능이 크게 떨어졌습니다. 그래서 Chunk 개념을 도입하여 블록을 묶어서 다루도록 바꾸었습니다.

## 읽는 순서

### 1. `Game/Block.cs` — 보이지 않는 면은 만들지 않습니다

가장 핵심입니다. `Draw()`는 블록의 여섯 방향 각각에 대해 이웃 블록이 있는지를 먼저 확인하고, 이웃이 없을 때만 그 방향의 면을 만듭니다.

```csharp
public void Draw()
{
    if (bType == BlockType.AIR) return;

    if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z + 1))
        CreateQuad(Cubeside.FRONT);
    if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z - 1))
        CreateQuad(Cubeside.BACK);
    // ... TOP / BOTTOM / LEFT / RIGHT
}
```

땅속에 묻힌 블록은 여섯 면이 모두 가려지므로 아무것도 만들어지지 않습니다.

`CreateQuad()`는 면 하나를 정점 4개와 삼각형 2개로 직접 만들고, `blockUVs[(int)bType, 0..3]`로 텍스처 아틀라스에서 해당 블록의 UV 영역을 지정합니다. 블록 종류가 달라도 UV만 바뀌기 때문에 **Material은 하나면 됩니다**.

### 2. `Game/Chunk.cs` — 만든 면을 하나로 합칩니다

`CombineQuads()`가 Chunk 안의 모든 자식 메시를 모아 `mesh.CombineMeshes(combine)`로 하나의 메시로 결합하고, 단일 `Material cubematerial`을 붙입니다. 면마다 드로우콜이 나가던 것이 Chunk 단위로 줄어듭니다. 결합한 메시는 그대로 `MeshCollider`에도 넣어 충돌 판정에 재사용합니다.

### 3. `Game/World.cs` · `Game/BlockCreate.cs` — 월드와 상호작용

`World`가 Chunk를 딕셔너리로 들고 있고, `BlockCreate`가 화면 중앙에서 레이를 쏘아 맞은 지점의 블록을 설치하거나 파괴합니다. 변경은 Photon RPC로 다른 플레이어에게 전달하고, 바뀐 Chunk만 다시 그립니다.

### 4. `Game/ChunkSave.cs` — 저장과 불러오기

바뀐 블록 정보를 파일로 저장하고 다시 불러옵니다.

## 고지

- **읽기용 발췌입니다.** Photon PUN2와 JsonDotNet은 서드파티라 소스를 포함하지 않았고 `using`으로 참조만 합니다. 이 폴더 단독으로는 컴파일되지 않습니다.
- 원본 폴더명이 `#Game` / `#Loddy` / `#Loding`이었으나, `#`이 들어간 경로는 링크가 깨지기 때문에 `Game` / `Lobby` / `Loading`으로 바꾸었습니다. 내용은 그대로입니다.
- 원격 저장소 없이 로컬에서만 작업한 프로젝트라 커밋 기록은 없습니다.

## 지금 다시 만든다면

**면 단위로 게임 오브젝트를 만들고 있습니다.** `CreateQuad()`가 면마다 `GameObject`를 만들어 붙였다가 `CombineQuads()`에서 합칩니다. 결합 후에는 자식 오브젝트가 필요 없으므로, 처음부터 정점과 삼각형 배열에 직접 채워 넣는 편이 생성 비용과 GC 부담이 훨씬 적습니다.

**Chunk 하나가 통째로 다시 그려집니다.** 블록 하나를 바꿔도 그 Chunk 전체를 다시 만들기 때문에, Chunk가 커질수록 한 번 바꿀 때의 비용이 커집니다. 바뀐 부분만 갱신하거나 갱신을 프레임에 나누어 처리하는 방법을 찾아보고 싶습니다.

**`World.chunks`가 문자열 키의 정적 딕셔너리입니다.** Chunk를 좌표 문자열로 찾는데, 전역 정적 상태라 테스트하기 어렵고 멀티플레이에서 상태를 나누기도 까다롭습니다. 좌표 구조체를 키로 쓰고 월드 인스턴스가 소유하도록 바꾸는 편이 낫습니다.
