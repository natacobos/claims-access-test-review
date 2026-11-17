# Security problem MYT-3301: members can see other members' claims

**Source:** the client's security scanning tool, confirmed by the client's security team ·
**Type:** broken access control (CWE-639) · **Severity:** High

**What happens:** on the claim detail page, changing the claim number in the address showed a claim
of **another member**. The scanner reported `ClaimsService.GetClaim`.

**Who uses `ClaimsService`:** the claim detail page (`GetClaim`), the claims list page (`ListClaims`)
and the support agents' tool.

**Who may see which claims (product rules, unchanged):**

- A member sees their own claims.
- An account holder also sees the claims of their dependents (for example, a parent sees their
  child's claims). The table `members` has a `holder_id` column for this.
- A dependent does not see the account holder's claims, or anyone else's.
- A support agent (role `support`) sees every claim.
- Any other role sees nothing.
- When the requester may not see a claim, the answer looks the same as when it does not exist:
  `GetClaim` returns `null`, `ListClaims` returns an empty list. No error that tells them it exists.

**To close it:** the fix merged through the pipeline, tests that prove it, and a new scan
that no longer reports the problem.
