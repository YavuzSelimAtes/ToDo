FROM node:18 AS frontend-build
WORKDIR /app

COPY todo-frontend/package*.json ./
RUN npm install

COPY todo-frontend/ ./

RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./

WORKDIR "/src/wwwroot"
COPY --from=frontend-build /app/dist .

WORKDIR "/src/"
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=backend-build /app/publish .

# Uygulamayı çalıştırıyoruz.
ENTRYPOINT ["dotnet", "ToDoApi.dll"]