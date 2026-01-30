# 구글 드라이브 업로드 최적화 스크립트
# 안전하게 삭제 가능한 폴더들을 자동으로 제거합니다

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

Write-Host "========================================"
Write-Host "  구글 드라이브 업로드 최적화" 
Write-Host "========================================"
Write-Host ""

# 삭제 전 용량 확인
Write-Host "삭제 대상 폴더 분석 중..."
Write-Host ""

$totalSize = 0
$existingFolders = @()

foreach ($folder in $foldersToDelete) {
    $fullPath = Join-Path $projectPath $folder
    if (Test-Path $fullPath) {
        $size = (Get-ChildItem $fullPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        if ($size -gt 0) {
            $totalSize += $size
            $existingFolders += $folder
            Write-Host "  $folder : $([math]::Round($size, 2)) MB"
        }
    }
}

Write-Host ""
Write-Host "삭제 가능한 총 용량: $([math]::Round($totalSize, 2)) MB"
Write-Host "삭제 대상 폴더 수: $($existingFolders.Count)개"
Write-Host ""

if ($existingFolders.Count -eq 0) {
    Write-Host "삭제할 폴더가 없습니다. 이미 최적화되어 있습니다!"
    exit
}

# 사용자 확인
$confirm = Read-Host "위 폴더들을 삭제하시겠습니까? (Y/N)"

if ($confirm -eq 'Y' -or $confirm -eq 'y') {
    Write-Host ""
    Write-Host "폴더 삭제 중..."
    Write-Host ""
    
    $deletedCount = 0
    $deletedSize = 0
    
    foreach ($folder in $existingFolders) {
        $fullPath = Join-Path $projectPath $folder
        $size = (Get-ChildItem $fullPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        
        Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  삭제 완료: $folder ($([math]::Round($size, 2)) MB)"
        $deletedCount++
        $deletedSize += $size
    }
    
    Write-Host ""
    Write-Host "========================================"
    Write-Host "  최적화 완료!"
    Write-Host "========================================"
    Write-Host "  삭제된 폴더: $deletedCount 개"
    Write-Host "  절감된 용량: $([math]::Round($deletedSize, 2)) MB"
    Write-Host ""
    Write-Host "이제 구글 드라이브에 업로드할 수 있습니다!"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "취소되었습니다."
}
