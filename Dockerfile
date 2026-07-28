FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CleanArchitecture.API/CleanArchitecture.API.csproj", "src/CleanArchitecture.API/"]
COPY ["src/CleanArchitecture.Application/CleanArchitecture.Application.csproj", "src/CleanArchitecture.Application/"]
COPY ["src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj", "src/CleanArchitecture.Infrastructure/"]
COPY ["src/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj", "src/CleanArchitecture.Domain/"]
RUN dotnet restore "src/CleanArchitecture.API/CleanArchitecture.API.csproj"
COPY . .
WORKDIR "/src/src/CleanArchitecture.API"
RUN dotnet build "CleanArchitecture.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CleanArchitecture.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitecture.API.dll"]
