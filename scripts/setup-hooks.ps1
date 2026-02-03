# Setup repository to use the bundled .githooks folder for hooks
# Run: pwsh scripts\setup-hooks.ps1

Write-Output 'Setting core.hooksPath to .githooks (local repo config)'
& git config core.hooksPath .githooks
if ($LASTEXITCODE -ne 0) { Write-Error 'Failed to set core.hooksPath.'; exit 1 }

Write-Output 'Making pre-push hook executable if possible.'
$hook = Join-Path (Get-Location) '.githooks\pre-push'
if (Test-Path $hook) {
    try { & icacls $hook /grant "Users:(RX)" > $null 2>&1 } catch {}
}

Write-Output 'Done. Hooks will run on git operations (pre-push). To enable retroactive rewrite run:'
Write-Output "  pwsh scripts\ensure_dated_commits.ps1 -Retroactive"
Write-Output 'Or to run non-interactively (auto apply + push):'
Write-Output "  pwsh scripts\ensure_dated_commits.ps1 -Retroactive -AutoYes"
