# FreeTarcoins

A planned client/server mod for local SPT that lets you add Tarcoins through the in-game Tarcoin button.

**Status: repository initialized; no plugin implemented or installed yet.**

## Intended behavior

Click the Tarcoin balance button, enter an amount to add, and credit the current local SPT profile. Refresh the displayed balance and use SPT's existing expansion shop for purchases.

The server already provides `TarcoinStoreService.CreditAsync` and balance notifications. Implementation must still identify the correct client button hook, validate amounts, persist changes, and verify expansion purchases end to end.

## Initial target

- SPT 5.0.0 bleeding edge
- EFT 1.3.0.0.48484
- BepInEx IL2CPP and .NET 10
- Local installation: `C:\sptarkov\SPTarkov 5.0.0-BE-1.3.0.0.48484`
- Development branch: `spt-5.0-eft-1.3.0.0.48484`

This mod is intended for the local SPT profile and backend. It does not credit a live Escape from Tarkov account.

Part of the [SPT-BE workspace](https://github.com/jpeckham/SPT-BE), checked out at `mods/FreeTarcoins`.

## License

MIT; see [LICENSE](LICENSE).
