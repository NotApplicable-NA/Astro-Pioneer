Get-ChildItem -Path 'c:\Nabil\Projects\Farming Sim 3D\Astro-Pioneer\Assets\Scripts' -Filter '*.cs' -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    # Matches "if (condition)" followed by optional whitespace/newlines and "}"
    # This is a syntax error in C# if there was no body between the condition and the brace
    if ($content -match 'if\s*\([^)]*\)\s*[\r\n]+\s*\}') {
        Write-Host "BROKEN IF DETECTED: $($_.FullName)"
    }
}
