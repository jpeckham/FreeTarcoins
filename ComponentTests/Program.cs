using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Reflection;
using System.Runtime.Loader;
using FreeTarcoins;
using HarmonyLib;
using SPTarkov.Server.Web.Pages.Shop.Components;
var path = Environment.GetEnvironmentVariable("SPT_TEST_RUNTIME") ?? @"C:\sptarkov\SPTarkov 5.0.0-BE-1.3.0.0.48484\SPT_Runtime";
AssemblyLoadContext.Default.Resolving += (context, name) => File.Exists(Path.Combine(path, name.Name + ".dll")) ? context.LoadFromAssemblyPath(Path.Combine(path, name.Name + ".dll")) : null;
await Run();
static async Task Run()
{
    int checks = 0; void Check(bool ok, string message) { checks++; if (!ok) throw new Exception(message); }
    var nav = new TestNavigation(); var js = new TestJS(); int credits = 0, balance = 246;
    ShopPatch.Credit = (session, amount, id) => { Check(session == "6ae3d244a61c030de8d04fc1", "page session bound"); credits++; balance += amount; return Task.FromResult(new CreditResult(true, balance, "Added")); };
    var services = new ServiceCollection().AddLogging().AddSingleton<NavigationManager>(nav).AddSingleton<IJSRuntime>(js).BuildServiceProvider();
    new Harmony("freetarcoins.component.tests").Patch(AccessTools.Method(typeof(ShopHeader), "BuildRenderTree"), prefix: new HarmonyMethod(typeof(ShopPatch), nameof(ShopPatch.Render)));
    using var renderer = new TestRenderer(services, services.GetRequiredService<ILoggerFactory>());
    await renderer.Dispatcher.InvokeAsync(async () =>
    {
        int parent = renderer.Add(typeof(ShopHeader));
        await renderer.Render(parent, ParameterView.FromDictionary(new Dictionary<string, object?> { { "Balance", balance } }));
        int root = renderer.Child(parent, typeof(FreeTarcoinsHeader));
        Check(root >= 0, "real ShopHeader patch renders our component");
        Check(renderer.Text(root).Contains("246"), "original balance displayed");
        var open = renderer.Event(root, "aria-label", "Add free Tarcoins", "onclick");
        await renderer.DispatchEventAsync(open, null, new MouseEventArgs());
        Check(renderer.Text(root).Contains("Amount to add"), "balance click opens form");
        Check(js.Calls.Any(c => c.Contains("focus")), "input focus requested on open");
        await renderer.DispatchEventAsync(renderer.EventOnElement(root, "section", "onkeydown"), null, new KeyboardEventArgs { Key = "Escape" });
        Check(!renderer.Text(root).Contains("Amount to add"), "escape cancels");
        await renderer.DispatchEventAsync(renderer.Event(root, "aria-label", "Add free Tarcoins", "onclick"), null, new MouseEventArgs());
        var input = renderer.Event(root, "id", "freetarcoins-amount", "oninput");
        await renderer.DispatchEventAsync(input, null, new ChangeEventArgs { Value = "0" });
        var submit = renderer.EventOnElement(root, "form", "onsubmit");
        await renderer.DispatchEventAsync(renderer.EventOnElement(root, "form", "onsubmit"), null, EventArgs.Empty);
        Check(renderer.Text(root).Contains("positive whole"), "invalid feedback"); Check(credits == 0, "invalid amount does not credit");
        await renderer.DispatchEventAsync(renderer.Event(root, "id", "freetarcoins-amount", "oninput"), null, new ChangeEventArgs { Value = "17" });
        await renderer.DispatchEventAsync(renderer.EventOnElement(root, "form", "onsubmit"), null, EventArgs.Empty);
        Check(credits == 1 && balance == 263, "valid form credits once"); Check(nav.Reloads == 1, "shop refresh requested");
    });
    Console.WriteLine($"PASS {checks} rendered Expansion Hub component checks");
}
class TestNavigation : NavigationManager { public int Reloads; public TestNavigation() { Initialize("https://127.0.0.1:6969/", "https://127.0.0.1:6969/shop/6ae3d244a61c030de8d04fc1"); } protected override void NavigateToCore(string uri, bool forceLoad) { if (forceLoad) Reloads++; } }
class TestJS : IJSRuntime { public List<string> Calls = new(); public ValueTask<T> InvokeAsync<T>(string name, object?[]? args) { Calls.Add(name); return ValueTask.FromResult(default(T)!); } public ValueTask<T> InvokeAsync<T>(string name, CancellationToken ct, object?[]? args) => InvokeAsync<T>(name, args); }
class TestRenderer : Renderer
{
    public TestRenderer(IServiceProvider services, ILoggerFactory logs) : base(services, logs) { ElementReferenceContext = new WebElementReferenceContext(services.GetRequiredService<IJSRuntime>()); }
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
    protected override Task UpdateDisplayAsync(in RenderBatch batch) => Task.CompletedTask;
    protected override void HandleException(Exception ex) => throw new Exception("Render failure", ex);
    public int Add(Type type) => AssignRootComponentId(InstantiateComponent(type));
    public Task Render(int id, ParameterView p) => RenderRootComponentAsync(id, p);
    public RenderTreeFrame[] Frames(int id) { var f = GetCurrentRenderTreeFrames(id); return f.Array.Take(f.Count).ToArray(); }
    public int Child(int id, Type type) => Frames(id).Single(f => f.FrameType == RenderTreeFrameType.Component && f.ComponentType == type).ComponentId;
    public string Text(int id) => string.Join(" ", Frames(id).Where(f => f.FrameType is RenderTreeFrameType.Text or RenderTreeFrameType.Markup).Select(f => f.FrameType == RenderTreeFrameType.Text ? f.TextContent : f.MarkupContent));
    public ulong Event(int id, string attr, string value, string evt) { var f = Frames(id); for (int i = 0; i < f.Length; i++) if (f[i].FrameType == RenderTreeFrameType.Element) { var attrs = f.Skip(i + 1).TakeWhile(x => x.FrameType == RenderTreeFrameType.Attribute).ToArray(); if (attrs.Any(a => a.AttributeName == attr && a.AttributeValue?.ToString() == value)) return attrs.Single(a => a.AttributeName == evt).AttributeEventHandlerId; } throw new Exception("Missing event " + attr + "=" + value); }
    public ulong EventOnElement(int id, string tag, string evt) { var f = Frames(id); for (int i = 0; i < f.Length; i++) if (f[i].FrameType == RenderTreeFrameType.Element && f[i].ElementName == tag) return f.Skip(i + 1).TakeWhile(x => x.FrameType == RenderTreeFrameType.Attribute).Single(a => a.AttributeName == evt).AttributeEventHandlerId; throw new Exception("Missing element " + tag); }
}
