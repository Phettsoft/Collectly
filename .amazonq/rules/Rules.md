# Universal Project Rules and Development README

## Document Version
- Rule Document Version: 1.1.1
- Last Updated: 2026-04-20

## Purpose
This file defines the non-negotiable development rules for this project.

These rules exist to keep the codebase:
- modular
- maintainable
- scalable
- readable
- testable
- production-ready
- consistent across files and features
- recoverable when bad changes are introduced

These rules must be followed for all new features, bug fixes, refactors, and architecture changes.

---

## Core Non-Negotiable Rules

### 1. No pseudocode
All code must be real, complete, working code.
Do not leave placeholder logic for core functionality.
Do not replace real implementation with comments describing what should happen.

### 2. Always output complete updated files
Whenever a file is changed:
- output the full updated file
- include all imports
- include all functions
- include all classes
- include all related code needed for the file to run
- do not output partial snippets unless explicitly requested

### 3. Never allow any file to exceed 1000 lines
This is a hard rule.

If a file approaches or exceeds 1000 lines:
- split it into smaller modules
- create new files with clear responsibility boundaries
- update imports accordingly
- preserve behavior
- keep each file focused on a single concern or closely related concerns

Preferred target:
- most files should stay well below 1000 lines
- when practical, aim for 200 to 600 lines
- large files must be broken down before they become difficult to maintain

### 4. Preserve modular architecture
Do not pile unrelated logic into a single file.
Separate responsibilities clearly.

Examples:
- models go in model files
- API routes go in route files
- schemas go in schema files
- business logic goes in service files
- helpers/utilities go in utility files
- configuration goes in config files
- database access goes in repository/data-access files
- UI logic should be separated from data logic
- long classes should be split if they handle multiple concerns

### 5. Fix root causes, not symptoms
When debugging:
- identify the actual root cause
- do not apply shallow patches that hide deeper issues
- do not remove features just to silence errors
- do not bypass validation unless explicitly required
- do not suppress exceptions unless there is a proper logging and handling strategy

### 6. Preserve project consistency
Before making changes:
- inspect the current project structure
- inspect related files
- inspect imports and dependencies
- preserve naming conventions
- preserve architecture style
- preserve existing working behavior unless intentionally changing it
- keep schemas, interfaces, and contracts aligned

### 7. Keep code production-oriented
All code should be written as if it will be maintained and extended later.

Required:
- clear naming
- type hints where applicable
- docstrings where useful
- input validation
- error handling
- logging
- sensible separation of concerns
- no hidden magic behavior
- no fragile one-off hacks unless explicitly requested and clearly isolated

### 8. Do not create tight coupling unnecessarily
When adding features:
- avoid hard-wiring unrelated modules together
- prefer dependency injection or clean interfaces where reasonable
- keep components replaceable
- reduce ripple effects from future changes

### 9. Every feature must integrate cleanly
When adding or changing code:
- update dependent files too
- do not leave broken imports
- do not leave stale references
- do not leave unused routes, classes, or functions if they were replaced
- keep file relationships coherent

### 10. Refactors must preserve behavior unless explicitly changing it
When refactoring:
- improve structure without breaking functionality
- preserve business logic
- preserve external interfaces unless intentionally redesigning them
- update all affected files together
- ensure the project still runs

---

## File Size and Project Structure Rules

### 11. File size limit
Hard rule:
- no file may exceed 1000 lines

If a file gets too large, split it by responsibility, such as:
- `routes_user.py`, `routes_admin.py`
- `service_auth.py`, `service_billing.py`
- `schema_user.py`, `schema_project.py`
- `ui_dashboard.py`, `ui_settings.py`
- `scanner_core.py`, `scanner_filters.py`, `scanner_ranker.py`

### 12. Recommended file sizing
Preferred ranges:
- utility/helper files: under 300 lines
- schemas/models: under 400 lines
- service files: under 500 lines
- route/controller files: under 500 lines
- large orchestration files: under 700 lines
- absolute hard max: 1000 lines

### 13. Directory organization
Use clean, focused folders when appropriate, for example:
- `app/`
- `api/`
- `core/`
- `config/`
- `models/`
- `schemas/`
- `services/`
- `repositories/`
- `utils/`
- `tests/`
- `frontend/`
- `assets/`
- `scripts/`

Not every project needs every folder, but structure should stay intentional.

---

## Implementation Rules

### 14. Inspect before changing
Before writing code:
- inspect the current file tree
- inspect the affected files
- inspect related dependencies
- understand the current architecture
- identify the exact files that should be changed

Do not guess what exists.

### 15. Build in dependency-safe order
Implement features in a logical order.

Typical order:
1. configuration
2. models
3. schemas
4. repositories/data access
5. services/business logic
6. API/routes/UI bindings
7. tests
8. docs and setup updates

Do not jump into UI or routes before foundational layers exist unless the project specifically requires it.

### 16. Keep functions and classes focused
Do not create giant “god functions” or “god classes.”
If a function or class handles too many responsibilities:
- split it
- create helper modules
- extract reusable logic

### 17. Prefer explicit code over overly clever code
Write code that is easy to maintain.
Avoid unnecessary complexity.
Avoid cryptic naming.
Avoid squeezing too much into one expression when it hurts readability.

### 18. Keep configuration centralized
Do not hardcode values all over the project.
Use:
- config files
- environment variables
- constants modules
- settings objects

### 19. Keep shared logic reusable
If logic is reused in multiple places:
- extract it into a shared module
- avoid duplicating core business logic
- keep reusable utilities well named and well scoped

### 19A. Mandatory application version tracking
Every project must have a clearly defined application version source.

Required:
- maintain a centralized version value for the application
- do not hardcode different app version strings in multiple files
- the GUI must display the current application version
- the displayed application version must come from the centralized version source
- version information must be accessible to both backend logic and UI logic where applicable

Recommended examples:
- `app_version.py`
- `config/version.py`
- `frontend/src/version.ts`
- `shared/version.json`

The version source should include, where practical:
- app version
- release date
- build date
- optional compatibility notes

### 19B. GUI version display is required
If the project includes a GUI, dashboard, desktop UI, admin panel, or web frontend, it must visibly display version information.

Preferred placement:
- settings screen
- about dialog
- footer
- header status area
- admin/system info panel

Minimum requirement:
- display the application version in at least one stable visible location

Optional additional values when relevant:
- rules version
- config schema version
- database schema version
- API version

### 19C. Application changes must trigger application version updates
When the application changes in a way that affects code, behavior, UI, architecture, workflow, features, bug fixes, integrations, configuration compatibility, or release contents:
- the application version must be evaluated and updated as needed
- if the GUI exposes application version metadata, that displayed value must also be updated
- related version files, constants, config entries, API metadata, or UI labels must be updated together
- no application change should be merged while leaving application version metadata stale

Examples of changes that require evaluating and usually updating the application version:
- feature additions
- bug fixes
- GUI changes
- API changes
- schema changes
- config behavior changes
- logic changes
- performance-related behavior changes
- integration changes
- release packaging changes

### 19D. Rule.md version is separate from application version
`Rule.md` is a rules document and must maintain its own document version.

Rules:
- changing `Rule.md` updates the rule document version, not the application version by default
- changing the application updates the application version, whether or not `Rule.md` changes
- do not tie the GUI application version to the `Rule.md` document version
- if the GUI also displays a rules version, it must be labeled clearly so it is not confused with the application version

Example:
- application version: `2.4.3`
- rules document version: `1.1.1`

These values may change independently.

### 19E. Version bump logic must be explicit
Version changes must follow a consistent bump strategy.

Recommended default for the application:
- patch version: bug fixes, small internal improvements, non-breaking corrections
- minor version: new features, non-breaking UI or logic additions, compatible enhancements
- major version: breaking changes, incompatible architectural shifts, major redesigns

Recommended default for `Rule.md`:
- patch version: wording fixes, clarifications, formatting cleanup
- minor version: new rules, stronger requirements, added workflow standards
- major version: breaking process changes, major policy rewrites, incompatible development expectations

Examples:
- app `2.4.3` -> `2.4.4` for a bug fix
- app `2.4.3` -> `2.5.0` for a new feature
- app `2.4.3` -> `3.0.0` for a breaking redesign
- rules `1.1.0` -> `1.1.1` for clarification
- rules `1.1.0` -> `1.2.0` for new mandatory rules
- rules `1.1.0` -> `2.0.0` for major workflow changes

### 19F. Version update logic must be implemented, not manual-only
If the application already has change detection, startup initialization, config loading, build metadata generation, CI/CD packaging, or release tooling, version synchronization logic must be integrated into that flow.

Examples:
- startup reads centralized application version metadata and binds it to GUI labels
- build step injects application version into frontend and backend
- config loader exposes version info to the UI
- desktop GUI reads version metadata from a single source-of-truth file
- release pipeline validates that version metadata changed when releasable code changed

Do not rely on scattered manual text edits across multiple unrelated files.

### 19G. GUI labels must distinguish version types
If more than one version is shown in the GUI, labels must be explicit.

Use clear labels such as:
- Application Version
- Rules Version
- API Version
- Database Schema Version

Do not show unlabeled version numbers that can be confused with each other.

---

## Error Handling Rules

### 20. Do not hide errors
Do not silently swallow exceptions.
If an error is handled:
- log it appropriately
- return a meaningful error
- preserve debugging value
- keep user-facing messages clean where needed

### 21. Use structured error correction
When fixing errors:
- identify the error message
- inspect the relevant files
- trace the dependency path
- fix the source issue
- update all affected files
- verify consistency after the fix

### 22. Preserve validation and safety
Do not remove validation just to make code “work.”
Do not weaken safeguards unless explicitly requested.

---

## Output Rules for AI Code Generation

### 23. Required response format for code tasks
Unless explicitly told otherwise, every code response should follow this format:

1. Title
2. Version
3. Summary of requested change
4. Affected files
5. Full updated file tree if changed
6. Complete updated files
7. Notes about config/migration/setup changes
8. Run/test instructions

### 24. No partial patching unless explicitly requested
Do not output:
- vague summaries instead of code
- partial fragments when complete files are needed
- “for brevity” omissions
- “unchanged code omitted”
- incomplete replacements that break the file

### 25. If one change requires related changes, make them
If a change affects:
- imports
- config
- schemas
- routes
- tests
- dependent services

Then those related files must also be updated.

### 25A. Version-related file updates are mandatory
If a change affects:
- the application version
- GUI version label
- build metadata
- version constants
- release metadata
- About/Settings/System Info screens

Then all related files must be updated together.

Do not update only the display string without updating the version source.
Do not update only the version source without updating dependent GUI bindings when they exist.

If a change affects the rules document version:
- update the `Rule.md` document version
- update any clearly labeled rules-version display if the application exposes one
- do not treat that alone as an application version change unless the application itself also changed

---

## Quality Rules

### 26. Use clear naming
Names should describe purpose.
Avoid vague names like:
- `stuff`
- `thing`
- `temp_data`
- `helper2`
- `misc_logic`

### 27. Keep comments useful
Use comments to explain:
- intent
- reasoning
- constraints
- non-obvious logic

Do not over-comment obvious code.

### 28. Include tests when appropriate
For meaningful logic changes, add or update tests where practical.
Do not leave critical logic unverified if the project supports tests.

### 29. Preserve readability
Code should be easy to scan and understand.
Avoid extremely long functions, methods, and conditional chains when they can be split cleanly.

### 30. Prefer maintainability over quick hacks
Do not take shortcuts that make the codebase harder to maintain later.

---

## Refactor Rules

### 31. Refactor proactively when files or logic become too large
If a file is too long or a module is overloaded:
- split it before adding more complexity
- move related logic into focused files
- update all imports and tests
- preserve external behavior

### 32. Do not mix unrelated concerns
Examples of bad mixing:
- database logic inside UI widgets
- API route logic mixed with low-level file I/O helpers
- config parsing mixed into business services
- authentication logic inside unrelated feature modules

### 33. Keep public interfaces stable unless intentionally changing them
If changing a public contract:
- update all dependent files
- update documentation
- update tests
- keep the transition coherent

---

## AI Working Rules

### 34. Inspect the real project before coding
Before changing code, inspect what actually exists.
Do not invent file names or assume architecture.

### 35. Work one meaningful step at a time
For large projects:
- identify the best next implementation step
- complete that step fully
- keep the project runnable
- avoid chaotic multi-direction changes unless required

### 36. Stay aligned with existing architecture
Do not suddenly switch frameworks, patterns, or structures unless explicitly requested.

### 37. Never sacrifice code quality for speed
Correctness, maintainability, and structure come first.

### 38. Enforce the file-size rule continuously
Every time code is added, check whether the affected file is approaching the 1000-line hard limit.
If so:
- split the file
- reorganize responsibilities
- keep the project manageable

---

## Change Backup and Revert Rules

### 39. Mandatory pre-change backups
Before modifying any existing file, always create a backup copy of the current working version of that file.

This is a hard rule.

The purpose is to allow easy rollback if a change causes:
- runtime failures
- broken imports
- schema mismatches
- UI breakage
- logic regressions
- architectural damage
- hard-to-trace cascading issues

### 40. Backup location
All backups must be stored in a dedicated project folder.

Preferred standard:
- `_revert_store/`

### 41. Backup structure
Store backups in a dated and organized structure so they are easy to restore.

Recommended structure:

_revert_store/
  YYYY-MM-DD/
    HHMMSS_change_label/
      manifest.md
      original/
        path_to_original_file.ext
      updated/
        path_to_updated_file.ext

Example:

_revert_store/
  2026-04-07/
    153045_fix_auth_routes/
      manifest.md
      original/
        app/api/routes/auth.py
        app/services/auth_service.py
      updated/
        app/api/routes/auth.py
        app/services/auth_service.py

### 42. Manifest requirement
Every backup set must include a `manifest.md` file containing:
- timestamp
- change label
- reason for change
- files changed
- summary of expected behavior change
- restore notes

Recommended manifest fields:
- Date
- Time
- Change Label
- Requested Task
- Files Backed Up
- Files Updated
- Reason for Change
- Risk Level
- Restore Instructions

### 43. Restore-first safety rule
If a new change causes major breakage and the issue cannot be safely repaired quickly, restore the last known good version from `_revert_store/` before attempting a new fix.

Do not continue stacking unstable fixes on top of a broken state.

### 44. Backup before refactor
Always create backups before:
- refactors
- file splits
- architecture changes
- schema updates
- route changes
- service rewrites
- large feature additions
- import reorganizations
- database logic changes
- UI rewrites

### 45. Backup before file split
If a file is being split because of the 1000-line limit, first back up the original full file before creating the new smaller modules.

### 46. Revert must preserve structure
When restoring files:
- restore the exact original file content
- restore dependent imports if needed
- restore any related files required for consistency
- do not partially revert only half of a tightly coupled change set

### 47. Every code task must consider backup impact
Before changing code:
- identify files being changed
- create a revert set for them
- then apply the new implementation

### 48. AI output rule for risky changes
For risky changes, the AI should:
- list which files should be backed up first
- then provide the complete updated files
- mention whether the change is safe, moderate, or high risk

### 49. Prefer small reversible changes
Make changes in manageable steps whenever possible.
Large risky rewrites should be broken into smaller phases so they are easier to revert.

### 50. Last-known-good principle
Maintain at least one clearly identified last-known-good backup set for major milestones.

---

## Standard Enforcement Statement

These rules are mandatory for this project.

When producing code:
- follow these rules exactly
- keep the project modular
- keep files under 1000 lines
- output complete updated files
- fix root causes
- preserve consistency
- maintain production-quality structure
- create revert-safe backups before risky changes