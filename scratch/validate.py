import sys

try:
    with open(r"F:\Github Projects\Acoustican\src\Acoustican.Web\wwwroot\style.css", "r", encoding="utf-8") as f:
        content = f.read()
except Exception as e:
    print(f"Error reading file: {e}")
    sys.exit(1)

braces = []
in_comment = False
in_string = False
string_char = None

for i, char in enumerate(content):
    if in_comment:
        if char == '/' and content[i-1] == '*':
            in_comment = False
        continue
    if char == '*' and content[i-1] == '/':
        in_comment = True
        continue
        
    if in_string:
        if char == string_char and content[i-1] != '\\':
            in_string = False
        continue
    if char in ('"', "'"):
        in_string = True
        string_char = char
        continue
        
    if char == '{':
        braces.append((char, i))
    elif char == '}':
        if not braces:
            print(f"Error: Unmatched closing brace '}}' at index {i}")
        else:
            braces.pop()

if braces:
    print(f"Error: Unmatched opening braces at:")
    for brace, idx in braces:
        # get line number
        line_num = content[:idx].count('\n') + 1
        col_num = idx - content[:idx].rfind('\n')
        snippet = content[idx:idx+40].replace('\n', ' ')
        print(f"  Line {line_num}, Col {col_num}: '{snippet}...'")
else:
    print("CSS is balanced and has no unmatched braces!")
