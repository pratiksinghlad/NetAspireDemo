# Project Assumptions

## Technology Stack
- **Used .NET 8 instead of .NET 9**: The environment has .NET 8 SDK available, which provides all the same capabilities and structure as requested.
- **Container Registry**: Configured for GitHub Container Registry (ghcr.io) by default, but can be easily switched to Docker Hub or Azure Container Registry.

## Architecture Decisions
- **Minimal APIs**: Used ASP.NET Core minimal APIs for the Weather API as requested, providing clean, simple endpoints.
- **Shared Models**: Created a separate SharedModels project for DTOs to maintain consistency between API and Web projects.
- **HttpClient Factory**: Used typed HttpClient with dependency injection for API communication from the web frontend.

## Docker Configuration
- **Base Images**: Using official Microsoft .NET images (`mcr.microsoft.com/dotnet/sdk:8.0` for build, `mcr.microsoft.com/dotnet/aspnet:8.0` for runtime).
- **Security**: Docker containers run as non-root user for security.
- **Port Mapping**: API runs on port 5001, Web runs on port 5000 locally. In Docker, both use port 8080 internally.

## Local Development
- **CORS**: Enabled for local development between frontend (port 5000) and API (port 5001).
- **Environment Variables**: Web app reads API base URL from environment variable `WEATHER_API_BASE_URL`.

## Data Strategy
- **Mock Data**: Weather service generates deterministic mock data with city-specific temperature biases for consistent demo experience.
- **No External APIs**: All weather data is generated locally to avoid external dependencies and API keys.

## CI/CD
- **GitHub Actions**: Configured for GitHub Container Registry by default.
- **Azure Deployment**: Provided example configuration (commented) for Azure Web App deployment.
- **Branch Strategy**: Builds images on main and develop branches, deploys only from main.

## Testing
- **Unit Tests**: Focused on Weather API service logic with comprehensive test coverage.
- **Integration Tests**: Not included to keep the demo minimal, but the architecture supports adding them.

## Production Considerations
- **Health Checks**: Added to both services for container orchestration.
- **Logging**: Using built-in ASP.NET Core logging with configurable levels.
- **Error Handling**: Graceful degradation when API is unavailable.

## Azure Deployment Notes
- The workflow includes examples for both Azure Web App publish profile and Azure CLI deployment methods.
- For production deployment, you would need to configure Azure resources and secrets as described in the README.