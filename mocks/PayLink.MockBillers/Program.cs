// Mock biller servers.
// Phase 1: the service starts and reports health so docker compose can wire it in.
// Phase 2: each biller gets its contract (see the student brief, section 2.4) and
// failure switches (slow answer, error code, duplicate callback) for the hard-case tests.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var billers = new[]
{
    new { Code = "electroco", Service = "Electricity", Protocol = "REST/JSON (sync)", BasePath = "/electroco" },
    new { Code = "aquaco", Service = "Water", Protocol = "SOAP-style XML (slow)", BasePath = "/aquaco" },
    new { Code = "topupnet", Service = "Mobile top-up", Protocol = "REST/JSON (async callback)", BasePath = "/topupnet" },
    new { Code = "netco", Service = "Internet", Protocol = "Daily CSV batch", BasePath = "/netco" },
};

app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/", () => Results.Ok(new { service = "PayLink Mock Billers", billers }));

app.Run();
