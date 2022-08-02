#!/bin/bash
set -x

echo NGINX_PORT_HTTP=${NGINX_PORT_HTTP}
echo SERVICE_PORT_HTTP=${SERVICE_PORT_HTTP}

export ASPNETCORE_URLS="http://+:${SERVICE_PORT_HTTP}"

nginx #nginx -g 'daemon off;'
dotnet "Backendify.Api.dll" $@