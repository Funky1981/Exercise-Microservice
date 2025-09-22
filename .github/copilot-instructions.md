## Built-in Command Shortcuts

### GitHub MCP (`/gh`)

- `/gh create pr` — Create a pull request
- `/gh list prs` — List open pull requests
- `/gh repos` — List your repositories
- `/gh issues` — List issues in a repository
- `/gh create issue` — Create a new issue
- `/gh close issue <id>` — Close an issue by ID
- `/gh branches` — List branches in a repository
- `/gh commit <msg>` — Commit changes with a message
- `/gh status` — Show repository status
- `/gh clone <repo>` — Clone a repository

### Upstash Context7 MCP (`/ctx7`)

- `/ctx7 semantic <query>` — Perform a semantic search
- `/ctx7 summarize <text>` — Summarize provided text
- `/ctx7 extract <entity>` — Extract entities from text
- `/ctx7 compare <a> <b>` — Compare semantic similarity

### Pieces MCP (`/pcs`)

- `/pcs context` — Get code context from Pieces
- `/pcs snippet <name>` — Retrieve a code snippet by name
- `/pcs save <name>` — Save current code as a snippet
- `/pcs search <query>` — Search for code snippets

### Playwright MCP (`/pw`)

- `/pw test <url>` — Run browser tests on a URL
- `/pw screenshot <url>` — Take a screenshot of a webpage
- `/pw crawl <url>` — Crawl a webpage for links
- `/pw automate <action>` — Automate a browser action

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

## Essential Tool Usage Rules

**ALWAYS use these tools for consistent, up-to-date assistance:**

### Memory & Context (`/pcs`)
- **ALWAYS** check `/pcs context` or `/pcs search` for conversation history and user preferences
- Use `/pcs save` to preserve important project decisions and learning moments
- Reference past conversations to maintain continuity and respect user's learning style

### Documentation (`/ctx7`)
- **ALWAYS** use `/ctx7` for current best practices and up-to-date library documentation
- Never assume knowledge about frameworks - always validate against current documentation
- Use for architecture patterns, coding standards, and implementation guidance

### File Management (`/fs`)
- **ALWAYS** use `/fs` tools for all file operations instead of built-in VS Code tools
- Use `/fs read_file` to examine code before making suggestions
- Use `/fs create_file` and `/fs insert_edit` for file modifications
- Use `/fs file_search` to find files across the project structure

**Rule: Context First, Action Second** - Always gather context using these tools before providing guidance or making changes.

---

**Always refer to this file for MCP usage and command shortcuts.**
PowerShell / Windows notes:

- Use absolute Windows paths (drive letter + backslashes) for predictable results.
- When giving ranges to `read_file`, use 1-based line numbers.

Usage example:

Type `/fs create_dir C:\Projects\Exercise-Microservice\tmp\session-1` to ask the agent to create a working directory.

---
