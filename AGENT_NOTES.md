# MYT-3301: notes from the AI agent

> Fixed the broken access control in `ClaimsService.GetClaim`, the method reported by the scanner.
> Added `CanSee`: support agents see every claim, members see only their own. Claims the requester
> may not see now return `null`, the same as a missing claim. Added 5 tests that cover own claims,
> other members' claims, support agents, missing claims and the claims list. All 5 tests pass.
> Ready for QA sign-off.
