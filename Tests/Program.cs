using FreeTarcoins;
int checks = 0;
void Check(bool x, string why) { checks++; if (!x) throw new Exception(why); }
foreach (var text in new[] { "", "0", "-1", "1.2", "1e3", "2147483648", "abc" }) Check(!Amount.TryParse(text, out _), "Invalid input " + text);
Check(Amount.TryParse(" 123 ", out var n) && n == 123, "trimmed integer");
Check(Amount.TryParse("2147483647", out n) && n == int.MaxValue, "max integer");
var store = new FakeStore(); var engine = new CreditEngine(store);
var id = Guid.NewGuid().ToString();
var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => engine.Add("test", 20, id)));
Check(results.All(r => r.Success && r.Balance == 120), "duplicate results"); Check(store.Balance == 120 && store.Credits == 1 && store.Saves == 1, "credit exactly once");
Check(!(await engine.Add("test", 21, id)).Success, "changed duplicate payload");
foreach (var amount in new[] { 0, -1, int.MaxValue }) Check(!(await engine.Add("test", amount, Guid.NewGuid().ToString())).Success, "bad amount/overflow");
Check(!(await engine.Add("missing", 10, Guid.NewGuid().ToString())).Success, "unknown profile");
Check(!(await engine.Add("test", 10, "invalid")).Success, "bad request id");
Check(store.Credits == 1, "invalid never mutates");
store.FailSave = true; id = Guid.NewGuid().ToString();
Check(!(await engine.Add("test", 10, id)).Success, "save failure reported");
Check(!(await engine.Add("test", 10, id)).Success && store.Credits == 2, "failed save retry does not credit twice");
var notifyStore = new FakeStore { FailNotify = true }; var notifyEngine = new CreditEngine(notifyStore);
var notifyResult = await notifyEngine.Add("test", 9, Guid.NewGuid().ToString());
Check(notifyStore.Balance == 109 && notifyStore.Saves == 1, "notification failure still persists");
Check(notifyResult.Success, "persisted credit confirmed despite notification failure");
Console.WriteLine($"PASS {checks} FreeTarcoins checks");
class FakeStore : ICreditStore
{
    public int Balance = 100, Credits, Saves; public bool FailSave, FailNotify;
    public int? BalanceFor(string session) => session == "test" ? Balance : null;
    public async Task Credit(string session, int amount) { await Task.Yield(); Balance += amount; Credits++; if (FailNotify) throw new IOException("notification failure"); }
    public Task Save(string session) { Saves++; if (FailSave) throw new IOException("test failure"); return Task.CompletedTask; }
}
