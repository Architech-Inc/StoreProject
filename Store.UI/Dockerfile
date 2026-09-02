FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Store.UI/Store.UI.csproj", "Store.UI/"]
COPY ["Store.Models/Store.Models.csproj", "Store.Models/"]
RUN dotnet restore "Store.UI/Store.UI.csproj"
COPY . .
WORKDIR "/src/Store.UI"
RUN dotnet build "Store.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Store.UI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StoreUI.dll"]
