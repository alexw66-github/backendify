FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the solution file
COPY ./*.sln ./

# Copy projects at the root
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

RUN dotnet restore
COPY . .
RUN dotnet build -o /app/build

FROM build AS test
RUN dotnet test -l:"console;verbosity=normal"

FROM test AS publish
RUN dotnet dev-certs https
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/

ENV ASPNETCORE_URLS="http://+:9000"

ENTRYPOINT ["dotnet", "Backendify.Api.dll"]