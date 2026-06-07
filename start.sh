#!/bin/sh
PORT="${PORT:-8080}"
echo "SubliSport: iniciando en puerto ${PORT}"
export ASPNETCORE_HTTP_PORTS="${PORT}"
unset ASPNETCORE_URLS
exec dotnet SubliSport.Web.dll
