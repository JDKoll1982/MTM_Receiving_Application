#!/bin/bash

# Mermaid Diagram Processor - Bash Wrapper
# Provides Unix/Linux compatibility for the PowerShell Mermaid processing scripts

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PWSH_SCRIPT="$SCRIPT_DIR/MermaidProcessor.ps1"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
log_info() {
    echo -e "${CYAN}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if PowerShell is available
check_powershell() {
    if ! command -v pwsh &> /dev/null; then
        log_error "PowerShell (pwsh) is not installed"
        log_info "Install PowerShell: https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell"
        exit 1
    fi
}

# Display usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Mermaid Diagram Processor - Applies namespace prefixes to Mermaid diagrams in specification files

OPTIONS:
    -f, --file PATH          Path to markdown file (required)
    -a, --action ACTION      Action to perform: Parse, ApplyNamespaces, Validate, ProcessAll, Restore, RemoveDiagrams (default: ProcessAll)
    -b, --backup             Create backup before processing (default: true)
    -d, --dry-run            Preview changes without applying them
    -p, --preserve-headers   When removing diagrams, keep the workflow headers
    -h, --help               Display this help message

EXAMPLES:
    # Process all diagrams with backup
    $0 -f specs/001-workflow-consolidation/spec.md

    # Dry run to preview changes
    $0 -f specs/001-workflow-consolidation/spec.md -d

    # Only validate diagrams
    $0 -f specs/001-workflow-consolidation/spec.md -a Validate

    # Parse diagrams to see structure
    $0 -f specs/001-workflow-consolidation/spec.md -a Parse

    # Restore from backup
    $0 -f specs/001-workflow-consolidation/spec.md -a Restore

    # Remove all diagrams (keeps headers)
    $0 -f specs/001-workflow-consolidation/spec.md -a RemoveDiagrams -p

    # Remove all diagrams and headers
    $0 -f specs/001-workflow-consolidation/spec.md -a RemoveDiagrams

EOF
    exit 0
}

# Parse command line arguments
FILE_PATH=""
ACTION="ProcessAll"
BACKUP="true"
DRY_RUN=""
PRESERVE_HEADERS=""

while [[ $# -gt 0 ]]; do
    case $1 in
        -f|--file)
            FILE_PATH="$2"
            shift 2
            ;;
        -a|--action)
            ACTION="$2"
            shift 2
            ;;
        -b|--backup)
            BACKUP="true"
            shift
            ;;
        -d|--dry-run)
            DRY_RUN="-DryRun"
            shift
            ;;
        -p|--preserve-headers)
            PRESERVE_HEADERS="-PreserveHeaders"
            shift
            ;;
        -h|--help)
            usage
            ;;
        *)
            log_error "Unknown option: $1"
            usage
            ;;
    esac
done

# Validate required arguments
if [ -z "$FILE_PATH" ]; then
    log_error "File path is required"
    usage
fi

# Check PowerShell availability
check_powershell

# Build PowerShell command
PWSH_CMD="pwsh -NoProfile -ExecutionPolicy Bypass -File '$PWSH_SCRIPT'"
PWSH_CMD="$PWSH_CMD -FilePath '$FILE_PATH'"
PWSH_CMD="$PWSH_CMD -Action '$ACTION'"

if [ "$BACKUP" = "true" ]; then
    PWSH_CMD="$PWSH_CMD -BackupFirst"
fi

if [ -n "$DRY_RUN" ]; then
    PWSH_CMD="$PWSH_CMD $DRY_RUN"
fi

if [ -n "$PRESERVE_HEADERS" ]; then
    PWSH_CMD="$PWSH_CMD $PRESERVE_HEADERS"
fi

# Execute
log_info "Executing: $ACTION on $FILE_PATH"
eval "$PWSH_CMD"

if [ $? -eq 0 ]; then
    log_success "Operation completed successfully"
else
    log_error "Operation failed"
    exit 1
fi
