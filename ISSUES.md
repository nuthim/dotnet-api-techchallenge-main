# Issues in Original Code

Issues identified in the original codebase that have been addressed during the modernisation effort.

---

## Overall Design

- **No project layering** — controllers, domain models, HTTP clients, interfaces, and infrastructure all lived in a single project
- **Flurl instead of `IHttpClientFactory`** — no delegating handler pipeline, no resilience integration, no handler lifetime management, no DI-native typed clients
- **No resilience** — no retry, circuit breaker, or timeout on any outbound HTTP call
- **Hardcoded URLs and tokens** — downstream API URLs baked in as string fields, API token duplicated as a magic string across multiple files, nothing in configuration
- **`Console.WriteLine` for error logging** — catch blocks used `Console.WriteLine(e)` instead of `ILogger`
- **Newtonsoft.Json** — used `[JsonProperty]` attributes instead of the built-in `System.Text.Json`
- **No `CancellationToken` propagation** — no method accepted or forwarded a cancellation token
- **Hand-rolled `ValueObject` base class** — custom abstract class for equality semantics, replaced by C# records for free
- **No central package management** — no `Directory.Build.props`, no `Directory.Packages.props`, no `NuGet.config`

---

## Startup.cs

- **Old hosting model** — `CreateHostBuilder` + `UseStartup<Startup>` instead of minimal hosting
- **Mixed routing patterns** — `endpoints.MapControllers()` alongside inline `endpoints.Map()` delegates, so some endpoints went through the controller pipeline and others bypassed it entirely
- **No global error handling** — `UseDeveloperExceptionPage()` in Development only, nothing for other environments. Unhandled exceptions returned raw 500 responses
- **API intent, MVC behaviour** — `services.AddControllers()` indicated API intent, but no `[ApiController]` attribute on controllers meant no automatic model validation, no `400` for binding failures, no binding source inference
- **No proper health checks** — `endpoints.Map("/", ...)` returning "Welcome to Product Catalog API" served as a basic liveness check, but there was no readiness check to verify downstream dependencies were reachable
- **No OpenAPI spec or API documentation** — no way for consumers to discover endpoints, request/response shapes, or generate typed clients
- **DI registered but not used** — `ConfigureServices` registered dependencies in the container, but `ProductController` manually `new`'d them up inside the action method

---

## ProductController

- **Dependencies manually instantiated** — `new GetSortedProductQueryHandler(new ProductHttpClient(), new ShopperHistoryHttpClient())` inside the action method, bypassing DI entirely
- **Sort option as raw string** — no enum, no validation, any garbage value silently accepted
- **Unknown sort option silently ignored** — unrecognised values returned the list unsorted with no error
- **`/sort` route is not RESTful** — verb in the URL; products are the resource, sorting is a query modifier
- **Monolithic sort logic** — all sort cases in a single if/else chain in `GetSortedProductQuery.cs`, no strategy pattern, no extensibility
- **`double` for money** — `Product.Price` used `double`, which has floating-point precision issues for monetary values
- **No limit on response size** — `GET /products` returns the entire product list with no cap. Options to address:
  - Pagination (`?page=1&pageSize=20`) — though true pagination needs downstream API support
  - Response compression (GZip/Brotli) — reduces payload size over the wire
  - Streaming (`IAsyncEnumerable<T>`) — avoids buffering the full list in memory
  - Filtering (`?name=...&minPrice=...`) — lets consumers narrow the result set

---

## UserController

- **Endpoint has no real-world purpose** — a service API doesn't have a "name" or return tokens via GET endpoints. Service identity belongs in OAuth client credentials, not a hardcoded response. This endpoint only exists because the tech challenge required candidate identification
- **Token returned in response body** — `GET /user` exposes the API token in the JSON response. Tokens should never be exposed via API endpoints
- **Model defined in controller file** — `UserResponseModel` co-located in the same file as the controller

---

## WooliesXProxy (Trolley)

- **Static class bypassing the controller pipeline** — `WooliesXProxy.TrolleyCalculator` took raw `HttpContext`, bypassing model binding, validation, filters, and content negotiation
- **Raw JSON passthrough** — request body read manually with `StreamReader`, forwarded as-is, response written back as a raw string. No typed model, no validation
- **Wrong content type** — used `application/json-patch+json` for a regular POST
- **No error handling** — downstream failures propagated unhandled
- **`/trolleyTotal` route is not RESTful** — camelCase URL with no resource noun

---

## Models (`Product`, `ShopperHistory`, `ValueObject`)

- **Boxing and heap allocations** — `GetEqualityComponents()` returns `IEnumerable<object>`, so value types like `double Price` and `double Quantity` are boxed on every equality check and hash code computation. Each comparison allocates enumerators and boxed values on the heap
- **GC pressure during sorting** — thousands of comparisons = thousands of unnecessary heap allocations and garbage collection cycles
- **Broken equality on `ShopperHistory`** — `GetEqualityComponents` yields a `List<Product>`, but list equality is by reference, not by contents. Two ShopperHistory objects with identical data would not be equal
- **Flawed `GetHashCode`** — XOR aggregation: identical values cancel out (`a ^ a == 0`), commutative (`a ^ b == b ^ a`), poor distribution. Should use `HashCode.Combine` or multiply-and-add with primes
- **`EqualOperator` / `NotEqualOperator` are dead code** — never wired to `operator ==` / `!=`, so `==` still does reference comparison
- **Unreadable null checks** — `ReferenceEquals(left, null) ^ ReferenceEquals(right, null)` uses XOR on boolean null checks instead of simple pattern matching (`left is null`)
- **All replaced by one-line records** — C# records provide value-based equality with direct field comparisons, no boxing, no enumerators, no allocations, and `==` works as value comparison out of the box

---

## HTTP Clients (`ProductHttpClient`, `ShopperHistoryHttpClient`)

- **Interface and implementation in the same file**
- **Hardcoded URLs as instance fields** — no configuration, no per-environment overrides
- **`catch (Exception e) { Console.WriteLine(e); throw; }`** — logs to console and re-throws, adding no value
- **No `CancellationToken`** — long-running HTTP calls could not be cancelled
- **Hidden data risk on `GetProducts()`** — the product dataset could be large, but there is no timeout, no streaming, no caching, and the entire response is buffered into memory before deserialization. With no retry or fallback, a single slow response or timeout results in a complete failure
- **`GetShopperHistory()` is potentially the bigger risk** — fetches order history for ALL customers, not a single customer. The product catalog is bounded, but shopper history grows unboundedly as more customers order over time. Same buffering and no-timeout issues apply, with a dataset that only gets larger
