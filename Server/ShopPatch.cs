using HarmonyLib;
using Microsoft.AspNetCore.Components.Rendering;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Web.Pages.Shop.Components;

namespace FreeTarcoins;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 1)]
public class ShopMod(CreditHandler handler, ISptLogger<ShopMod> logger) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken ct)
    {
        ShopPatch.Credit = handler.Add;
        try
        {
            new Harmony("com.jpeckham.freetarcoins.shop").Patch(
                AccessTools.Method(typeof(ShopHeader), "BuildRenderTree"),
                prefix: new HarmonyMethod(typeof(ShopPatch), nameof(ShopPatch.Render)));
            logger.Info("[FreeTarcoins] Expansion Hub balance dialog enabled.");
        }
        catch (Exception ex)
        {
            logger.Error("[FreeTarcoins] Could not enable the balance dialog: " + ex);
        }
        return Task.CompletedTask;
    }
}

public static class ShopPatch
{
    public static Func<string, int, string, Task<CreditResult>> Credit = null!;

    // Only the shop header is replaced. Catalog and purchase components remain the server's own.
    public static bool Render(ShopHeader __instance, RenderTreeBuilder __0)
    {
        __0.OpenComponent<FreeTarcoinsHeader>(0);
        __0.AddAttribute(1, "Balance", __instance.Balance);
        __0.CloseComponent();
        return false;
    }
}
