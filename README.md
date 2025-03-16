# ActiLink Backend  

Backend aplikacji [**ActiLink**](https://github.com/ActiLink/ActiLink-frontend.git) odpowiada za obsługę użytkowników, wydarzeń oraz interakcje pomiędzy uczestnikami.  
## 🎯 Cel projektu  
Zapewnienie **interfejsów API RESTful**, które zasilają aplikację frontendową. Obsługa użytkowników, wydarzeń oraz interakcji między nimi poprzez zarządzanie autoryzacją, organizacją aktywności i komunikacją.  

## 🚀 Kluczowe funkcjonalności  
- **Rejestracja i logowanie** – obsługa użytkowników  
- **Zarządzanie wydarzeniami** – CRUD dla aktywności organizowanych przez użytkowników  
- **Przechowywanie danych użytkowników i wydarzeń**  
- **Obsługa znajomych** – zarządzanie relacjami użytkowników  

## 🛠 Technologie i narzędzia  
- **ASP.NET Core 9.0** – framework backendowy

## 🔗 API  
W obecnej wersji dostępny jest jeden endpoint:  
- `GET /WeatherForecast` – zwraca przykładową prognozę pogody 

## 📦 Instalacja i uruchomienie  
Aby uruchomić backend lokalnie, wykonaj następujące kroki:  

### 1. **Sklonuj repozytorium:**  
   ```bash
   git clone https://github.com/ActiLink/ActiLink-backend.git
   cd ActiLink-backend
   ```  

### 2. **Zainstaluj wymagane zależności:**  
   Przed uruchomieniem aplikacji upewnij się, że masz zainstalowane wymagane narzędzia:  
   - [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download)  
   - [Docker](https://www.docker.com/get-started)  

### 3. **Zbuduj obraz Dockera:**  
   ```bash
   docker build -t actilink:latest -f ActiLink/Dockerfile .
   ```  
   Spowoduje to utworzenie obrazu backendu gotowego do uruchomienia w kontenerze Docker.  

### 4. **Uruchom nowy kontener:**  
   ```bash
   docker run -d -p 8080:8080 -p 8081:8081 --name actilink-container actilink
   ```  
   Backend powinien być teraz uruchomiony i dostępny pod odpowiednimi portami.  

### 5. **Zatrzymanie działającego kontenera:**  
   ```bash
   docker stop actilink-container
   ```  

### 6. **Uruchomienie istniejącego kontenera:**  
   ```bash
   docker start actilink-container
  ```

## 🧹 Czyszczenie  
Aby usunąć istniejący kontener i zwolnić zasoby, użyj:  
   ```bash
   docker rm -f actilink-container
   ```  

Aby usunąć obraz Dockera:  
   ```bash
   docker rmi actilink:latest
   ```

## ❓ Rozwiązywanie problemów  
Jeśli napotkasz problemy z uruchomieniem backendu, upewnij się, że jesteś w folderze z plikiem .sln oraz że Docker jest włączony przed uruchomieniem kontenera.
