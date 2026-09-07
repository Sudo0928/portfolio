---
description: "이모지와 컨벤셔널 커밋 메시지로 잘 포맷된 커밋을 생성합니다."
model: claude-haiku-4-5-20251001
allowed-tools: 
    {
        "Bash(git add:*)",
        "Bash(git status:*)",
        "Bash(git commit:*)",
        "Bash(git diff:*)",
        "Bash(git log:*)"
    }
---

# Claude 명령어: Commit
이모지와 컨벤셔널 커밋 메시지로 잘 포맷된 커밋을 생성합니다.

## 사용법

```
/commit
```

## 프로세스
1. 스테이지된 파일 확인, 스테이지된 파일이 있으면 해당 파일만 커밋
2. 없다면 사용자에게 자동으로 스테이지 해도 되는지 여부 확인.
3. 여러 논리적 변경사항에 대한 diff 분석
4. 필요시 분할 제안
5. 이모지 컨벤셔널 포맷으로 커밋 생성

## 커밋 포맷
`<이모지> <타입>: <설명>`

## 커밋 컨벤션
- ✨ Feat: “ ” ⇒ 새로운 기능 추가 시 **:sparkles:**
- 🙈 WIP: “ ” ⇒ 일단 작업중이던거 냅다 커밋할 때 **:see_no_evil:**
- ⚡️ Add: “ ” ⇒ 일반적인 추가 작업할 때 **:zap:**
- 🐛 Fix: “ ”  ⇒ 버그 수정시 **:bug:**
- 📝 Docs: “ ” ⇒ 주석 추가 시 **:memo:**
- 🎨 Style: “ ” ⇒ 줄간격이나 칸정렬 수정 시 **:art:**
- 🔨 Refactor: “ ” ⇒ 코드의 구조 / 형식 갈아엎을 때 **:hammer:**
- ✅ Test: “ ” ⇒ 테스트 코드 작성한거 커밋할 때 **:white_check_mark:**
- 👷 Chore: “ ” ⇒ 코드 기능 구현말고 관리작업(깃허브같은 것)할 때  **:construction_worker:**