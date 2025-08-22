# NetAspireDemo - Weather Application

[![CI/CD Pipeline](https://github.com/pratiksinghlad/NetAspireDemo/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/pratiksinghlad/NetAspireDemo/actions/workflows/ci-cd.yml)

A complete, production-ready demo application showcasing a modern .NET 8 weather dashboard with separate API and web frontend, Docker containerization, and CI/CD pipeline.

## 🏗️ Architecture

This solution demonstrates a microservices architecture with:

- **WeatherApi** - ASP.NET Core Web API with minimal APIs providing weather endpoints
- **WeatherWeb** - Razor Pages frontend consuming the Weather API
- **SharedModels** - Common DTOs shared between projects
- **WeatherApi.Tests** - Unit tests for the API services

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (for containerized deployment)
- [Docker Compose](https://docs.docker.com/compose/)

### Local Development (dotnet run)

1. **Clone the repository**
   ```bash
   git clone https://github.com/pratiksinghlad/NetAspireDemo.git
   cd NetAspireDemo
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

4. **Start the API** (Terminal 1)
   ```bash
   dotnet run --project src/WeatherApi
   ```
   API will be available at http://localhost:5001/swagger

5. **Start the Web App** (Terminal 2)
   ```bash
   dotnet run --project src/WeatherWeb
   ```
   Web app will be available at http://localhost:5000

### Docker Compose (Recommended)

Run the complete application stack with a single command:

```bash
docker compose up --build
```

This will:
- Build both Docker images
- Start the API on http://localhost:5001
- Start the Web app on http://localhost:5000
- Set up proper networking between containers

To run in background:
```bash
docker compose up -d --build
```

To stop:
```bash
docker compose down
```

## 📱 Using the Application

1. Open your browser to http://localhost:5000
2. Enter a city name (try: New York, London, Tokyo, Sydney, Berlin)
3. Click "Get Weather" to see:
   - Current weather conditions
   - 5-day forecast
   - Temperature in both Celsius and Fahrenheit

## 🛠️ Development

### Project Structure

```
NetAspireDemo/
├── src/
│   ├── WeatherApi/          # ASP.NET Core Web API
│   │   ├── Services/        # Business logic
│   │   ├── Dockerfile       # API container definition
│   │   └── Program.cs       # API configuration
│   ├── WeatherWeb/          # Razor Pages frontend
│   │   ├── Pages/           # Razor pages
│   │   ├── Services/        # HTTP client services
│   │   ├── Dockerfile       # Web container definition
│   │   └── Program.cs       # Web app configuration
│   └── SharedModels/        # Common DTOs
├── tests/
│   └── WeatherApi.Tests/    # Unit tests
├── docker-compose.yml       # Multi-container orchestration
├── .github/workflows/       # CI/CD pipeline
└── README.md
```

### API Endpoints

The Weather API provides the following endpoints:

- `GET /api/weather/forecast?city={city}` - 5-day weather forecast
- `GET /api/weather/{city}` - Current weather for city
- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `WEATHER_API_BASE_URL` | Base URL for the Weather API | `http://localhost:5001` |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Development` |

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

The test suite includes:
- Weather service unit tests
- Data validation tests
- Temperature conversion tests
- Edge case handling

## 🐳 Docker

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

# Restart specific service
docker compose restart weatherweb

# Remove all containers and networks
docker compose down --volumes --remove-orphans
```

### Logs

Check application logs:
- **Local**: Console output from `dotnet run`
- **Docker**: `docker compose logs [service-name]`
- **Azure**: Azure portal > Web App > Log stream

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes and add tests
4. Ensure tests pass: `dotnet test`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Docker Documentation](https://docs.docker.com/)
- [Azure Web Apps](https://docs.microsoft.com/azure/app-service/)
- [GitHub Actions](https://docs.github.com/actions)
