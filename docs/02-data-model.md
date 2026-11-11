# Data model

Approved 11 November 2026.

The system rests on one chain: a person gives a gift, split across funds, collected in a batch, deposited, matched to a bank line, and posted to the fund ledger. Every record below exists to keep that chain complete and to produce a correct IRS statement from it.

## People and households

| Record | Key fields | Notes |
| --- | --- | --- |
| Person | Name, preferred name, birth date, email, phone, address, membership status, member since, date of death | Status: visitor, attender, member, inactive, deceased. A deceased person's giving stays on their record. No envelope numbers. |
| Household | Name, mailing address, statement mode | Statement mode: each adult separately, or the couple together. |
| Household link | Person, household, role, start date, end date | Role: head, spouse, child, other. End dates keep history. A child's gifts appear on their parents' statement. |
| Relationship | Person, related person, type | Spouse, parent, guardian. |

## Funds, gifts and pledges

A gift is money received once; its splits say which funds it goes to.

| Record | Key fields | Notes |
| --- | --- | --- |
| Fund | Code, name, net asset class, restriction type, tax deductible, fund balance account, active | Restriction: unrestricted, board-designated, temporarily restricted, permanently restricted. |
| Gift | Donor person (nullable), date received, total amount, method, check number, batch, processor reference, status | Method: cash, check, card, ACH, non-cash. Status: posted, refunded, returned, reversed. A gift with no donor is anonymous and never appears on a statement. |
| Gift split | Gift, fund, amount | Splits must sum to the gift total. |
| Goods or services given | Gift, description, fair market value | Quid pro quo. Deductible amount = gift total minus this value. |
| Non-cash gift detail | Gift, item description, donor's stated value, appraisal received, Form 8283 signed, date sold, Form 8282 filed | Statement prints the description only, never a value. |
| Pledge | Person or household, fund or campaign, total promised, start, end, frequency | Never on the IRS statement; shown against actual giving. |

## Batches, deposits and reconciliation

| Record | Key fields | Notes |
| --- | --- | --- |
| Batch | Source, date, expected total, counter 1, counter 2, status | Source: Sunday count, mail, processor. Status: open, balanced, closed. Closes only when entered gifts equal the expected total. |
| Bank account | Name, bank, last 4 digits, ledger account | Full account numbers are never stored. |
| Deposit | Bank account, date, gross, fees, net, batches, processor payout reference | Net is what the bank shows. |
| Bank statement import | Bank account, file, period, opening and closing balance, imported by, file hash | Re-importing the same file is detected and skipped. |
| Bank line | Import, date, amount, description, bank transaction ID (FITID), match status | Match status: unmatched, matched, ignored (with reason). |
| Match | Bank line, deposit or journal entry, matched by, matched at | One bank line can match several deposits. |
| Reconciliation period | Bank account, month, statement ending balance, book balance, difference, status, closed by | Closes only when the difference is zero and every line is matched or explained. |

## Fund ledger

Every line posts to an account and a fund. Net asset classes follow nonprofit GAAP: without donor restrictions (general, board-designated) and with donor restrictions (temporary, permanent).

| Record | Key fields | Notes |
| --- | --- | --- |
| Account | Number, name, type, parent, active | Types: asset, liability, net assets, revenue, expense. |
| Journal entry | Number, date, memo, source, status, prepared by, posted at | Source: deposit, processor fee, bank withdrawal, restriction release, recurring, manual, opening balance. Status: draft, posted, reversed. |
| Journal line | Entry, account, fund, debit, credit, memo | Debits equal credits for the entry and for each fund in it. |
| Fiscal year and period | Year, monthly periods, status | Fiscal year is January to December. Status: open, closed, locked after audit. |
| Restriction release | From fund, amount, purpose met, approved by, journal entry | Records restricted money spent for its purpose. |
| Recurring entry | Template entry, schedule, next date | Depreciation, loan payments. |

Automatic postings:

1. Closing a cash/check deposit: debit 1050 Undeposited, credit revenue per gift split; when matched to the bank, debit the bank account, credit 1050.
2. Stripe gifts: debit 1060 Processor clearing, credit revenue per split. Payout: debit the bank account (net), debit 7530 fees, credit 1060 (gross).
3. Bank withdrawal found during reconciliation: an expense entry created on the spot (account + fund).
4. Everything else (transfers, corrections, opening balances) is a manual entry, posted directly and logged.

Ledger rules: posted entries are never edited, only reversed. Manual entries need no second approver; every one is in the audit log. An entry that would push a restricted fund below zero is blocked. The books start on the go-live date with one opening-balance entry; that year's earlier gifts are imported so statements are complete.

## Statements, users and audit

| Record | Key fields | Notes |
| --- | --- | --- |
| Statement | Recipient (person or household), tax year, version, total, PDF, generated at, sent at, sent by, replaced by | Corrections create a new version; nothing is deleted. |
| Statement template | Church name, address, EIN, wording, signature, effective year | Changes apply to future statements only. |
| User | Name, email, Entra object ID, linked person, last sign-in | |
| Role assignment | User, role, granted by, granted at | |
| Audit log entry | Who, when, record type, record ID, action, before, after, reason | Append-only; use an Azure SQL ledger table. |

## Rules that keep records audit-safe

1. Nothing financial is deleted; wrong gifts are reversed with a reason.
2. Closed months are locked; fixes are dated adjustments in the current month.
3. A Sunday batch needs two different counters.
4. Gift splits always balance to the gift total.
5. Money is stored exactly: `decimal(19,4)` in SQL and `decimal` in C#, never floating point.
6. Gifts follow the person, not the household.
7. Statements use reconciled data only, unless the treasurer overrides with a logged reason.
8. Moving a gift out of a restricted fund needs the treasurer and a reason.
9. Giving amounts are hidden from non-finance roles in pages, APIs and exports.
10. Data is encrypted at rest and in transit; card and full bank numbers are never stored.

## Chart of accounts

Accounts say what kind of money it is; funds say what it is for. Lines are written account-fund, e.g. `4010-30` for a missions gift.

| Number | Account | Type |
| --- | --- | --- |
| 1010 | Operating checking | Asset |
| 1020 | Savings | Asset |
| 1030 | Reserve and building savings | Asset |
| 1050 | Undeposited cash and checks | Asset |
| 1060 | Payment processor clearing | Asset |
| 1100 | Petty cash | Asset |
| 1200 | Investments | Asset |
| 1300 | Prepaid expenses | Asset |
| 1500 | Land | Asset |
| 1510 | Buildings | Asset |
| 1520 | Furniture and equipment | Asset |
| 1530 | Vehicles | Asset |
| 1590 | Accumulated depreciation | Asset (contra) |
| 2010 | Church credit card payable | Liability |
| 2100 | Payroll taxes withheld | Liability |
| 2200 | Gifts held for others | Liability |
| 2500 | Mortgage payable | Liability |
| 3010 | Net assets without donor restrictions | Net assets |
| 3020 | Board-designated net assets | Net assets |
| 3100 | Net assets with donor restrictions, temporary | Net assets |
| 3200 | Net assets with donor restrictions, permanent | Net assets |
| 4010 | Tithes and offerings | Revenue |
| 4020 | Special and designated gifts | Revenue |
| 4030 | Non-cash gifts | Revenue |
| 4100 | Event and registration income | Revenue |
| 4200 | Facility rental income | Revenue |
| 4300 | Interest and dividends | Revenue |
| 4400 | Other income | Revenue |
| 4900 | Released from restriction | Revenue (transfer) |
| 5010 | Pastor salary | Expense: personnel |
| 5020 | Pastor housing allowance | Expense: personnel |
| 5030 | Staff wages | Expense: personnel |
| 5040 | Employer payroll taxes | Expense: personnel |
| 5050 | Health insurance | Expense: personnel |
| 5060 | Retirement contributions | Expense: personnel |
| 6010 | Mortgage interest | Expense: facilities |
| 6020 | Utilities | Expense: facilities |
| 6030 | Repairs and maintenance | Expense: facilities |
| 6040 | Property and liability insurance | Expense: facilities |
| 6050 | Cleaning and grounds | Expense: facilities |
| 6510 | Worship and music | Expense: ministry |
| 6520 | Children's ministry | Expense: ministry |
| 6530 | Youth ministry | Expense: ministry |
| 6540 | Adult education and small groups | Expense: ministry |
| 6550 | Outreach and events | Expense: ministry |
| 6560 | Benevolence assistance | Expense: ministry |
| 7010 | Missionary support | Expense: missions |
| 7020 | Denominational giving | Expense: missions |
| 7030 | Mission trips | Expense: missions |
| 7510 | Office supplies | Expense: administration |
| 7520 | Software and technology | Expense: administration |
| 7530 | Bank and card processing fees | Expense: administration |
| 7540 | Audit, legal and accounting fees | Expense: administration |
| 7550 | Postage and printing | Expense: administration |
| 8010 | Depreciation | Expense: other |
| 8020 | Fundraising event costs | Expense: other |

### Starter funds

| Code | Fund | Restriction |
| --- | --- | --- |
| 10 | General | Without donor restrictions |
| 15 | Operating reserve | Board-designated |
| 20 | Building | With donor restrictions, temporary |
| 30 | Missions | With donor restrictions, temporary |
| 40 | Benevolence | With donor restrictions, temporary |
| 50 | Youth ministry | With donor restrictions, temporary |
| 60 | Scholarship | With donor restrictions, temporary |
| 70 | Capital campaign | With donor restrictions, temporary |
| 90 | Endowment | With donor restrictions, permanent |

Accounts and funds can be added, renamed or retired; retired ones keep their history.

## Main screens

| Screen | Who | What it shows |
| --- | --- | --- |
| Member page | Staff; finance roles see giving | Profile, household, giving tab by year and fund, pledges, past statements |
| Household page | Staff | Members, address, statement mode |
| Member portal | Members | Own profile, giving history, statements |
| Batch entry | Counters, finance assistant | Fast keyboard entry with running total vs counted total |
| Deposits | Finance | Deposits by bank account, batches or payouts inside, fees |
| Bank reconciliation | Treasurer | Bank lines beside deposits, suggested matches, exceptions, difference |
| Funds | Treasurer | Funds, restriction types, balances |
| Chart of accounts | Treasurer | Accounts and their status |
| Journal entries | Treasurer | Entries, reversals, opening balances |
| Financial reports | Treasurer | Trial balance, ledger detail, fund balances, statement of financial position, statement of activities |
| Year-end statements | Treasurer | Preview, exceptions, bulk send, corrections |
| Audit log | Treasurer, administrator | Every change, filterable |
| Users and roles | Administrator | Access and visibility |
