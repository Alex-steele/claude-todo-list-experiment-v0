using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Layout;
using Xunit;

namespace TodoApp.Tests.Components.Layout;

public class MainLayoutTests : BunitContext
{
    private static BunitContext CreateBunitContext(bool darkModePreference = false)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("todoApp.saveDarkModePreference", _ => true);
        ctx.JSInterop
            .Setup<bool>("todoApp.getDarkModePreference")
            .SetResult(darkModePreference);
        return ctx;
    }

    private static IRenderedComponent<MainLayout> RenderLayout(BunitContext ctx)
    {
        ctx.Render<MudPopoverProvider>();
        return ctx.Render<MainLayout>(p => p.Add(m => m.Body, builder => { }));
    }

    [Fact]
    public void DarkModeToggleButton_IsRendered()
    {
        var ctx = CreateBunitContext();
        var cut = RenderLayout(ctx);

        Assert.Contains("dark-mode-toggle", cut.Markup);
    }

    [Fact]
    public void DarkModeToggleButton_InitialTitle_IsSwitchToDarkMode()
    {
        var ctx = CreateBunitContext(darkModePreference: false);
        var cut = RenderLayout(ctx);

        var toggleBtn = cut.Find(".dark-mode-toggle");
        Assert.Equal("Switch to dark mode", toggleBtn.GetAttribute("title"));
    }

    [Fact]
    public void DarkModeToggleButton_AfterClick_TitleChangesToSwitchToLightMode()
    {
        var ctx = CreateBunitContext();
        var cut = RenderLayout(ctx);

        cut.Find(".dark-mode-toggle").Click();

        var toggleBtn = cut.Find(".dark-mode-toggle");
        Assert.Equal("Switch to light mode", toggleBtn.GetAttribute("title"));
    }

    [Fact]
    public void DarkModeToggleButton_ClickTwice_ReturnsToDarkModeTitle()
    {
        var ctx = CreateBunitContext();
        var cut = RenderLayout(ctx);

        cut.Find(".dark-mode-toggle").Click();
        cut.Find(".dark-mode-toggle").Click();

        var toggleBtn = cut.Find(".dark-mode-toggle");
        Assert.Equal("Switch to dark mode", toggleBtn.GetAttribute("title"));
    }

    [Fact]
    public async Task DarkMode_LoadsPreferenceFromJs_OnFirstRender()
    {
        var ctx = CreateBunitContext(darkModePreference: true);
        var cut = RenderLayout(ctx);

        // After first render the JS interop loads the preference
        await cut.WaitForAssertionAsync(() =>
        {
            var toggleBtn = cut.Find(".dark-mode-toggle");
            Assert.Equal("Switch to light mode", toggleBtn.GetAttribute("title"));
        });
    }

    [Fact]
    public async Task DarkMode_Toggle_CallsSavePreference()
    {
        var ctx = CreateBunitContext();
        var cut = RenderLayout(ctx);

        cut.Find(".dark-mode-toggle").Click();

        // Verify saveDarkModePreference was invoked
        var saveInvocations = ctx.JSInterop.Invocations["todoApp.saveDarkModePreference"];
        Assert.NotEmpty(saveInvocations);
        Assert.Equal(true, saveInvocations[0].Arguments[0]);
    }
}
