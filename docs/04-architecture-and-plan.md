# Architecture and build plan

Approved 11 November 2026. Built and maintained by the church.

## Tech stack

| Piece | Choice | Why |
| --- | --- | --- |
| Web app and API | ASP.NET Core, C#, .NET 10 (LTS) | Microsoft's main web framework; `decimal` money math |
| Screens | React with TypeScript | Fast batch entry and reconciliation screens; desktop and phone browsers |
| Database | Azure SQL Database | Transactions keep entries balanced; ledger tables for a tamper-evident audit log; Power BI and Excel connect directly |
| Hosting | Azure App Service | Managed hosting; separate test and live environments |
| Files | Azure Blob Storage | Statement PDFs and bank files, with versioning |
| Secrets | Azure Key Vault | Stripe keys and connection strings never in code |
| Staff sign-in | Microsoft Entra ID (Microsoft 365) | Existing accounts, MFA enforced |
| Member sign-in | Microsoft Entra External ID | Separate from staff accounts |
| Online giving | Stripe | Hosted giving page, recurring gifts, webhooks for gifts and payouts |
| Email | Microsoft Graph or Azure Communication Services | Statements sent from the church's address |
| Monitoring | Azure Application Insights | Errors and failed imports |
| Code and releases | GitHub with GitHub Actions | Tests on every pull request |

## Security

- No card data or full bank account numbers stored.
- Encryption in transit and at rest, including backups.
- MFA for all staff through Microsoft 365.
- Least access: giving amounts only for finance roles, enforced in the API, not just the UI.
- Append-only, tamper-evident audit log.
- Point-in-time restore (35 days) and geo-redundant backups.
- Real member data never in the test environment.
- Security review before go-live.

## Build phases

| Phase | Months | Done when | Review |
| --- | --- | --- | --- |
| 0. Foundations | Dec 2026 | Staff sign in with Microsoft 365 on a test site; roles and audit log work | Sign in and check roles |
| 1. People and households | Jan 2027 | Add and import members and households; member pages | Import a sample of records |
| 2. Funds and gifts | Feb–Mar 2027 | Sunday batches with two counters; gift splits; giving tab | Enter a real Sunday's gifts on test |
| 3. Go live, part 1 | Apr 2027 | Live for members and gifts; January–March gifts imported | Approve go-live |
| 4. Fund ledger | Apr–Jun 2027 | Chart of accounts, opening entry, deposits post to ledger, fund balances, financial reports | Check opening balances and reports |
| 5. Online giving and reconciliation | Jul–Aug 2027 | Stripe gifts flow in; bank file import; months reconciled back to April | Reconcile one month |
| 6. Member portal | Sep 2027 | Members see own giving and update profiles | Try with a few members |
| 7. Statements | Oct–Nov 2027 | Preview, correct and send statements; non-cash and quid pro quo wording | Accountant reviews wording |
| 8. Security review and dry run | Dec 2027 | Full test run of 2027 statements | Approve sending |
| 9. Year-end | Jan 2028 | December closed; 2027 statements sent by 31 January | Sign off |

## Approximate running cost

Roughly $60–$150 a month on Azure for one church, before any nonprofit grant, plus Stripe fees on online gifts. Confirm with the Azure pricing calculator before provisioning.

## Needed before or during the build

- [x] Plan approved
- [x] Builder and maintainer: the church
- [x] GitHub repository
- [ ] Azure subscription and Microsoft 365 admin access (phase 0)
- [ ] Sample exports of member and giving records (phase 1)
- [ ] Church legal name, address and EIN (phase 7)
- [ ] Stripe account in the church's name (phase 5)
- [ ] Accountant review of statement wording (phase 7)
