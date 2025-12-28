import sys
import re
import subprocess
import os
from collections import defaultdict

def parse_build_log(log_content):
    # Regex to match MSBuild warning/error format
    # Example: C:\Path\File.cs(12,34): warning RCS1037: Message [C:\Path\Project.csproj]
    pattern = re.compile(r'^\s*(?P<file>.*)\((?P<line>\d+),(?P<col>\d+)\):\s+(?P<level>warning|error)\s+(?P<code>\w+):\s+(?P<msg>.*)\s+\[.*\]')
    
    issues = []
    for line in log_content.splitlines():
        match = pattern.match(line)
        if match:
            issues.append(match.groupdict())
    return issues

def generate_dot(issues):
    if not issues:
        return 'digraph G { label="No Issues Found"; "Good Job!" [shape=box, style=filled, fillcolor=green]; }'

    # Aggregate data
    # Structure: File -> Code -> Count
    file_issues = defaultdict(lambda: defaultdict(int))
    code_details = {} # Code -> Level/Msg (sample)

    for i in issues:
        filename = os.path.basename(i['file'])
        code = i['code']
        file_issues[filename][code] += 1
        code_details[code] = (i['level'], i['msg'])

    dot = ['digraph BuildIssues {']
    dot.append('  rankdir=LR;')
    dot.append('  node [shape=box, style=filled, fontname="Segoe UI"];')
    dot.append('  edge [fontname="Segoe UI", fontsize=10];')
    
    # Root node
    dot.append(f'  ROOT [label="Build Issues\\nTotal: {len(issues)}", fillcolor="lightblue", shape=doubleoctagon];')

    for filename, codes in file_issues.items():
        # File node
        total_file_issues = sum(codes.values())
        color = "lightyellow" if any(code_details[c][0] == 'warning' for c in codes) else "lightpink"
        # Check for errors
        if any(code_details[c][0] == 'error' for c in codes):
            color = "lightpink"
            
        file_id = f'"{filename}"'
        dot.append(f'  {file_id} [label="{filename}\\n({total_file_issues})", fillcolor="{color}"];')
        dot.append(f'  ROOT -> {file_id};')

        for code, count in codes.items():
            level, msg = code_details[code]
            code_id = f'"{filename}_{code}"'
            code_label = f'{code}\\n{count}'
            
            # Code node
            code_color = "orange" if level == 'warning' else "red"
            dot.append(f'  {code_id} [label="{code}", fillcolor="{code_color}", shape=ellipse];')
            dot.append(f'  {file_id} -> {code_id} [label="{count}"];')

    dot.append('}')
    return '\n'.join(dot)

def main():
    # 1. Run dotnet build or read from stdin/file
    log_content = ""
    
    # If a file argument is provided, read it
    if len(sys.argv) > 1 and os.path.exists(sys.argv[1]):
        print(f"Reading log from {sys.argv[1]}...", file=sys.stderr)
        with open(sys.argv[1], 'r', encoding='utf-8', errors='ignore') as f:
            log_content = f.read()
    else:
        # Otherwise run dotnet build
        print("Running 'dotnet build'...", file=sys.stderr)
        try:
            process = subprocess.run(
                ['dotnet', 'build', '-clp:NoSummary'], 
                capture_output=True, 
                text=True,
                encoding='utf-8'
            )
            log_content = process.stdout
            # Also print stderr if any
            if process.stderr:
                print(process.stderr, file=sys.stderr)
        except Exception as e:
            print(f"Error running build: {e}", file=sys.stderr)
            sys.exit(1)

    # 2. Parse
    issues = parse_build_log(log_content)
    print(f"Found {len(issues)} issues.", file=sys.stderr)

    # 3. Generate DOT
    dot_content = generate_dot(issues)
    
    # 4. Output
    print(dot_content)

if __name__ == '__main__':
    main()
