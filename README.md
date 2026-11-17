# Test an AI agent's security fix before it is signed off

You are the test engineer on a team that fixes security problems in an old health-benefits website.
An AI agent fixed security problem **MYT-3301** (members could see other members' claims) in
`src/Claims/ClaimsService.cs`, wrote 5 tests, and asks for your sign-off. Your job: find out whether
the fix really closes the problem without breaking what members and support agents need, and prove it
with tests. You may use your AI tool; we will watch how you use it.

**Do not change the code in `src/`.** If you find a problem, show it with a test and write it down.
A test that shows a problem in the agent's fix is expected to fail on the agent's code.

## Files

- `FINDING.md`: the problem, and who may see which claims.
- `AGENT_NOTES.md`: what the agent says it did.
- `src/Claims/ClaimsService.cs`: the code after the agent's change. `src/Claims/Db.cs`: the sample data.
- `tests/Claims.Tests/AgentTests.cs`: the agent's tests. `tests/Claims.Tests/MyTests.cs`: yours.

## Start

```
dotnet test tests/Claims.Tests
```

## Tasks: 15 minutes in total (any order)

1. **Review the agent's tests (about 3 minutes).** What do they really prove, and what do they miss?
2. **Your tests (about 8 minutes).** In `MyTests.cs`, write the tests you would need before signing
   off: each one should check one rule in `FINDING.md`.
3. **Notes (about 4 minutes).** Fill in `NOTES.md`: what the agent's tests miss, the problems you
   found, and whether you would sign off, with your evidence.

If you are still on one task two minutes after its time, move to the next one.
At 15 minutes we stop. Then run:

```
dotnet run --project check
```

Do not edit the `check` folder.
