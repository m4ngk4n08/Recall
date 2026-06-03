FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Copy csproj from the correct nested path
COPY ["Recall.Api/Recall.Api/Recall.Api.csproj", "Recall.Api/Recall.Api/"]
RUN dotnet restore "Recall.Api/Recall.Api/Recall.Api.csproj"
COPY . .
# Move to the project folder for the build
WORKDIR "/src/Recall.Api/Recall.Api"
RUN dotnet build "Recall.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "Recall.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Recall.Api.dll"]