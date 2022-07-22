FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Backendify.Api/Backendify.Api.csproj", "Backendify.Api/"]
RUN dotnet restore "Backendify.Api/Backendify.Api.csproj"
COPY . .
WORKDIR "/src/Backendify.Api"
RUN dotnet build "Backendify.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Backendify.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backendify.Api.dll"]