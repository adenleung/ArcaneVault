FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ArcaneVault.Web/ArcaneVault.Web.csproj", "ArcaneVault.Web/"]
RUN dotnet restore "ArcaneVault.Web/ArcaneVault.Web.csproj"

COPY . .
WORKDIR "/src/ArcaneVault.Web"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ArcaneVault.Web.dll"]