# .NET Aspire Integration

This project has been enhanced with .NET Aspire integration to provide observability, service discovery, and simplified distributed application development.

## Project Structure

- **NetAspireDemo.AppHost**: The Aspire orchestrator that manages service discovery and configuration
- **NetAspireDemo.ServiceDefaults**: Shared Aspire configurations including OpenTelemetry, health checks, and resilience patterns
- **WeatherApi**: Weather API service with Aspire integration
- **WeatherWeb**: Web frontend with Aspire integration

## Running with Aspire

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop (for containers)

### Run with Aspire Dashboard
```bash
cd src/NetAspireDemo.AppHost
dotnet run
```

This will start the Aspire dashboard at `https://localhost:15001` where you can:
- View service topology and dependencies
- Monitor health checks
- View distributed traces
- Monitor metrics and logs
- See real-time service status

The services will be available at:
- Weather API: http://localhost:5001
- Weather Web: http://localhost:5000

### Features Included

#### Observability
- **OpenTelemetry**: Distributed tracing and metrics
- **Structured Logging**: Centralized log collection
- **Health Checks**: Service health monitoring
- **Metrics**: Application performance metrics

#### Resilience
- **HTTP Resilience**: Automatic retry, circuit breaker, and timeout policies
- **Service Discovery**: Automatic service endpoint resolution

#### Development Experience
- **Aspire Dashboard**: Real-time monitoring and debugging
- **Hot Reload**: Fast development iteration
- **Configuration Management**: Centralized configuration

## Docker Compose Compatibility

The existing Docker Compose setup remains fully functional:

```bash
# Build and run with Docker Compose
docker-compose up --build

# Services will be available at:
# - Weather API: http://localhost:5001
# - Weather Web: http://localhost:5000
```

## Configuration

### Environment Variables
- `OTEL_EXPORTER_OTLP_ENDPOINT`: OpenTelemetry collector endpoint (optional)
- `WEATHER_API_BASE_URL`: Weather API base URL for the web app

### Health Checks
Both services expose health check endpoints:
- `/health`: Overall service health
- `/alive`: Liveness probe

## Development Workflow

1. **Local Development with Aspire**:
   ```bash
   dotnet run --project src/NetAspireDemo.AppHost
   ```

2. **Container Development**:
   ```bash
   docker-compose up --build
   ```

3. **Production Deployment**:
   Use the existing Docker Compose setup for production deployment.

## Monitoring

The Aspire dashboard provides comprehensive monitoring:
- Service dependencies and communication
- Request tracing across services
- Performance metrics
- Error rates and health status
- Resource utilization
