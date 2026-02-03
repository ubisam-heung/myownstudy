<#
Ensures commits authored by the local git user have messages that start with
YYYYMMDD-... 수정 (e.g. 20260203-메시지 수정).

Usage:
  pwsh scripts\ensure_dated_commits.ps1            # interactive; will operate on commits about to be pushed
  pwsh scripts\ensure_dated_commits.ps1 -AutoYes   # run non-interactive and push changes if required
  pwsh scripts\ensure_dated_commits.ps1 -Retroactive -AutoYes  # rewrite all branches/tags history for your commits (DANGEROUS)

Important: This script rewrites history using `git filter-branch` when changes
are necessary. Rewriting public history will require force pushes. Use with care.
#>

param (
    [switch]$Retroactive,
    [switch]$AutoYes,
    [switch]$OnPush
)

function Abort($msg) {
    Write-Error $msg
    exit 1
}

# get git user email
$email = (& git config user.email).Trim()
if (-not $email) {
    $name = (& git config user.name).Trim()
    if ($name) { Abort "git user.email not set (user.name=$name). Please set git user.email first." }
    else { Abort "git user.email not set. Configure git user.email before running." }
}

$date = (Get-Date).ToString('yyyyMMdd')
$regex = '^[0-9]{8}-.+수정$'

Write-Output "Running ensure-dated-commits for user: $email (date prefix: $date)"

if ($Retroactive) {
    Write-Output 'Retroactive mode: scanning all refs (branches and tags) for your commits to update.'
    $commits = & git rev-list --all --author="$email" 2>$null | Select-Object -Unique
    if (-not $commits) { Write-Output 'No commits found for your author across refs.'; exit 0 }
    $rangeDescription = "all refs"
} else {
    # find upstream for current branch
    try { $upstream = (& git rev-parse --abbrev-ref --symbolic-full-name @{u}) -replace '\\r','' } catch { $upstream = $null }
    if (-not $upstream) {
        Write-Output "No upstream found for current branch; using origin/main as base for push-range.";
        $upstream = 'origin/main'
    }
    Write-Output "Checking commits in range: $upstream..HEAD"
    $commits = & git rev-list $upstream..HEAD --author="$email" 2>$null | Select-Object -Unique
    $rangeDescription = "$upstream..HEAD"
}

# find commits that need message changes
$toFix = @()
foreach ($c in $commits) {
    $msg = (& git log -1 --format=%s $c) -replace '\\r',''
    if ($msg -notmatch $regex) { $toFix += $c }
}

if ($toFix.Count -eq 0) {
    Write-Output "No commits to rewrite in range ($rangeDescription)."
    exit 0
}

Write-Output "Found $($toFix.Count) commits authored by you that do not have the required message format ($regex)."

if (-not $AutoYes) {
    Write-Output "List of commits to change (oldest last):"
    & git log --pretty=format:"%h %an %ad %s" --date=short $($toFix -join ' ') | ForEach-Object { Write-Output "  $_" }
    $resp = Read-Host "Rewrite these commit messages to prepend date and append ' 수정'? This will rewrite history and require force-push. Proceed? (y/N)"
    if ($resp -ne 'y' -and $resp -ne 'Y') { Write-Output 'Aborted by user.'; exit 1 }
}

# Determine range for filter-branch
if ($Retroactive) {
    $filterRange = '-- --branches --tags'
} else {
    # oldest commit to change (chronological) -> determine lowest ancestor among toFix
    $oldest = (& git rev-list --reverse $upstream..HEAD --author="$email" 2>$null | Where-Object { $toFix -contains $_ } | Select-Object -First 1)
    if (-not $oldest) { Abort 'Could not determine oldest commit to fix.' }
    $filterRange = "$oldest^..HEAD"
}

# create temp shell script for msg-filter
$tmp = [System.IO.Path]::GetTempFileName() + '.sh'
$sh = @"
#!/bin/sh
email='$email'
date='$date'
msg=`cat`
commit_email=`git show -s --format='%ae' "$GIT_COMMIT"`
if [ "\$commit_email" = "\$email" ]; then
  if echo "\$msg" | grep -Eq '^[0-9]{8}-.+수정$'; then
    printf "%s" "\$msg"
  else
    # remove trailing newlines and then prefix
    printf "%s-%s 수정" "\$date" "\$msg"
  fi
else
  printf "%s" "\$msg"
fi
"@
Set-Content -Path $tmp -Value $sh -Encoding UTF8
# ensure executable if on *nix
try { & chmod +x $tmp } catch {}

Write-Output "Rewriting commit messages using git filter-branch on range: $filterRange"

# run filter-branch
$cmd = "git filter-branch -f --msg-filter 'sh $tmp' $filterRange"
Write-Output "Running: $cmd"
$filterResult = cmd /c $cmd
$exit = $LASTEXITCODE
if ($exit -ne 0) {
    Write-Error "git filter-branch failed with exit code $exit"
    Remove-Item $tmp -ErrorAction SilentlyContinue
    exit 1
}

# clean up refs left by filter-branch and remove tmp
Write-Output 'Cleaning up backup refs from filter-branch...'
& git for-each-ref --format='%(refname)' refs/original/ | ForEach-Object { & git update-ref -d $_ }
& git reflog expire --expire=now --all
& git gc --prune=now --aggressive
Remove-Item $tmp -ErrorAction SilentlyContinue

# push changes
if ($AutoYes -or $OnPush) {
    Write-Output 'Pushing rewritten history to origin (force-with-lease)...'
    & git push --force-with-lease
    if ($LASTEXITCODE -ne 0) {
        Write-Error 'Force-push failed. You may need to inspect repository manually.'
        exit 1
    }
    Write-Output 'Rewrite + push complete.'
    # return special code to indicate push was performed
    exit 2
} else {
    Write-Output 'Rewrite complete. Please inspect and push manually (force required).' 
    exit 0
}
