# Git Workflow Preferences

## Commit Strategy
- **Always commit staged changes only** — do NOT commit all changes with `-a` flag
- **Cancel commits if no staged changes exist** — verify there are staged changes before proceeding
- This ensures deliberate, controlled commits rather than bulk commits of unreviewed changes
- **Do not commit changes unless explicitly asked** — do not stage or commit changes unless the user explicitly requests it.
- **Always split commits logically** — when instructed to commit, split the changes into small, logical, self-contained commits rather than committing everything in one go.
- **Keep credentials clean** — if committing files containing sensitive fields (like Token, WebhookKey, etc.), temporarily replace them with dummy values before staging/committing, and restore the actual values afterward.
