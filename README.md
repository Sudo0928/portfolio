# 포트폴리오 — 염기용 (Yeom Giyong)

**Live: https://sudo0928-portfolio-sudo-s-projects3.vercel.app**

시스템 구조 설계에 강한 Unity 클라이언트 개발자의 포트폴리오 사이트입니다.

- 직접 만든 계층형 상태머신(HFSM)과 Behavior Tree — 상용 라이브러리 없이 구현
- 매니저·유저 데이터 구조 재설계와 클라이언트 예측/서버 보정 (ProjectRAID · GStar 2025 출품, 출품 빌드는 단일 플레이 전투 버전)
- 프로파일링 기반 성능 최적화 (Occlusion Culling·DLSS·메시 결합·TextureAtlas)
- 모든 서술은 실제 코드·커밋으로 확인된 사실만 담았습니다.
- 비공개 프로젝트의 본인 작성 코드는 `samples/` 폴더에 발췌해 두었습니다 — 네트워크 코어, 자작 행동트리(73개), 클라이언트 예측/서버 보정.

## 기술 스택

Next.js 16 · TypeScript · Tailwind CSS v4 · shadcn/ui · Vercel

구성과 내용은 직접 정하고, 구현은 Claude Code와 협업해 진행했습니다.

## 로컬 실행

```bash
npm install
npm run dev
```

## 문의

- GitHub: [Sudo0928](https://github.com/Sudo0928)
- Email: duarldyd0928@gmail.com
