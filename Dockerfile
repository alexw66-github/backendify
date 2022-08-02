FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
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

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN	apk update && \
	apk upgrade && \
	apk add --update nginx && \
	rm -rf /var/cache/apk/*

RUN ln -sf /dev/stdout /var/log/nginx/access.log && \
    ln -sf /dev/stderr /var/log/nginx/error.log
	
COPY nginx.conf /etc/nginx
COPY ./startup.sh .
RUN chmod 755 /app/startup.sh

ARG NGINX_PORT_HTTP=9000
ENV NGINX_PORT_HTTP=${NGINX_PORT_HTTP}

ARG SERVICE_PORT_HTTP=5000
ENV SERVICE_PORT_HTTP=${SERVICE_PORT_HTTP}

ARG ASPNETCORE_URLS="http://+:${SERVICE_PORT_HTTP}"
ENV ASPNETCORE_URLS=${ASPNETCORE_URLS}

VOLUME ["/var/log/nginx", "/tmp"]
EXPOSE ${NGINX_PORT_HTTP}:${NGINX_PORT_HTTP}

ENTRYPOINT ["sh", "/app/startup.sh"]