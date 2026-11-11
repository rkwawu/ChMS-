# IRS statement rules

The year-end statement is how the church meets the written acknowledgment rule for US 501(c)(3) donations, so its wording is a compliance requirement. Sources: [IRS substantiation and disclosure requirements](https://www.irs.gov/charities-non-profits/charitable-organizations/charitable-organizations-substantiation-and-disclosure-requirements) and IRS Publication 1771. The church's accountant must review the final wording before the first statements are sent.

| Rule | Trigger | What the software must do |
| --- | --- | --- |
| Written acknowledgment | Any single gift of $250 or more | Church name, date, cash amount, and whether goods or services were provided. Donors need it by their filing date; send by 31 January. |
| Religious benefit wording | Every statement | "No goods or services were provided in exchange for these contributions other than intangible religious benefits," or similar. |
| Quid pro quo disclosure | Payment over $75 that is partly a gift | State that only the amount above fair market value is deductible, with a good-faith estimate of that value. |
| Non-cash gifts | Stock, goods, property | Describe the item; never state its value. Donor files Form 8283 over $500; over $5,000 the church may sign Form 8283 and must file Form 8282 if it disposes of the item within 3 years. |
| Vehicles | Claimed value over $500 | Issue Form 1098-C within 30 days. |
| Records | Every gift | Keep donor, date and amount; retain at least 7 years. |

## Behaviour

- Statements include only gifts with status `posted` in closed, reconciled months.
- Pledges are never shown on a statement.
- Anonymous gifts (no donor) never appear on a statement.
- A child's gifts are listed on their parents' statement.
- Each statement shows totals per fund and an itemized gift list, with deductible amounts for quid pro quo gifts.
- Corrections produce a new version marked as corrected; the previous PDF is kept.
