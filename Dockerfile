FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FormsApp.csproj", "./"]
RUN dotnet restore "FormsApp.csproj"
COPY . .
RUN dotnet build "FormsApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FormsApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FormsApp.dll"] 