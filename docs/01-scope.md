# First release scope

Approved 11 November 2026.

The first release does six things: members and households, gifts by fund, deposits and bank reconciliation, a fund ledger, a giving history on each member's page, and IRS-compliant year-end statements. Everything else waits until this works.

## In scope

| Module | What it does | Key rules |
| --- | --- | --- |
| Members and households | Profiles, membership status, contact details, household links | A person belongs to one household at a time; history is kept when people move, marry or die |
| Gifts and funds | Record cash, check, card and ACH gifts against funds; pledges tracked separately | Every gift has a donor (or is anonymous), date, amount, fund split and method; restricted funds are flagged |
| Deposits and bank reconciliation | Group gifts into batches and deposits, import bank statement files, match each deposit to a bank line | Card payouts matched net of fees, fee booked separately; closed months are locked and corrected only by logged adjustments |
| Fund ledger | Chart of accounts, journal entries, fund balances, restriction releases, financial reports | Every line carries an account and a fund; debits equal credits per entry and per fund |
| Member giving page | Gifts by year and fund, pledges against giving, statement downloads | Only finance roles see amounts; members see their own history in the portal |
| Year-end statements | Generate, review, email or print IRS statements in bulk; issue corrected versions | See [IRS statement rules](03-irs-statements.md) |

Every change to a gift, deposit, journal entry or statement is written to an audit log with who, when, and before and after values.

## Out of scope for now

| Left out | Use instead |
| --- | --- |
| Accounts payable (bill approval, check printing), accounts receivable, budgets | Expenses are entered from bank withdrawals during reconciliation; bills are paid as today |
| Payroll and clergy housing allowance | A payroll service such as Gusto or ADP; payroll is posted to the ledger as journal entries |
| Attendance, check-in, groups, events, facilities | A later module |
| Email and SMS campaigns | Microsoft 365 or an email tool, using exported lists |
| Pastoral care notes | A later module with stricter permissions |
| Online giving forms and mobile app | Stripe's hosted giving page feeding this system |

## How a gift reaches the bank statement

1. **The gift is recorded.**
   - Cash and checks: two counters enter Sunday's gifts into a batch; the batch total must match the counted total.
   - Card and ACH: Stripe sends each gift by webhook, already tied to the donor and fund.
2. **Gifts form a deposit.**
   - Cash and checks: the closed batch becomes one bank deposit.
   - Card and ACH: each Stripe payout becomes one deposit, recorded as gross gifts minus fees.
3. **The bank statement is imported** (CSV or OFX file in the first release; a live bank feed later).
4. **Each deposit is matched to a bank line.** Exact amount and date matches are suggested; the treasurer confirms. Unmatched items stay on an exceptions list.
5. **Refunds and returned checks** reverse the original gift with a logged entry.
6. **The month is closed** once every line is matched or explained.
7. **At year end**, statements are built only from gifts in closed, reconciled months.

## Roles

| Role | Can do | Sees giving amounts |
| --- | --- | --- |
| Administrator | Manage users and settings | No, unless also given a finance role |
| Treasurer | Reconcile, close months, post journal entries, issue statements | Yes |
| Finance assistant | Enter batches, manage giving records | Yes |
| Counter | Enter cash and check batches during counting | Only the current batch |
| Pastor and staff | View member profiles and households | No |
| Member | Update own profile, view own giving, download own statements | Own only |

All staff sign in through Microsoft 365 with multi-factor authentication.

## Assumptions (confirmed)

- One church, one campus. No multi-tenant data separation.
- Fund ledger is built in and replaces QuickBooks or Aplos for the church's books.
- Stripe processes online gifts; card data never touches our servers.
- Statements go to individuals by default, with an option to combine a married couple.
- Bank statements are imported as CSV or OFX files.
- Hosted on Microsoft Azure with Microsoft 365 sign-in for staff.
- Existing records are imported from spreadsheets.
- Target: live in time to send 2027 statements by 31 January 2028.
