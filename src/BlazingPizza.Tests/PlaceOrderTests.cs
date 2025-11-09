using Microsoft.Playwright;

namespace BlazingPizza.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlaceOrderTests
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;

    private const string BaseUrl = "https://localhost:7107";

    [SetUp]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 800
        });

        var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "videos");

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            RecordVideoDir = videoPath,
            RecordVideoSize = new RecordVideoSize
            {
                Width = 1280,
                Height = 720
            }
        });

        _page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (_page.Video != null)
        {
            var path = await _page.Video.PathAsync();
            TestContext.WriteLine($"Video saved to: {path}");
        }

        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Test]
    public async Task Not_Logged_In_User_ConfigurePizza_OrderDetails_And_DeliveryTracking()
    {
        // Home
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Choose pizza
        await _page.GetByText("Buffalo chicken", new() { Exact = false }).ClickAsync();

        // Click "Order >" (button)
        await _page.GetByRole(AriaRole.Button, new() { Name = "Order >" }).ClickAsync();

        // If not logged in, click Login and sign in
        var loginLink = _page.GetByRole(AriaRole.Link, new() { Name = "Log in" });
        if (await loginLink.CountAsync() > 0)
        {
            await loginLink.First.ClickAsync();

            // Email: try placeholder, type=email, common name/id, or autocomplete
            var email = _page.Locator("input[placeholder*='@' i], input[type='email'], input[name='email' i], input[id*='email' i], input[autocomplete='username']").First;

            // Wait for it to appear (or throw with a clean error)
            await email.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
            await email.FillAsync("john.doe@gmail.com");

            // Password: type=password or common name/id patterns
            var pwd = _page.Locator("input[type='password'], input[name*='pass' i], input[id*='pass' i], input[autocomplete='current-password']").First;
            await pwd.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
            await pwd.FillAsync("John@1234");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // (Back on menu) choose pizza again if needed
        // If your app returns to menu after login, reselect the same card:
        var pizzaAgain = _page.GetByText("Buffalo chicken", new() { Exact = false });
        if (await pizzaAgain.CountAsync() > 0)
            await pizzaAgain.First.ClickAsync();

        // Adjust size — target actual range input (avoid role=slider)
        var sizeSlider = _page.Locator("input[type='range']").First;
        await sizeSlider.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await sizeSlider.EvaluateAsync(
            @"e => { e.value = '13'; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }"
        );

        // Select quantity/toppings — use a real <select>
        var select = _page.Locator("select").First;
        await select.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await select.SelectOptionAsync("2");

        // Order button (summary)
        await _page.GetByRole(AriaRole.Button, new() { Name = "Order >" }).ClickAsync();

        // Some builds use a link with the same label; click it only if present
        var orderLink = _page.GetByRole(AriaRole.Link, new() { Name = "Order >" });
        if (await orderLink.CountAsync() > 0)
            await orderLink.First.ClickAsync();

        // Checkout form — prefer labels, then placeholders; avoid CSS nth-child
        ILocator NameField() =>
            _page.GetByLabel("Name", new() { Exact = false }).Or(_page.GetByPlaceholder("Name"));
        ILocator AddressLine1Field() =>
            _page.GetByLabel("Line 1", new() { Exact = false }).Or(_page.GetByPlaceholder("Line1"));
        ILocator CityField() =>
            _page.GetByLabel("City", new() { Exact = false }).Or(_page.GetByPlaceholder("City"));
        ILocator RegionField() =>
            _page.GetByLabel("Region", new() { Exact = false }).Or(_page.GetByPlaceholder("Region"));
        ILocator PostalField() =>
            _page.GetByLabel("Postal Code", new() { Exact = false }).Or(_page.GetByPlaceholder("Postal Code"));

        await NameField().FillAsync("Layan");
        await AddressLine1Field().FillAsync("123 Moon St");
        await CityField().FillAsync("Moon");
        await RegionField().FillAsync("Planet");
        await PostalField().FillAsync("12345");

        // Place order
        await _page.GetByRole(AriaRole.Button, new() { Name = "Place order" }).ClickAsync();

        // Order status assertions (tolerant to casing/spacing)
        await Assertions.Expect(_page.GetByText("Status: Preparing", new() { Exact = false })).ToBeVisibleAsync();

        // Final screenshot for your report
        Directory.CreateDirectory("TestResults/screens");
        await _page.ScreenshotAsync(new()
        {
            Path = $"TestResults/screens/{TestContext.CurrentContext.Test.Name}.png",
            FullPage = true
        });
    }

    [Test]
    public async Task Logged_In_User_ConfigurePizza_OrderDetails_And_DeliveryTracking()
    {
        // Precondition: user is logged in before starting the Place Order flow
        await EnsureLoggedInAsync();

        // Home
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Choose pizza
        await _page.GetByText("Buffalo chicken", new() { Exact = false }).ClickAsync();

        // Click "Order >" (button)
        await _page.GetByRole(AriaRole.Button, new() { Name = "Order >" }).ClickAsync();

        // If not logged in, click Login and sign in
        var loginLink = _page.GetByRole(AriaRole.Link, new() { Name = "Log in" });
        if (await loginLink.CountAsync() > 0)
        {
            await loginLink.First.ClickAsync();

            // Email: try placeholder, type=email, common name/id, or autocomplete
            var email = _page.Locator("input[placeholder*='@' i], input[type='email'], input[name='email' i], input[id*='email' i], input[autocomplete='username']").First;

            // Wait for it to appear (or throw with a clean error)
            await email.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
            await email.FillAsync("john.doe@gmail.com");

            // Password: type=password or common name/id patterns
            var pwd = _page.Locator("input[type='password'], input[name*='pass' i], input[id*='pass' i], input[autocomplete='current-password']").First;
            await pwd.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
            await pwd.FillAsync("John@1234");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // (Back on menu) choose pizza again if needed
        // If your app returns to menu after login, reselect the same card:
        var pizzaAgain = _page.GetByText("Buffalo chicken", new() { Exact = false });
        if (await pizzaAgain.CountAsync() > 0)
            await pizzaAgain.First.ClickAsync();

        // Adjust size — target actual range input (avoid role=slider)
        var sizeSlider = _page.Locator("input[type='range']").First;
        await sizeSlider.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await sizeSlider.EvaluateAsync(
            @"e => { e.value = '13'; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }"
        );

        // Select quantity/toppings — use a real <select>
        var select = _page.Locator("select").First;
        await select.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await select.SelectOptionAsync("2");

        // Order button (summary)
        await _page.GetByRole(AriaRole.Button, new() { Name = "Order >" }).ClickAsync();

        // Some builds use a link with the same label; click it only if present
        var orderLink = _page.GetByRole(AriaRole.Link, new() { Name = "Order >" });
        if (await orderLink.CountAsync() > 0)
            await orderLink.First.ClickAsync();

        // Checkout form — prefer labels, then placeholders; avoid CSS nth-child
        ILocator NameField() =>
            _page.GetByLabel("Name", new() { Exact = false }).Or(_page.GetByPlaceholder("Name"));
        ILocator AddressLine1Field() =>
            _page.GetByLabel("Line 1", new() { Exact = false }).Or(_page.GetByPlaceholder("Line1"));
        ILocator CityField() =>
            _page.GetByLabel("City", new() { Exact = false }).Or(_page.GetByPlaceholder("City"));
        ILocator RegionField() =>
            _page.GetByLabel("Region", new() { Exact = false }).Or(_page.GetByPlaceholder("Region"));
        ILocator PostalField() =>
            _page.GetByLabel("Postal Code", new() { Exact = false }).Or(_page.GetByPlaceholder("Postal Code"));

        await NameField().FillAsync("Layan");
        await AddressLine1Field().FillAsync("123 Moon St");
        await CityField().FillAsync("Moon");
        await RegionField().FillAsync("Planet");
        await PostalField().FillAsync("12345");

        // Place order
        await _page.GetByRole(AriaRole.Button, new() { Name = "Place order" }).ClickAsync();

        // Order status assertions (tolerant to casing/spacing)
        await Assertions.Expect(_page.GetByText("Status: Preparing", new() { Exact = false })).ToBeVisibleAsync();

        // Final screenshot for your report
        Directory.CreateDirectory("TestResults/screens");
        await _page.ScreenshotAsync(new()
        {
            Path = $"TestResults/screens/{TestContext.CurrentContext.Test.Name}.png",
            FullPage = true
        });
    }

    // Helper
    private async Task EnsureLoggedInAsync()
    {
        // Go to home
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If we *don't* see "Log in", assume the user is already authenticated
        var loginLink = _page.GetByRole(AriaRole.Link, new() { Name = "Log in" });
        if (await loginLink.CountAsync() == 0)
        {
            TestContext.WriteLine("User already logged in. Skipping login.");
            return;
        }

        // Otherwise, perform login once
        await loginLink.First.ClickAsync();

        // Email: try placeholder, type=email, common name/id, or autocomplete
        var email = _page.Locator("input[placeholder*='@' i], input[type='email'], input[name='email' i], input[id*='email' i], input[autocomplete='username']").First;

        await email.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
        await email.FillAsync("john.doe@gmail.com");

        // Password: type=password or common name/id patterns
        var pwd = _page.Locator("input[type='password'], input[name*='pass' i], input[id*='pass' i], input[autocomplete='current-password']").First;
        await pwd.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
        await pwd.FillAsync("John@1234");

        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        TestContext.WriteLine("User successfully logged in.");
    }
}