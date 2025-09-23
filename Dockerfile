# === AŞAMA 1: Vue.js Frontend'i Build Etme ===
FROM node:18 AS frontend-build
WORKDIR /app

# Frontend projesinin dosyalarını kopyala
COPY todo-frontend/package*.json ./
RUN npm install
COPY todo-frontend/ ./
RUN npm run build


# === AŞAMA 2: .NET Backend'i Build Etme ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Önce .csproj dosyasını kopyalayıp bağımlılıkları yükle
COPY ToDoApi/*.csproj ./ToDoApi/
RUN dotnet restore ./ToDoApi/

# Backend projesinin geri kalanını kopyala
COPY ToDoApi/ ./ToDoApi/

# Build edilmiş frontend dosyalarını, backend'in wwwroot klasörüne kopyala
COPY --from=frontend-build /app/dist ./ToDoApi/wwwroot

# Backend projesini publish et
RUN dotnet publish ./ToDoApi/ -c Release -o /app/publish


# === AŞAMA 3: Final İmajını Oluşturma ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend-build /app/publish .

RUN ls -R
ENTRYPOINT ["dotnet", "ToDoApi.dll"]