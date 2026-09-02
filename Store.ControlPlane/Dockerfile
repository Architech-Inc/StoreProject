FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Store.ControlPlane/Store.ControlPlane.csproj", "Store.ControlPlane/"]
COPY ["Store.Models/Store.Models.csproj", "Store.Models/"]
RUN dotnet restore "Store.ControlPlane/Store.ControlPlane.csproj"

COPY . .
WORKDIR "/src/Store.ControlPlane"
RUN dotnet build "Store.ControlPlane.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Store.ControlPlane.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 19999
ENV ASPNETCORE_HTTP_PORTS=19999

# Install curl for healthchecks and Docker CLI + Compose plugin for tenant orchestration
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    gnupg \
    lsb-release \
    && mkdir -m 0755 -p /etc/apt/keyrings \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" > /etc/apt/sources.list.d/docker.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends docker-ce-cli docker-compose-plugin \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.ControlPlane.dll"]
