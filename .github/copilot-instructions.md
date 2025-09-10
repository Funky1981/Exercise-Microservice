## Built-in Command Shortcuts (minimal)

This file contains a short, minimal set of MCP shortcuts that are most useful for everyday development. Keep a small toolset in your Copilot/agent settings: Code management, Pull request management, Issues, and Repos.

### GitHub MCP (`/gh`) — keep these
- `/gh create pr` — Create a pull request
- `/gh list prs` — List open pull requests
- `/gh repos` — List your repositories
- `/gh issues` — List issues in a repository
- `/gh create issue` — Create a new issue

Always replace placeholders like `<repo>`, `<id>`, `<msg>` with your values.

### Filesystem MCP (`/fs`) — essential shortcuts
Use these to let the agent manage workspace files and directories. Paths and examples use Windows absolute paths.

- `/fs create_dir <absolute-path>` — Create directories recursively.
- `/fs create_file <absolute-path>` — Create a new file with content supplied after the command.
- `/fs read_file <absolute-path> <startLine> <endLine>` — Read lines from a file (1-based).
- `/fs list_dir <absolute-path>` — List folder contents.
- `/fs file_search <glob-pattern>` — Find files by glob (e.g. `**\*.csproj`).
- `/fs apply_patch` — Apply a multi-file patch when editing several files.
- `/fs insert_edit <file-path>` — Make a small inline edit to an existing file.

PowerShell / Windows notes:
- Use absolute Windows paths (drive letter + backslashes) for predictable results.
- When giving ranges to `read_file`, use 1-based line numbers.

Usage example:

`/fs create_dir C:\Projects\Exercise-Microservice\tmp\session-1`

---

If you want this trimmed further or to add back a specific category (workflows, notifications, Playwright), edit this file or say `expand: <category>` and an updated snippet will be provided.

**Always refer to this file for MCP usage and command shortcuts.**
PowerShell / Windows notes:
- Use absolute Windows paths (drive letter + backslashes) for predictable results.
- When giving ranges to `read_file`, use 1-based line numbers.

Usage example:

Type `/fs create_dir C:\Projects\Exercise-Microservice\tmp\session-1` to ask the agent to create a working directory.

---