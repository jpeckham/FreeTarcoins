using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Models.Spt.Mod;
namespace FreeTarcoins;

[Injectable(InjectionType.Singleton)]
public class CreditStore(SaveServer saves, TarcoinStoreService coins) : ICreditStore
{
    public int? BalanceFor(string session)
    {
        var id = new MongoId(session);
        if (id.IsEmpty || !saves.ProfileExists(id)) return null;
        var pmc = saves.GetProfile(id)?.CharacterData?.PmcData;
        return pmc is null ? null : pmc.TarCoinBalance ?? 0;
    }
    public Task Credit(string session, int amount) => coins.CreditAsync(new MongoId(session), amount);
    public async Task Save(string session) => await saves.SaveProfileAsync(new MongoId(session));
}
[Injectable(InjectionType.Singleton)]
public class CreditHandler(CreditStore store)
{
    private readonly CreditEngine engine = new(store);
    public Task<CreditResult> Add(string session, int amount, string requestId) => engine.Add(session, amount, requestId);
    public async ValueTask<string> Handle(CreditRequest request, MongoId session)
    {
        var result = await engine.Add(session.ToString(), request.Amount, request.RequestId ?? "");
        return JsonSerializer.Serialize(result);
    }
}
[Injectable]
public class Routes(JsonUtil json, CreditHandler handler) : StaticRouter(json, [
 new RouteAction<CreditRequest>("/freetarcoins/add",async (url,info,session,output,ct)=>await handler.Handle(info,session))
]);
public record CreditRequest : IRequestData
{
    [JsonPropertyName("amount")] public int Amount { get; set; }
    [JsonPropertyName("requestId")] public string? RequestId { get; set; }
}
public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.jpeckham.freetarcoins";
    public string Name { get; init; } = "FreeTarcoins";
    public string Author { get; init; } = "jpeckham";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("0.1.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~5.0.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; } = "https://github.com/jpeckham/FreeTarcoins";
    public string License { get; init; } = "MIT AND CC-BY-NC-SA-4.0";
}
