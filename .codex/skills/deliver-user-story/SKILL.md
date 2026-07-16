---
name: deliver-user-story
description: Implement and verify a Dynamic.Json.EfCore user story from its narrative, scope, acceptance criteria, exclusions, and validation requirements. Use when Codex is asked to deliver the next library story, persistence or query feature, metadata enhancement, provider integration, regression fix framed as a story, or a continuation of roadmap work in this repository.
---

# Deliver a User Story

Turn the story into a small, reviewable repository change whose tests map directly to its acceptance criteria.

## Establish the boundary

1. Read the complete story, including context, scope, acceptance criteria, exclusions, and validation.
2. Inspect `AGENTS.md` when present, repository status, solution structure, relevant production code, neighboring tests, `README.md`, `ROADMAP.md`, and `CHANGELOG.md` as needed.
3. Preserve unrelated user changes in a dirty worktree.
4. Translate each acceptance criterion into an observable behavior. Treat out-of-scope items as hard boundaries.
5. Resolve details from existing architecture and conventions. Ask the user only when a missing choice would materially change the public API or result.
6. Send a short commentary update stating the inferred implementation boundary and validation approach before editing.

## Design the smallest complete change

- Follow the existing public API, naming, namespace, nullability, XML documentation, and test conventions.
- Extend the established pipeline instead of creating a parallel abstraction.
- Avoid speculative support for future roadmap items.
- Prefer behavioral tests over implementation-coupled tests.
- Cover null, empty, populated, and regression cases when the story distinguishes them.
- Include a production change only when behavior requires one. If existing code already satisfies the story, add decisive regression coverage and explain that finding instead of manufacturing code churn.
- Update public documentation, roadmap, changelog, or coverage inventory only when repository convention or the story requires it.

## Implement safely

1. Make focused edits with `apply_patch`.
2. Keep query translation, validation, change tracking, provider behavior, or demo changes untouched when excluded.
3. Add unit tests for conversion, serialization, metadata, or isolated behavior.
4. Add persistence or provider integration tests only where a real database boundary is part of acceptance.
5. Verify serialized provider data directly when exact storage shape matters; separately materialize through a fresh or no-tracking context.
6. Keep an existing scalar or established behavior in the test fixture when the story requires regression confidence.
7. Share concise progress commentary when discovery changes the expected implementation or work runs longer than a minute.

## Validate proportionally

Run validation from narrowest to broadest:

1. Build or run the affected unit-test project with the relevant test filter.
2. Run the full affected unit-test project.
3. Run integration tests required by the story when their infrastructure is available.
4. Build the solution or run the broader suite when risk and runtime justify it.
5. Inspect `git diff --check`, `git diff`, and `git status --short` before handoff.

Do not claim an integration test passed when it was skipped or infrastructure was unavailable. Report the exact limitation and preserve compile-time validation where possible.

## Hand off the story

Lead with the delivered behavior. Briefly report:

- the production and test changes;
- how populated, null, empty, and regression behaviors were covered, when applicable;
- commands run and their outcomes;
- any skipped validation or remaining risk.

Link the most relevant changed files. Do not present out-of-scope follow-up work as part of the completed story.
