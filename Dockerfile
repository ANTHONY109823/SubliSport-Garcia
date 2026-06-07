## Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/SubliSport.Domain/SubliSport.Domain.csproj SubliSport.Domain/
COPY src/SubliSport.Infrastructure/SubliSport.Infrastructure.csproj SubliSport.Infrastructure/
COPY src/SubliSport.Web/SubliSport.Web.csproj SubliSport.Web/
RUN dotnet restore SubliSport.Web/SubliSport.Web.csproj
COPY src/ .
RUN dotnet publish SubliSport.Web/SubliSport.Web.csproj -c Release -o /app/publish

## Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SubliSport.Web.dll"]
