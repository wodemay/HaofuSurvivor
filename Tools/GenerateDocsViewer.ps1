param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$docsRoot = Join-Path $ProjectRoot 'Docs'
$temporaryRoot = Join-Path $docsRoot 'Temporary'
$output = Join-Path $docsRoot 'docs-data.js'
$contentRoot = Join-Path $docsRoot 'docs-content'
New-Item -ItemType Directory -Path $contentRoot -Force | Out-Null
$script:docIndex = 0
$items = Get-ChildItem -LiteralPath $docsRoot -Recurse -File -Filter *.md | Where-Object { $_.FullName -notlike "$temporaryRoot\*" } | Sort-Object FullName | ForEach-Object {
    $relative = $_.FullName.Substring($docsRoot.Length + 1).Replace('\','/')
    $markdown = [IO.File]::ReadAllText($_.FullName, [Text.Encoding]::UTF8)
    $titleMatch = [regex]::Match($markdown, '(?m)^#\s+(.+)$')
    $title = if ($titleMatch.Success) { $titleMatch.Groups[1].Value.Trim() } else { $relative }
    $file = 'doc-{0:D3}.js' -f $script:docIndex
    $script:docIndex++
    $content = $markdown | ConvertTo-Json -Compress
    [IO.File]::WriteAllText((Join-Path $contentRoot $file), "window.DOC_CONTENT = $content;", (New-Object Text.UTF8Encoding($false)))
    [ordered]@{ path=$relative; title=$title; file="docs-content/$file" }
}
$json = $items | ConvertTo-Json -Depth 4 -Compress
[IO.File]::WriteAllText($output, "window.DOCS_DATA = $json;", (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated $($items.Count) documents: $output"
