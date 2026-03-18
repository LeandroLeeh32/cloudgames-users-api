# STAGE 1 - BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["CloudGames.Users.API/CloudGames.Users.API.csproj", "CloudGames.Users.API/"]
COPY ["CloudGames.Users.Application/CloudGames.Users.Application.csproj", "CloudGames.Users.Application/"]
COPY ["CloudGames.Users.Domain/CloudGames.Users.Domain.csproj", "CloudGames.Users.Domain/"]
COPY ["CloudGames.Users.Infrastructure/CloudGames.Users.Infrastructure.csproj", "CloudGames.Users.Infrastructure/"]

RUN dotnet restore "CloudGames.Users.API/CloudGames.Users.API.csproj"

COPY . .

WORKDIR "/src/CloudGames.Users.API"

RUN dotnet publish -c Release -o /app/publish


# STAGE 2 - RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

# PARA SQLITE
RUN mkdir -p /app/Data

EXPOSE 8080

ENTRYPOINT ["dotnet", "CloudGames.Users.API.dll"]