#!/bin/bash
set -x

nginx #nginx -g 'daemon off;'
dotnet "Backendify.Api.dll"