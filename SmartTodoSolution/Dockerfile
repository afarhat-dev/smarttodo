# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["source/ui/StdApi/StdApi.csproj", "source/ui/StdApi/"]
COPY ["source/core/SmartTodo.Application/SmartTodo.Application.csproj", "source/core/SmartTodo.Application/"]
COPY ["source/infrastructure/SmartTodo.Infrastructure/SmartTodo.Infrastructure.csproj", "source/infrastructure/SmartTodo.Infrastructure/"]
COPY ["source/core/SmartTodo.Domain/SmartTodo.Domain.csproj", "source/core/SmartTodo.Domain/"]

# Restore dependencies
RUN dotnet restore "source/ui/StdApi/StdApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/source/ui/StdApi"
RUN dotnet build "StdApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "StdApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "StdApi.dll"]
