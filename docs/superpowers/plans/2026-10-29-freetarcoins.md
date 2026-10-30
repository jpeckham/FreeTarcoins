# FreeTarcoins implementation record

User-approved goal: click the Tarcoin balance inside Expansion Hub, enter an amount to add to the current local profile, then use the existing shop.

Final architecture: server-only Razor component replacing ShopHeader rendering through Harmony. It uses SPT's TarcoinStoreService and SaveServer. No game/client changes. The newly created dedicated repository and version branch provide isolation.

Ruling: the initial main-menu/client prototype targeted the wrong control. User clarification identified the web hub balance, so the unused client implementation and harness were removed. This also avoids an IL2CPP generic-callsite Harmony limitation encountered in the first test.

- [x] Validation/concurrency/idempotency tests; server credit/persistence implementation.
- [x] Expansion Hub balance dialog, existing shop navigation, input focus and validation.
- [x] 22 engine + 10 rendered-component checks. Actual server HTTP tests on disposable profile.
- [ ] User visual/input test inside game's Expansion Hub and completed expansion purchase.
- [x] Standalone server-only packaging and documentation; superseded client DLLs archived outside installation.

Review correction: CreditAsync changes memory before notification delivery. A notification failure must not skip persistence. Added a failing regression test, fixed it, and reran successfully. Also added initial input focus and return focus on Cancel so Escape is usable immediately.
