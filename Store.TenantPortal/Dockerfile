FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Store.TenantPortal/Store.TenantPortal.csproj", "Store.TenantPortal/"]
RUN dotnet restore "Store.TenantPortal/Store.TenantPortal.csproj"
COPY Store.TenantPortal/ Store.TenantPortal/
WORKDIR "/src/Store.TenantPortal"
RUN dotnet build "Store.TenantPortal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Store.TenantPortal.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 19090
ENV ASPNETCORE_HTTP_PORTS=19090

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.TenantPortal.dll"]
