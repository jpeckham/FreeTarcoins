# FreeTarcoins

Add Tarcoins to your local SPT profile by clicking the **Tarcoin balance inside Expansion Hub**.

Version **0.1.0**, targeting the local **SPT 5.0.0 bleeding-edge / EFT 1.3.0.0.48484** build. This is a **server-only mod**: no BepInEx plugin or WebOverlay dependency is required. It does not change live Escape from Tarkov accounts.

## Use

1. Start the SPT server and client, then open Expansion Hub normally.
2. Click the Tarcoin balance at the top of the hub.
3. Enter the positive whole number of Tarcoins to **add**, then press **Add Tarcoins** (or Enter).
4. The server saves the new balance and refreshes the shop so prices and affordability agree. Buy items using the existing shop controls.

Cancel, Escape, or clicking outside the dialog closes it before submission. Controls are disabled while a credit is pending. The amount and resulting balance must fit a signed 32-bit integer (maximum balance 2,147,483,647). A failed or uncertain save displays a message; check your balance before submitting again.

## Install / remove

With the server stopped, extract the package into your SPT installation. It contains:

```text
SPT_Runtime/user/mods/FreeTarcoins/
  FreeTarcoins.Server.dll
  README.md
  LICENSE
  LICENSE-ShopHeader.txt
  THIRD-PARTY-NOTICES.md
```

Restart the server. Look for `[FreeTarcoins] Expansion Hub balance dialog enabled.` Close and reopen an already-open Expansion Hub. Remove that mod directory while the server is stopped to uninstall. Removing the mod restores the normal header; previously credited currency remains in your save.

## Build

Requires .NET 10 SDK and the matching SPT server assemblies, including SPTarkov.Server.Web.dll and the server's 0Harmony.dll. No game/server reference DLLs are bundled.

```powershell
dotnet build Server/FreeTarcoins.Server.csproj -c Release
# Or override the default installation:
dotnet build Server/FreeTarcoins.Server.csproj -c Release '-p:SPTPath=C:\sptarkov\SPTarkov 5.0.0-BE-1.3.0.0.48484'
dotnet run --project Tests -c Release
dotnet run --project ComponentTests -c Release
# Optional live integration test; adds 123 coins only to a LocalBuildTest* account:
python Tests/integration.py --runtime <SPT_Runtime-path> --profile <disposable-profile-id>
```

Build output is `Server/bin/Release/net10.0/FreeTarcoins.Server.dll`. Copy it plus the README/license files into the mod directory. Builds do not automatically deploy.

## Implementation and validation

A Harmony prefix replaces only SPT's Blazor ShopHeader rendering with an adapted header containing a clickable balance and amount-entry form. The existing shop page, catalog, purchase actions and Exit behavior remain available. The page URL supplies the same profile session used by SPT's shop. Credits call `TarcoinStoreService.CreditAsync`, persist with `SaveServer.SaveProfileAsync`, and refresh the shop. If a notification fails after the balance changed, persistence is still attempted.

A session-bound `/freetarcoins/add` route shares the same credit engine for integration testing. Positive amounts, profile existence, request IDs and overflow are checked. Credits are serialized and the last 1,024 request receipts are retained in memory to prevent immediate duplicate/retried requests crediting twice. This is not durable idempotency across server restarts; the UI never automatically retries an uncertain credit.

Validation on 48484: 22 credit-engine checks and 10 rendered Blazor checks, including the actual ShopHeader Harmony patch, clicking the balance, focus request, Escape, invalid input, successful submission and shop-refresh request. Actual server HTTP tests verified credits, immediate duplicate replay, invalid/overflow amounts, unknown/missing sessions and profile-file persistence using a disposable account. Live HTTP rendering confirmed the patched header. In-game visual/input confirmation and a complete expansion purchase remain user tests; automated rendering is not a claim that those were completed.

The first prototype targeted the main-menu button and hit an IL2CPP generic-callsite patch limitation. After clarification, that prototype was removed in favor of the correct Expansion Hub web control. No client plugin from that prototype belongs in the final installation.

## Source and licenses

Development branch: `spt-5.0-eft-1.3.0.0.48484`. Part of [SPT-BE](https://github.com/jpeckham/SPT-BE) at `mods/FreeTarcoins`.

Original FreeTarcoins code is MIT licensed. The adapted SPT shop header is **CC BY-NC-SA 4.0**; the distributed DLL includes that material. Retain both licenses and [third-party attribution](THIRD-PARTY-NOTICES.md).
