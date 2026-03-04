# Giai đoạn 1: Build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copy TOÀN BỘ các file .csproj để Restore thư viện (Phải khớp chính xác tên thư mục)
COPY ["ITAssetManagement/ITAssetManagement.csproj", "ITAssetManagement/"]
COPY ["ITAssetManagement.Client/ITAssetManagement.Client.csproj", "ITAssetManagement.Client/"]
COPY ["ITAssetManagement.Common/ITAssetManagement.Common.csproj", "ITAssetManagement.Common/"]
COPY ["ITAssetManagement.Mapper/ITAssetManagement.Mapper.csproj", "ITAssetManagement.Mapper/"]
COPY ["ITAssetManagement.Models/ITAssetManagement.Models.csproj", "ITAssetManagement.Models/"]
COPY ["ITAssetManagement.Repo/ITAssetManagement.Repo.csproj", "ITAssetManagement.Repo/"]
COPY ["ITAssetManagement.Request/ITAssetManagement.Request.csproj", "ITAssetManagement.Request/"]
COPY ["ITAssetManagement.Response/ITAssetManagement.Response.csproj", "ITAssetManagement.Response/"]
COPY ["ITAssetManagement.Service/ITAssetManagement.Service.csproj", "ITAssetManagement.Service/"]

# 2. Lệnh Restore (Dùng project Server làm gốc)
RUN dotnet restore "ITAssetManagement/ITAssetManagement.csproj"

# 3. Copy toàn bộ mã nguồn vào Docker
COPY . .

# 4. Biên dịch project chính
WORKDIR "/src/ITAssetManagement"
RUN dotnet build "ITAssetManagement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ITAssetManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ITAssetManagement.dll"]