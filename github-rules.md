GitHub & Team Collaboration Guidelines
1. Repository Structure and Branching Model
Main Branches:


main: Contains the stable, production-ready version of the project.
beta: Serves as an integration/testing branch for new features and bug fixes.
Feature Branches:


Create a new branch for each feature, bug fix, or experiment (e.g., username-feature/player-movement or username-bugfix/collision-issue).
Branch names should be descriptive and follow a consistent naming convention.
Hotfix Branches:


For urgent fixes to the main branch, create a branch off main, and once the fix is verified, merge it back into beta then main.

2. Commit Message Guidelines
Descriptive Messages:


Write clear, concise commit messages that describe what was changed and why.
Use a common structure: a short summary on the first line, followed by a more detailed explanation if needed.
For detailed explanation follow the X, Y, Z method.
X: Explain what is the PR
Y: Why is it needed
Z: How it was implemented
Commit Format:


Example: feat: add player jump mechanic (#23)
Prefixes: feat (new feature), fix (bug fix), docs (documentation), refactor (code restructuring), style (formatting), etc.
Reference related issues or tasks from trello
Small, Atomic Commits:


Commit small, logical changes frequently. This makes it easier to track changes and review code.

3. Pull Request (PR) Process
Opening a PR:


Always create a pull request for merging any changes from a feature branch into beta (or the designated integration branch).
Include a descriptive title and summary that explains what the PR accomplishes.
Reference related issues or tasks in the PR description.
Review Process:


Each PR should be reviewed by at least one other team member before merging.
Review stage is to agree on the coding style methodology chosen.
Provide constructive feedback and be open to suggestions.
Use GitHub’s review tools to approve or request changes.
Merging Guidelines:


Merge only after the code has passed reviews and any necessary tests.
Consider using a merge strategy (e.g., squash merge or rebase merge) to keep the commit history clean.
