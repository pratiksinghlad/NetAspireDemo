# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/WeatherWeb/WeatherWeb.csproj", "src/WeatherWeb/"]
COPY ["src/SharedModels/SharedModels.csproj", "src/SharedModels/"]

# Restore dependencies
RUN dotnet restore "src/WeatherWeb/WeatherWeb.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/WeatherWeb"
RUN dotnet build "WeatherWeb.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WeatherWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WeatherWeb.dll"]