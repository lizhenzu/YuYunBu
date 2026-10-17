# 사용법: powershell -File Prayer365_ToSql.ps1 -InputFile 입력.txt
# 입력 텍스트(UTF-8)를 dbo.Prayer365 MERGE SQL(UTF-8 BOM 없음, CRLF)로 바꾼다.
# 항목은 "=====" 한 줄로 구분한다. 형식은 README.txt 참고.
param(
    [Parameter(Mandatory = $true)][string]$InputFile,
    [string]$OutDir = (Join-Path $PSScriptRoot '..\..\SQL')
)
$ErrorActionPreference = 'Stop'
$crlf = "`r`n"
function Q([string]$s) { "N'" + $s.Replace("'", "''") + "'" }
function JsonStr([string]$s) { '"' + $s.Replace('\', '\\').Replace('"', '\"') + '"' }

$raw = [IO.File]::ReadAllText((Resolve-Path $InputFile), [Text.Encoding]::UTF8)
$blocks = [regex]::Split($raw, '(?m)^\s*=====\s*$') | Where-Object { $_.Trim() -ne '' }
$items = @()
foreach ($b in $blocks) {
    $md = $null; $title = ''; $vref = ''; $vtext = ''; $fixes = @()
    $body = New-Object Collections.Generic.List[string]
    $para = New-Object Collections.Generic.List[string]
    $inBody = $false
    foreach ($line in ($b -split "\r?\n")) {
        if ($inBody) {
            if ($line.Trim() -eq '') {
                if ($para.Count -gt 0) { $body.Add(($para -join ' ')); $para.Clear() }
            } else { $para.Add($line.Trim()) }
            continue
        }
        if ($line -match '^\s*MD\s*:\s*(\d{4})\s*$') { $md = $Matches[1] }
        elseif ($line -match '^\s*TITLE\s*:\s*(.*)$') { $title = $Matches[1].Trim() }
        elseif ($line -match '^\s*VERSE_REF\s*:\s*(.*)$') { $vref = $Matches[1].Trim() }
        elseif ($line -match '^\s*VERSE\s*:\s*(.*)$') { $vtext = $Matches[1].Trim() }
        elseif ($line -match '^\s*FIX\s*:\s*(.*?)\s*=>\s*(.*)$') { $fixes += ,@($Matches[1], $Matches[2]) }
        elseif ($line -match '^\s*BODY\s*:\s*$') { $inBody = $true }
    }
    if ($para.Count -gt 0) { $body.Add(($para -join ' ')) }
    if (-not $md) { throw 'MD(월일 4자리)가 없는 항목이 있습니다.' }
    $m = [int]$md.Substring(0, 2); $d = [int]$md.Substring(2, 2)
    if ($m -lt 1 -or $m -gt 12 -or $d -lt 1 -or $d -gt [DateTime]::DaysInMonth(2024, $m)) { throw "잘못된 날짜: $md" }
    if ($title -eq '' -or $body.Count -eq 0) { throw "$md : TITLE 또는 BODY가 비었습니다." }
    $items += [pscustomobject]@{ MD = $md; Title = $title; VRef = $vref; VText = $vtext; Fixes = $fixes; Body = ($body -join ($crlf + $crlf)) }
}
if ($items.Count -eq 0) { throw '항목이 없습니다.' }
$dup = $items | Group-Object MD | Where-Object Count -gt 1
if ($dup) { throw ('같은 날짜가 두 번 있습니다: ' + ($dup.Name -join ', ')) }

$sb = New-Object Text.StringBuilder
[void]$sb.Append('SET XACT_ABORT ON;' + $crlf + 'BEGIN TRAN;' + $crlf + $crlf)
foreach ($it in $items) {
    $fx = '[' + (($it.Fixes | ForEach-Object { '{"From":' + (JsonStr $_[0]) + ',"To":' + (JsonStr $_[1]) + '}' }) -join ',') + ']'
    $s = @"
-- $($it.MD)
MERGE dbo.Prayer365 AS t
USING (SELECT '$($it.MD)' AS MonthDay) AS s ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title=$(Q $it.Title), VerseRef=$(Q $it.VRef), VerseText=$(Q $it.VText), Body=$(Q $it.Body), Fixes=$(Q $fx), IsReviewed=1,
     UpdatedAt=DATEADD(HOUR,9,SYSUTCDATETIME()), UpdatedBy=N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay,Title,VerseRef,VerseText,Body,Fixes,IsReviewed,UpdatedAt,UpdatedBy)
     VALUES ('$($it.MD)',$(Q $it.Title),$(Q $it.VRef),$(Q $it.VText),$(Q $it.Body),$(Q $fx),1,DATEADD(HOUR,9,SYSUTCDATETIME()),N'Claude');
"@
    [void]$sb.Append(($s -replace "\r?\n", $crlf) + $crlf + $crlf)
}
[void]$sb.Append('COMMIT;' + $crlf + $crlf + 'SELECT MonthDay, Title, IsReviewed FROM dbo.Prayer365 ORDER BY MonthDay;' + $crlf)

$first = ($items | Select-Object -First 1).MD; $last = ($items | Select-Object -Last 1).MD
$name = "Prayer365_$first-$last" + '_' + (Get-Date).ToUniversalTime().AddHours(9).ToString('yyyyMMdd') + '.sql'
$path = Join-Path (Resolve-Path $OutDir) $name
[IO.File]::WriteAllText($path, $sb.ToString(), (New-Object Text.UTF8Encoding($false)))
$bytes = [IO.File]::ReadAllBytes($path)
if ($bytes.Length -eq 0) { throw '파일이 0바이트입니다.' }
if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { throw 'BOM이 들어갔습니다.' }
"$($items.Count)편 -> $path (BOM 없음, $($bytes.Length) bytes)"
'SSMS 파일 -> 열기로 실행해 주세요.'
