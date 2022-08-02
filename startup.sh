#!/bin/bash
set -x

echo NGINX_PORT_HTTP=${NGINX_PORT_HTTP}
echo SERVICE_PORT_HTTP=${SERVICE_PORT_HTTP}
echo ASPNETCORE_URLS=${ASPNETCORE_URLS}

export ASPNETCORE_URLS=${ASPNETCORE_URLS}

nginx #nginx -g 'daemon off;'
dotnet "Backendify.Api.dll"