$content = [System.IO.File]::ReadAllText("F:\Github Projects\Acoustican\src\Acoustican.Web\wwwroot\style.css")
$braces = [System.Collections.Generic.List[int]]::new()
$in_comment = $false
$in_string = $false
$string_char = $null

for ($i = 0; $i -lt $content.Length; $i++) {
    $char = $content[$i]
    if ($in_comment) {
        if ($char -eq '/' -and $content[$i-1] -eq '*') {
            $in_comment = $false
        }
        continue
    }
    if ($char -eq '*' -and $content[$i-1] -eq '/') {
        $in_comment = $true
        continue
    }
    if ($in_string) {
        if ($char -eq $string_char -and $content[$i-1] -ne '\') {
            $in_string = $false
        }
        continue
    }
    if ($char -eq '"' -or $char -eq "'") {
        $in_string = $true
        $string_char = $char
        continue
    }
    if ($char -eq '{') {
        $braces.Add($i)
    } elseif ($char -eq '}') {
        if ($braces.Count -eq 0) {
            Write-Host "Error: Unmatched closing brace at index $i"
        } else {
            $braces.RemoveAt($braces.Count - 1)
        }
    }
}
if ($braces.Count -gt 0) {
    Write-Host "Error: Unmatched opening braces at:"
    foreach ($idx in $braces) {
        $sub = $content.Substring(0, $idx)
        $line = ($sub.Split("`n")).Count
        $len = [Math]::Min(40, $content.Length - $idx)
        $snippet = $content.Substring($idx, $len).Replace("`r", "").Replace("`n", " ")
        Write-Host "  Line $line : $snippet"
    }
} else {
    Write-Host "CSS is balanced!"
}
