# ChMS

Church management and fund accounting software for one church: members and households, gifts by fund, deposits and bank reconciliation, a fund ledger, member giving history, and IRS year-end contribution statements.

Nothing is built yet. The approved design lives in [`docs/`](docs/):

| Doc | What it covers |
| --- | --- |
| [01-scope.md](docs/01-scope.md) | What the first release does and doesn't do, the gift-to-bank flow, roles |
| [02-data-model.md](docs/02-data-model.md) | Every record and field, ledger posting rules, chart of accounts, starter funds, screens |
| [03-irs-statements.md](docs/03-irs-statements.md) | The IRS rules statements must follow |
| [04-architecture-and-plan.md](docs/04-architecture-and-plan.md) | Azure tech stack, security, build phases to January 2028 |
| [decisions.md](docs/decisions.md) | Every decision made so far, with dates |

## Principles for contributors

- **Money is `decimal`, never `float` or `double`.** Store as `decimal(19,4)`.
- **Financial records are never deleted or edited after posting.** Reverse and re-enter instead.
- **Every journal entry balances**, in total and per fund. Enforce it in the database, not only in code.
- **Every change to a gift, deposit, entry, period, statement or role is audit-logged.**
- **Giving amounts are filtered by role in the API**, not just hidden in the UI.
- **No real member data in test environments.** Use generated data.
- **No card numbers or full bank account numbers**, ever.

## Getting started

Phase 0 (Foundations) is next: the ASP.NET Core and React project skeleton, Microsoft 365 sign-in, roles, the audit log, and a GitHub Actions build. See the phase table in [04-architecture-and-plan.md](docs/04-architecture-and-plan.md).
