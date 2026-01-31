FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AiChat/AiChat.csproj", "AiChat/"]
COPY ["Core/Core.csproj","Core/"]
COPY ["Interfaces/Interfaces.csproj","Interfaces/"]
COPY ["Managers/Managers.csproj","Managers/"]
COPY ["Models/Models.csproj","Models/"]
COPY ["Repositories/Repositories.csproj","Repositories/"]
COPY ["SmartWeather/SmartWeather.csproj","SmartWeather/"]

RUN dotnet restore "SmartWeather/SmartWeather.csproj"

COPY . .

WORKDIR "/src/SmartWeather"
RUN dotnet publish "SmartWeather.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet","SmartWeather.dll"]