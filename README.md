# NetAspireDemo - .NET Aspire Weather Application

[![CI/CD Pipeline](https://github.com/pratiksinghlad/NetAspireDemo/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/pratiksinghlad/NetAspireDemo/actions/workflows/ci-cd.yml)

A complete .NET Aspire demonstration showcasing Microsoft's opinionated, cloud-ready stack for building observable, production-ready, distributed applications with a weather API and web frontend.

## 🌟 What is .NET Aspire?

.NET Aspire is Microsoft's opinionated, cloud-ready stack that provides:

- **🔍 Observability**: Built-in telemetry, logging, and health checks
- **🔧 Orchestration**: Simplified service discovery and configuration management  
- **💪 Resilience**: Automatic retry policies and circuit breakers
- **📊 Dashboard**: Rich development-time dashboard for monitoring your application
- **⚙️ Service Defaults**: Pre-configured settings for common scenarios

## 🆕 .NET 9 & Central Package Management

This project has been migrated to **.NET 9** with **Central Package Management** for improved maintainability:

- **📦 Directory.Packages.props**: Centralized package version management
- **🔧 Directory.Build.props**: Global project settings and properties  
- **🎯 .NET 9 Target Framework**: Latest stable framework with performance improvements
- **📋 Simplified Project Files**: Cleaner `.csproj` files without version specifications

## 🏗️ Project Architecture

```
📁 NetAspireDemo/
├── 📦 Directory.Packages.props          # Central package version management
├── 🔧 Directory.Build.props             # Global MSBuild properties
├── 🎯 src/NetAspireDemo.AppHost/        # Aspire orchestrator (START HERE)
├── ⚙️ src/NetAspireDemo.ServiceDefaults/ # Shared Aspire configurations
├── 🌐 src/WeatherApi/                   # Weather API service
├── 🖥️ src/WeatherWeb/                   # Web frontend
├── 📦 src/SharedModels/                 # Common data models
└── 🧪 tests/WeatherApi.Tests/           # Unit tests
```

## 🔴 **Redis Caching Integration**

This project includes **Redis caching** powered by .NET Aspire for improved performance and scalability:

### **🚀 Features**

- **⚡ In-Memory Caching**: Weather data cached for faster response times
- **🔗 Aspire Integration**: Redis container managed by .NET Aspire AppHost
- **🐳 Docker Support**: Redis included in Docker Compose for production
- **🧪 Testcontainers**: Integration tests using Redis Testcontainers
- **🛡️ SOLID Principles**: Clean architecture with proper dependency injection

### **⚙️ How It Works**

1. **WeatherApi** requests are cached in Redis:
   - Forecast data: cached for 10 minutes
   - Current weather: cached for 5 minutes
   - Cache keys: `forecast:{city}` and `current:{city}`

2. **Cache Service** (`ICacheService`):
   - Generic interface for caching operations
   - Redis implementation using `IDistributedCache`
   - Automatic JSON serialization/deserialization
   - Error handling for corrupted cache data

3. **Aspire Orchestration**:
   - Redis container automatically started with AppHost
   - Connection string managed by Aspire service discovery
   - Health checks for Redis availability

### **🧪 Testing**

Comprehensive integration tests using **Testcontainers**:

```bash
# Run Redis integration tests
dotnet test tests/WeatherApi.Tests/ --filter "Redis"
```

**Test Coverage:**
- ✅ Cache hit/miss scenarios
- ✅ Data expiration behavior  
- ✅ Concurrent access patterns
- ✅ Error handling (corrupted data)
- ✅ Cache key validation
- ✅ Performance consistency

### **🔧 Local Development**

**With Aspire (Recommended):**
```bash
dotnet run --project src/NetAspireDemo.AppHost
# Redis automatically started and configured
```

**With Docker Compose:**
```bash
docker compose up --build
# Includes Redis container with health checks
```

**Manual Redis Setup:**
```bash
# If running services individually
docker run -d -p 6379:6379 redis:7-alpine
dotnet run --project src/WeatherApi
```

## 🐳 **Redis Container Setup & Testing Guide**

### **🚀 Running Redis for Development**

#### **Option 1: Automatic with .NET Aspire (Recommended)**
```bash
# Redis is automatically managed - just run the AppHost
dotnet run --project src/NetAspireDemo.AppHost
```
✅ **Benefits:**
- Redis container automatically started and stopped
- Connection strings managed by Aspire
- Health checks and monitoring included
- View Redis status in Aspire Dashboard

#### **Option 2: Manual Redis Container**
```bash
# Start Redis container manually
docker run -d \
  --name redis-cache \
  -p 6379:6379 \
  redis:7-alpine

# Verify Redis is running
docker ps | grep redis

# Test Redis connection
docker exec -it redis-cache redis-cli ping
# Should return: PONG

# Start your services
dotnet run --project src/WeatherApi
dotnet run --project src/WeatherWeb
```

#### **Option 3: Docker Compose (Production-like)**
```bash
# Starts Redis + WeatherApi + WeatherWeb together
docker compose up --build

# View logs for specific service
docker compose logs redis
docker compose logs weatherapi

# Stop all services
docker compose down
```

### **🧪 Running Redis Integration Tests**

#### **Prerequisites for Integration Tests**
1. **Docker Desktop** must be running
2. **Testcontainers** will automatically manage Redis containers for tests

#### **Running Tests with Docker**

**Full Test Suite (Requires Docker):**
```bash
# Make sure Docker Desktop is running first
docker --version

# Run all tests including integration tests
dotnet test tests/WeatherApi.Tests/ --verbosity normal
```

**Unit Tests Only (No Docker Required):**
```bash
# Run only unit tests (skips Docker-dependent integration tests)
dotnet test tests/WeatherApi.Tests/ --filter "FullyQualifiedName!~Integration" --verbosity normal
```

**Redis Integration Tests Only:**
```bash
# Run only Redis-specific integration tests
dotnet test tests/WeatherApi.Tests/ --filter "FullyQualifiedName~Redis" --verbosity normal
```

#### **What Testcontainers Does**
- 🐳 **Automatic Container Management**: Starts fresh Redis container for each test class
- 🔒 **Isolation**: Each test gets a clean Redis instance  
- 🧹 **Cleanup**: Automatically stops and removes containers after tests
- 🚀 **Fast**: Uses lightweight Redis Alpine image

#### **Test Categories**

| Test Type | Description | Docker Required |
|-----------|-------------|----------------|
| **Unit Tests** | WeatherService with in-memory cache | ❌ No |
| **Redis Cache Tests** | Redis operations with Testcontainers | ✅ Yes |
| **Integration Tests** | End-to-end service testing with Redis | ✅ Yes |

#### **Troubleshooting Test Issues**

**❌ "Docker is not running" Error:**
```bash
# Start Docker Desktop first
# Then run tests again
dotnet test tests/WeatherApi.Tests/
```

**❌ Port Conflicts:**
```bash
# Testcontainers uses random ports automatically
# No manual port management needed
```

**❌ Slow Test Performance:**
```bash
# First run downloads Redis image (one-time)
# Subsequent runs are much faster
docker pull redis:7-alpine  # Pre-download if needed
```

### **🔍 Monitoring Redis in Development**

#### **With .NET Aspire Dashboard**
1. Run `dotnet run --project src/NetAspireDemo.AppHost`
2. Open Aspire Dashboard (auto-opens in browser)
3. Go to **Resources** tab → See Redis container status
4. Go to **Metrics** tab → Monitor cache hit/miss rates

#### **Manual Redis Monitoring**
```bash
# Connect to Redis CLI
docker exec -it redis-cache redis-cli

# Monitor Redis commands in real-time
MONITOR

# Check cache keys
KEYS *

# View specific cached data
GET "forecast:london"
GET "current:newyork"

# Check Redis info
INFO memory
INFO stats
```

#### **Redis Performance Monitoring**
```bash
# Check cache hit ratio
docker exec -it redis-cache redis-cli INFO stats | grep keyspace

# Monitor memory usage
docker exec -it redis-cache redis-cli INFO memory | grep used_memory_human

# View connected clients
docker exec -it redis-cache redis-cli CLIENT LIST
```

### **🚀 Redis Container Best Practices**

#### **Development Environment**
```bash
# Use .NET Aspire for development (recommended)
dotnet run --project src/NetAspireDemo.AppHost

# Or start Redis with data persistence
docker run -d \
  --name redis-dev \
  -p 6379:6379 \
  -v redis-data:/data \
  redis:7-alpine redis-server --appendonly yes
```

#### **Production Environment**
```bash
# Use docker-compose.yml for production deployment
docker compose -f docker-compose.yml up -d

# Or use managed Redis service (Azure Cache for Redis, AWS ElastiCache)
```

#### **Testing Environment**
```bash
# Testcontainers handles everything automatically
dotnet test tests/WeatherApi.Tests/

# No manual Redis setup needed for tests!
```

### **📋 Quick Reference Summary**

| Scenario | Redis Setup | Command |
|----------|-------------|---------|
| **🎯 Development (Recommended)** | Automatic via Aspire | `dotnet run --project src/NetAspireDemo.AppHost` |
| **🔧 Individual Services** | Manual Docker container | `docker run -d -p 6379:6379 redis:7-alpine` |
| **🐳 Production-like** | Docker Compose | `docker compose up --build` |
| **🧪 Testing** | Automatic via Testcontainers | `dotnet test` |

**Key Benefits:**
- ✅ **Aspire**: Zero-config Redis with monitoring
- ✅ **Testcontainers**: Isolated test environments
- ✅ **Docker Compose**: Production-ready setup
- ✅ **Manual Setup**: Full control for development

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (**Required**)
- [Docker Desktop](https://www.docker.com/get-started) (optional, for containers)

### 🎯 **Option 1: Run with .NET Aspire (RECOMMENDED)**

> **⚠️ Note for .NET 10 Preview Users**: If you encounter DCP/Dashboard path errors, this is a known issue with .NET 10 preview versions. Use Option 2 or 3 below while Microsoft stabilizes the Aspire tooling for preview SDKs.

**For .NET 9 SDK Users:**

1. **Clone and build**
   ```bash
   git clone https://github.com/pratiksinghlad/NetAspireDemo.git
   cd NetAspireDemo
   dotnet build
   ```

2. **Start Aspire AppHost** (this starts everything!)
   ```bash
   dotnet run --project src/NetAspireDemo.AppHost
   ```

3. **🎉 Access the applications:**
   - **📊 Aspire Dashboard**: `https://localhost:15001` (monitoring & insights)
   - **🌐 Weather API**: `http://localhost:5001` (auto-detected port)
   - **🖥️ Weather Web**: `http://localhost:5000` (auto-detected port)
   - **🔴 Redis Cache**: Automatically managed by Aspire

**For .NET 10 Preview Users:**
```bash
# Use Option 2 below until Aspire tooling is updated for .NET 10 preview
```

### 🔧 **Option 2: Traditional Development (Individual Services)**

**⚠️ Important:** Option 2 requires manual Redis setup since the services expect Redis to be available.

1. **Start Redis Container** (Terminal 1)
   ```bash
   docker run -d --name redis-dev -p 6379:6379 redis:7-alpine
   ```

2. **Start the API** (Terminal 2)
   ```bash
   dotnet run --project src/WeatherApi
   ```

3. **Start the Web App** (Terminal 3)
   ```bash
   dotnet run --project src/WeatherWeb
   ```

4. **Access the applications:**
   - **🌐 Weather API**: `https://localhost:7001` or `http://localhost:5001`
   - **🖥️ Weather Web**: `https://localhost:7000` or `http://localhost:5000`
   - **🔴 Redis**: `localhost:6379`

5. **Stop services when done:**
   ```bash
   # Stop and remove Redis container
   docker stop redis-dev && docker rm redis-dev
   ```

### 🐳 **Option 3: Docker Compose (Production-like)**

```bash
# Starts Redis + WeatherApi + WeatherWeb together
docker compose up --build
```

**Access:**
- **🖥️ Weather Web**: `http://localhost:5000`
- **🌐 Weather API**: `http://localhost:5001`
- **🔴 Redis**: `localhost:6379`

## 📊 **The .NET Aspire Dashboard - Your Command Center**

When you run the AppHost, the Aspire Dashboard automatically opens and provides:

### 🎯 **Dashboard Features**

| Tab | What You See | Why It's Useful |
|-----|-------------|----------------|
| **🏠 Resources** | All running services, their status, and endpoints | Quick overview of your entire application |
| **📊 Metrics** | Real-time performance metrics, request rates, response times | Monitor application performance |
| **🔍 Traces** | Distributed traces showing request flow across services | Debug issues and understand request paths |
| **📝 Logs** | Centralized logs from all services with correlation | Unified debugging and monitoring |
| **⚙️ Config** | Environment variables and configuration values | Verify service configuration |

### 🚀 **What the Dashboard Adds**

1. **🔍 Real-time Observability**
   - See all services and their health status instantly
   - Monitor CPU, memory, and request metrics live
   - View distributed traces across your microservices

2. **🐛 Enhanced Debugging**
   - Correlate logs across all services
   - Trace requests from frontend to API
   - Identify performance bottlenecks quickly

3. **⚡ Development Velocity**
   - No need to set up separate monitoring tools
   - Instant feedback on code changes
   - Quick access to all service endpoints

4. **🔧 Configuration Management**
   - View all environment variables in one place
   - Validate service configuration
   - Easy service discovery setup

## 🤔 **Why Do We Need .NET Aspire?**

### **Without Aspire** 😤
```bash
# Start API manually
cd src/WeatherApi && dotnet run &

# Start Web manually in another terminal
cd src/WeatherWeb && dotnet run &

# Check logs in multiple places
# Set up monitoring tools separately
# Configure service discovery manually
# Handle health checks individually
```

### **With Aspire** 😍
```bash
# Start everything with one command
dotnet run --project src/NetAspireDemo.AppHost

# Everything is automatically:
# ✅ Started and orchestrated
# ✅ Monitored and observed
# ✅ Health-checked
# ✅ Connected with service discovery
# ✅ Logged and traced
```

### **Key Benefits:**

🎯 **Simplified Development**
- Single command to start your entire distributed application
- Automatic service discovery and configuration
- Built-in health checks and monitoring

📊 **Built-in Observability**
- No need to set up Prometheus, Grafana, or other monitoring tools
- Automatic telemetry collection and visualization
- Distributed tracing out of the box

🔧 **Production Readiness**
- Industry-standard OpenTelemetry integration
- Resilience patterns (retry, circuit breaker)
- Configuration and secrets management

⚡ **Developer Experience**
- Rich dashboard for real-time insights
- Hot reload and fast iteration
- Integrated debugging and diagnostics

## 🧪 **Testing the Application**

### **Running Tests**

#### **📋 All Tests (Requires Docker Desktop)**
```bash
# Make sure Docker Desktop is running
docker --version

# Run complete test suite including Redis integration tests
dotnet test --verbosity normal
```

#### **⚡ Unit Tests Only (No Docker Required)**
```bash
# Fast tests that don't require containers
dotnet test --filter "FullyQualifiedName!~Integration" --verbosity normal
```

#### **🔴 Redis Integration Tests Only**
```bash
# Tests that specifically test Redis caching with Testcontainers
dotnet test --filter "FullyQualifiedName~Redis" --verbosity normal
```

### **Test Categories Explained**

| Test Type | What It Tests | Docker Required | Speed |
|-----------|---------------|----------------|-------|
| **Unit Tests** | Business logic with mocked dependencies | ❌ No | ⚡ Fast |
| **Redis Cache Tests** | Redis operations with real containers | ✅ Yes | 🐌 Slower |
| **Service Integration Tests** | End-to-end with Redis caching | ✅ Yes | 🐌 Slower |

### **Test Scenarios Covered**

✅ **Caching Behavior**
- Cache hit/miss scenarios
- Data expiration (5min current, 10min forecast)
- Cache key generation and validation

✅ **Error Handling**
- Invalid city names and empty inputs
- Corrupted cache data handling
- Redis connection failures

✅ **Performance**
- Concurrent request handling
- Cache consistency across multiple calls
- Memory usage and cleanup

### **Troubleshooting Tests**

**❌ "Docker is not running" Error:**
```bash
# Solution: Start Docker Desktop first
# Then run tests again
dotnet test
```

**❌ Tests are slow on first run:**
```bash
# Normal: First run downloads Redis container image
# Subsequent runs are much faster
# Pre-download Redis image:
docker pull redis:7-alpine
```

**❌ Port conflicts:**
```bash
# Testcontainers automatically handles ports
# No manual configuration needed
# Each test gets isolated Redis instance
```

## 📱 **Using the Application**

1. **Start with Aspire**: `dotnet run --project src/NetAspireDemo.AppHost`
2. **Open Dashboard**: Browser automatically opens to `https://localhost:15001`
3. **Use the App**: Navigate to Weather Web at `http://localhost:5000`
4. **Monitor**: Watch real-time metrics and traces in the dashboard

### **Try These Features:**
- Enter different city names (New York, London, Tokyo, Sydney)
- Watch the request traces flow from Web → API
- Monitor response times and success rates
- View structured logs with correlation IDs

## 🛠️ **Troubleshooting**

### **Common Issue: Aspire DCP/Dashboard Path Error**

**Error Message:**
```
Property CliPath: The path to the DCP executable used for Aspire orchestration is required.
Property DashboardPath: The path to the Aspire Dashboard binaries is missing.
```

**Solution Options:**

1. **Use Stable .NET 9 SDK** (Recommended)
   ```bash
   # Download and install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/9.0
   dotnet --version  # Should show 8.x.x
   dotnet run --project src/NetAspireDemo.AppHost
   ```

2. **Use Individual Services** (Alternative)
   ```bash
   # Terminal 1: Start API
   dotnet run --project src/WeatherApi
   
   # Terminal 2: Start Web (in new terminal)
   dotnet run --project src/WeatherWeb
   ```

3. **Use Docker Compose** (Production-like)
   ```bash
   docker compose up --build
   ```

### **Other Common Issues**

**Build Errors:**
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

**Connection Refused:**
- Ensure API is running before starting Web app
- Check `WeatherApiBaseUrl` configuration
- Verify ports 5000 and 5001 are available

**Docker Issues:**
- Ensure Docker Desktop is running
- Check you're running from repository root
- Verify Docker daemon is accessible

### **Enhanced Project Structure**

```
NetAspireDemo/
├── 🎯 src/NetAspireDemo.AppHost/          # 🌟 Aspire Orchestrator
│   ├── Program.cs                         # Service definitions and dependencies
│   └── Properties/launchSettings.json     # Launch profiles for dashboard
├── ⚙️ src/NetAspireDemo.ServiceDefaults/   # 🌟 Aspire Service Defaults
│   ├── Extensions.cs                      # OpenTelemetry, health checks, resilience
│   └── NetAspireDemo.ServiceDefaults.csproj
├── 🌐 src/WeatherApi/                     # Weather API Service
│   ├── Services/IWeatherService.cs        # Business logic interface
│   ├── Services/WeatherService.cs         # Weather data implementation
│   ├── Program.cs                         # 🌟 Enhanced with Aspire
│   └── Dockerfile                         # Container definition
├── 🖥️ src/WeatherWeb/                     # Web Frontend
│   ├── Pages/Index.cshtml                 # Weather dashboard UI
│   ├── Services/IWeatherApiClient.cs      # API client interface
│   ├── Services/WeatherApiClient.cs       # 🌟 Enhanced with resilience
│   ├── Program.cs                         # 🌟 Enhanced with Aspire
│   └── Dockerfile                         # Container definition
├── 📦 src/SharedModels/                   # Common Models
└── 🧪 tests/WeatherApi.Tests/             # Unit Tests
```

### **🌟 Aspire-Enhanced API Endpoints**

| Endpoint | Description | Aspire Enhancement |
|----------|-------------|-------------------|
| `GET /api/weather/forecast?city={city}` | 5-day forecast | ✅ Traced & Monitored |
| `GET /api/weather/{city}` | Current weather | ✅ Traced & Monitored |
| `GET /health` | Health check | ✅ Enhanced health reporting |
| `GET /alive` | Liveness probe | ✅ Aspire-specific endpoint |
| `GET /swagger` | API documentation | ✅ Available in dashboard |

### **🔧 Configuration & Environment Variables**

| Variable | Description | Default | Aspire Feature |
|----------|-------------|---------|----------------|
| `WeatherApiBaseUrl` | API endpoint for web app | Auto-discovered | ✅ Service Discovery |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry endpoint | Not set | ✅ Telemetry Export |
| `ASPNETCORE_ENVIRONMENT` | Environment setting | Development | ✅ Environment-aware |

## 🧪 **Testing & Quality**

### **Run Tests**

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/WeatherApi.Tests/
```

### **Test Coverage Includes:**

- ✅ Weather service unit tests
- ✅ Data validation tests  
- ✅ Temperature conversion tests
- ✅ HTTP client resilience tests
- ✅ Health check validations
- ✅ Edge case handling

### **Aspire Testing Benefits:**

- 🔍 **Distributed Tracing in Tests**: See how test requests flow through services
- 📊 **Test Metrics**: Monitor test performance and reliability
- 🏥 **Health Check Testing**: Validate service health during test runs

## 🐳 **Docker & Containerization**

### Build Individual Images

Build the API image:
```bash
docker build -f src/WeatherApi/Dockerfile -t weatherapi .
```

Build the Web image:
```bash
docker build -f src/WeatherWeb/Dockerfile -t weatherweb .
```

### Run Individual Containers

Run API container:
```bash
docker run -p 5001:8080 --name weatherapi weatherapi
```

Run Web container (after API is running):
```bash
docker run -p 5000:8080 --name weatherweb -e WEATHER_API_BASE_URL=http://weatherapi:8080 --link weatherapi weatherweb
```

## ☁️ Deployment to Azure

### Option 1: Azure Web App with Docker Compose (Recommended)

1. **Create Azure Web App for Containers**
   ```bash
   az webapp create \
     --resource-group myResourceGroup \
     --plan myAppServicePlan \
     --name myWeatherApp \
     --multicontainer-config-type compose \
     --multicontainer-config-file docker-compose.yml
   ```

2. **Configure Environment Variables**
   ```bash
   az webapp config appsettings set \
     --resource-group myResourceGroup \
     --name myWeatherApp \
     --settings WEATHER_API_BASE_URL=http://weatherapi:8080
   ```

### Option 2: GitHub Actions Automated Deployment

1. **Set up GitHub Secrets**
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: Download from Azure portal
   - Or configure `AZURE_CREDENTIALS` for service principal authentication

2. **Enable Deployment**
   - Uncomment the `deploy-to-azure` job in `.github/workflows/ci-cd.yml`
   - Update the app name and other Azure-specific settings

3. **Deploy**
   - Push to `main` branch triggers automatic deployment

### Option 3: Azure Container Registry + Web App

1. **Push images to Azure Container Registry**
   ```bash
   az acr build --registry myregistry --image weatherapi ./src/WeatherApi
   az acr build --registry myregistry --image weatherweb ./src/WeatherWeb
   ```

2. **Update Web App to use ACR images**
   ```bash
   az webapp config container set \
     --name myWeatherApp \
     --resource-group myResourceGroup \
     --docker-custom-image-name myregistry.azurecr.io/weatherweb:latest
   ```

## 🔧 Configuration

### Required Azure Resources

For production deployment, you'll need:

1. **Resource Group**
2. **App Service Plan** (Linux, supports containers)
3. **Azure Web App** (configured for multi-container)
4. **Azure Container Registry** (optional, for private images)

### Environment Variables for Azure

Set these in your Azure Web App configuration:

```bash
WEATHER_API_BASE_URL=http://weatherapi:8080
ASPNETCORE_ENVIRONMENT=Production
WEBSITES_ENABLE_APP_SERVICE_STORAGE=false
WEBSITES_PORT=8080
```

## 📈 Monitoring and Health Checks

Both services expose health check endpoints:

- API Health: http://localhost:5001/health
- Web Health: http://localhost:5000/health

In Azure, configure health check paths in the Web App settings.

## 🔒 Security Considerations

- All containers run as non-root users
- CORS is configured for development (restrict in production)
- Health checks don't expose sensitive information
- No hardcoded secrets in configuration files

## 🛠️ Troubleshooting

### Common Issues

1. **"Connection refused" errors**
   - Ensure API is running before starting Web app
   - Check `WEATHER_API_BASE_URL` configuration

2. **Docker build fails**
   - Ensure you're running from the repository root
   - Check Docker daemon is running

3. **Tests fail**
   - Run `dotnet restore` to ensure packages are restored
   - Check .NET 8 SDK is installed

### Frequently Used Commands

```bash
# Clean and rebuild everything
dotnet clean && dotnet build

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/WeatherWeb

# View Docker logs
docker compose logs weatherapi
docker compose logs weatherweb

## 🚀 **Getting Started in 30 Seconds**

```bash
# Clone and start with Aspire
git clone https://github.com/pratiksinghlad/NetAspireDemo.git
cd NetAspireDemo
dotnet run --project src/NetAspireDemo.AppHost

# 🎉 That's it! Everything starts automatically:
# ✅ Weather API + Web App running  
# ✅ Aspire Dashboard monitoring
# ✅ Service discovery configured
# ✅ Health checks active
# ✅ Distributed tracing enabled
```

## 🎯 **What Makes This Special**

This isn't just another weather app - it's a showcase of **modern .NET development** with:

- **🌟 .NET Aspire Integration**: Experience the future of .NET distributed applications
- **📊 Built-in Observability**: See your app's behavior in real-time
- **🔧 Service Discovery**: Services find each other automatically  
- **🏥 Health Monitoring**: Know your app's health at a glance
- **⚡ Developer Experience**: Single command to start everything
- **🐳 Container Ready**: Docker support for production deployment

## 🤝 **Contributing**

We welcome contributions! Here's how:

1. **🍴 Fork** the repository
2. **🌟 Create** a feature branch: `git checkout -b feature/amazing-feature`
3. **✍️ Make** your changes and add tests
4. **✅ Ensure** tests pass: `dotnet test`
5. **📝 Commit** your changes: `git commit -m 'Add amazing feature'`
6. **🚀 Push** to the branch: `git push origin feature/amazing-feature`
7. **📬 Open** a Pull Request

### **Areas for Contribution:**
- 🌐 Additional weather data sources
- 🎨 UI/UX improvements
- 🧪 More comprehensive testing
- 📊 Additional monitoring capabilities
- 🔒 Security enhancements
- 📱 Mobile responsiveness

## 📄 **License**

This project is licensed under the MIT License - see the LICENSE file for details.

## 🎓 **Additional Resources**

### **Microsoft Documentation**
- 📖 [.NET Aspire Official Docs](https://learn.microsoft.com/en-us/dotnet/aspire/)
- 🏗️ [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- ☁️ [Azure Container Apps](https://docs.microsoft.com/azure/container-apps/)

### **Learning & Tutorials**
- 🎬 [.NET Aspire Video Series](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU)
- 🛠️ [Aspire Samples Repository](https://github.com/dotnet/aspire-samples)
- 📊 [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)

### **Development Tools**
- 🐳 [Docker Documentation](https://docs.docker.com/)
- 🚀 [GitHub Actions](https://docs.github.com/actions)
- 🔧 [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/)

---

**⭐ Star this repository if you found it helpful!**

*Built with ❤️ using .NET Aspire, ASP.NET Core, and modern development practices.*
