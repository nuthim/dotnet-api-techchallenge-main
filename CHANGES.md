# Changes from Original

See [ISSUES.md](ISSUES.md) for the problems identified in the original codebase.

## Architecture Note

This API is an **aggregation/orchestration layer**, not a product microservice. It does not own data — it orchestrates calls to the WooliesX resource API and applies business logic (sorting, recommendations) on top. Any consumer can call it: another backend service, a frontend, a mobile app, or a CLI tool.

The three downstream endpoints (products, shopperHistory, trolleyCalculator) are currently served by the same WooliesX API but are treated as independent services in the client design — each has its own interface, implementation, HttpClient registration, and resilience configuration. This makes the code ready for a future where they are split into separate services.

---

## What Changed

- **.NET 10**, minimal hosting, file-scoped namespaces
- **Clean Architecture** — split into Domain, Services, Api, Tests
- **`IHttpClientFactory`** with resilience pipeline (retry, circuit breaker, timeout, rate limiting per client)
- **Strategy pattern** for sorting with assembly scanning auto-discovery
- **`ITokenProvider` abstraction** + `TokenDelegatingHandler` for auth
- **`ILogger`** replacing `Console.WriteLine`
- **`System.Text.Json`** replacing Newtonsoft.Json
- **Records** replacing `ValueObject` base class
- **`SortOption` enum** with `[EnumDataType]` validation replacing raw strings
- **Proper DI** — constructor injection, convention-based registration
- **`ProductService`** — orchestration logic extracted from controller
- **RESTful routes** — `GET /products?sortBy=`, `POST /trolley/total`
- **`[ApiController]`** on all controllers
- **`IExceptionHandler`** + ProblemDetails for structured error responses
- **Typed `TrolleyRequest`** model replacing raw JSON passthrough
- **`CancellationToken`** required throughout
- **Config-driven** — URLs, timeouts, retries all in `appsettings.json`
- **Proper health checks** — `/health/live` (liveness) and `/health/ready` (readiness with downstream dependency check) using ASP.NET Core health check middleware
- **OpenAPI + Scalar** — built-in `Microsoft.AspNetCore.OpenApi` for spec generation, Scalar UI for interactive documentation (replaces Swashbuckle)
- **Inbound rate limiting**
- **Central Package Management**, `Directory.Build.props`, `.gitignore`, `NuGet.config`

---

## TODO

### Code
- [ ] `Product.Price` — change `double` to `decimal` for monetary values

### Security
- [ ] Authentication (JWT bearer / API key)
- [ ] Input validation (FluentValidation or DataAnnotations)
- [ ] Security headers (X-Frame-Options, X-Content-Type-Options, HSTS)
- [ ] CORS policy

### Observability
- [ ] Structured logging (Serilog / OpenTelemetry)
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Metrics endpoint (Prometheus)
- [ ] Correlation ID middleware

### Resilience
- [ ] Graceful shutdown handling
- [ ] Distributed rate limiting (Redis) for multi-instance deployments

### API Design
- [ ] API versioning
- [ ] Pagination
- [ ] Content negotiation
- [ ] ETag / conditional responses
- [ ] Response caching

### Operational
- [ ] Containerisation (Dockerfile, docker-compose)
- [ ] Per-environment config (`appsettings.{env}.json`)
- [ ] Readiness vs liveness health checks (downstream probes)
- [ ] CI/CD pipeline (GitHub Actions)

### Testing
- [ ] Integration tests with `WebApplicationFactory` and stubbed dependencies
- [ ] Code coverage gate
