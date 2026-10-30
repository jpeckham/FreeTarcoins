using System.Globalization;
namespace FreeTarcoins;

public static class Amount
{
    public static bool TryParse(string? text, out int amount) => int.TryParse(text?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out amount) && amount > 0;
}
public record CreditResult(bool Success, int Balance, string Message);
public interface ICreditStore
{
    int? BalanceFor(string session);
    Task Credit(string session, int amount);
    Task Save(string session);
}
public sealed class CreditEngine(ICreditStore store)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<(string, string), (int, CreditResult)> receipts = new();
    private readonly Queue<(string, string)> order = new();
    public async Task<CreditResult> Add(string session, int amount, string requestId)
    {
        await gate.WaitAsync();
        try
        {
            var balance = store.BalanceFor(session);
            if (balance is null) return new(false, 0, "No active SPT profile.");
            if (!Guid.TryParse(requestId, out var guid)) return new(false, balance.Value, "Invalid request ID.");
            var key = (session, guid.ToString());
            if (receipts.TryGetValue(key, out var prior)) return prior.Item1 == amount ? prior.Item2 : new(false, balance.Value, "Request ID already used for another amount.");
            if (amount <= 0 || balance < 0 || (long)balance + amount > int.MaxValue) return new(false, balance.Value, "Enter a positive whole amount that keeps the balance below 2,147,483,648.");
            CreditResult result;
            try
            {
                try { await store.Credit(session, amount); }
                catch { if (store.BalanceFor(session) != (long)balance + amount) throw; }
                // CreditAsync changes the balance before sending notifications. Persist even if delivery failed.
                await store.Save(session);
                result = new(true, store.BalanceFor(session)!.Value, $"Added {amount:N0} Tarcoins.");
            }
            catch
            {
                // A notification or save may fail after the balance has changed. Never repeat this request.
                result = new(false, store.BalanceFor(session) ?? balance.Value, "Could not confirm the credit/save. Check your balance before adding again.");
            }
            receipts[key] = (amount, result); order.Enqueue(key);
            while (order.Count > 1024) receipts.Remove(order.Dequeue());
            return result;
        }
        finally { gate.Release(); }
    }
}
