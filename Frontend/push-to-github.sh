#!/bin/bash

# GitHub 푸시 스크립트
# 이미 git init과 git commit을 하셨으므로, 추가 변경사항만 커밋하고 푸시합니다.

# 1. 업데이트된 README.md 추가
git add README.md .gitignore

# 2. 변경사항 커밋
git commit -m "Update README for rhythm game website"

# 3. 브랜치를 main으로 설정
git branch -M main

# 4. GitHub 원격 저장소 추가
git remote add origin https://github.com/sohee9010/rhythm-game-website.git

# 5. GitHub에 푸시
git push -u origin main

echo "✅ GitHub 푸시 완료!"
