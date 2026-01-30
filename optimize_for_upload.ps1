# ===================================
# 구글 드라이브 업로드 최적화 스크립트
# ===================================
# 안전하게 삭제 가능한 폴더들을 자동으로 제거합니다.
# Unity와 npm이 자동으로 재생성할 수 있는 폴더만 삭제합니다.

$projectPath = "c:\Users\user\Downloads\Rhythm_game (2)"

# 삭제할 폴더 목록
$foldersToDelete = @(
    "Library",
    "Temp",
    "Logs",
    "obj",
    "UserSettings",
    "Frontend\node_modules",
    "Frontend\dist",
    "Frontend\build",
    "Frontend\.vite",
    ".vs"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  구글 드라이브 업로드 최적화 스크립트" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 삭제 전 용량 확인
Write-Host "=== 삭제 대상 폴더 분석 중... ===" -ForegroundColor Yellow
Write-Host ""

$totalSize = 0
$existingFolders = @()

foreach ($folder in $foldersToDelete) {
    $fullPath = Join-Path $projectPath $folder
    if (Test-Path $fullPath) {
        try {
            $size = (Get-ChildItem $fullPath -Recurse -File -ErrorAction SilentlyContinue | 
                     Measure-Object -Property Length -Sum).Sum / 1MB
            $totalSize += $size
            $existingFolders += $folder
            Write-Host "  📁 $folder" -ForegroundColor White -NoNewline
            Write-Host " : $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
        } catch {
            Write-Host "  ⚠️  $folder : 크기 계산 실패" -ForegroundColor Gray
        }
    } else {
        Write-Host "  - $folder : 존재하지 않음" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "=== 요약 ===" -ForegroundColor Cyan
Write-Host "  삭제 가능한 총 용량: $([math]::Round($totalSize, 2)) MB" -ForegroundColor Green
Write-Host "  삭제 대상 폴더 수: $($existingFolders.Count)개" -ForegroundColor Green
Write-Host ""

if ($existingFolders.Count -eq 0) {
    Write-Host "✅ 삭제할 폴더가 없습니다. 이미 최적화되어 있습니다!" -ForegroundColor Green
    Write-Host ""
    Read-Host "엔터를 눌러 종료하세요"
    exit
}

# 사용자 확인
Write-Host "위 폴더들을 삭제하시겠습니까? (Y/N): " -ForegroundColor Cyan -NoNewline
$confirm = Read-Host

if ($confirm -eq 'Y' -or $confirm -eq 'y') {
    Write-Host ""
    Write-Host "=== 폴더 삭제 중... ===" -ForegroundColor Yellow
    Write-Host ""
    
    $deletedCount = 0
    $deletedSize = 0
    
    foreach ($folder in $existingFolders) {
        $fullPath = Join-Path $projectPath $folder
        try {
            $size = (Get-ChildItem $fullPath -Recurse -File -ErrorAction SilentlyContinue | 
                     Measure-Object -Property Length -Sum).Sum / 1MB
            
            Remove-Item -Path $fullPath -Recurse -Force -ErrorAction Stop
            Write-Host "  ✓ 삭제 완료: $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
            $deletedCount++
            $deletedSize += $size
        } catch {
            Write-Host "  ✗ 삭제 실패: $folder - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  ✅ 최적화 완료!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  삭제된 폴더: $deletedCount 개" -ForegroundColor White
    Write-Host "  절감된 용량: $([math]::Round($deletedSize, 2)) MB" -ForegroundColor White
    Write-Host ""
    Write-Host "📦 이제 구글 드라이브에 업로드할 수 있습니다!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📝 복원 방법:" -ForegroundColor Yellow
    Write-Host "  1. Unity: Unity Hub에서 프로젝트 열기 (자동 재생성)" -ForegroundColor White
    Write-Host "  2. Frontend: cd Frontend && npm install" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ 취소되었습니다." -ForegroundColor Red
    Write-Host ""
}

Read-Host "엔터를 눌러 종료하세요"
