FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["StudentHelperAPI.csproj", "."]
RUN dotnet restore "StudentHelperAPI.csproj"
COPY . .
RUN dotnet build "StudentHelperAPI.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "StudentHelperAPI.dll"]