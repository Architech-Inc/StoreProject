FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Store.API/Store.API.csproj", "Store.API/"]
COPY ["Store.Models/Store.Models.csproj", "Store.Models/"]
COPY ["Store.DbServices/Store.DbServices.csproj", "Store.DbServices/"]
RUN dotnet restore "Store.API/Store.API.csproj"
COPY . .
WORKDIR "/src/Store.API"
RUN dotnet build "Store.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Store.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.API.dll"]
