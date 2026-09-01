# Base Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Store.TenantPortal/Store.TenantPortal.csproj", "Store.TenantPortal/"]
RUN dotnet restore "Store.TenantPortal/Store.TenantPortal.csproj"
COPY Store.TenantPortal/ Store.TenantPortal/
WORKDIR "/src/Store.TenantPortal"
RUN dotnet build "Store.TenantPortal.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "Store.TenantPortal.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.TenantPortal.dll"]
