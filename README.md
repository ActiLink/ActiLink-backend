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
Po uruchomieniu aplikacji API będzie dostępne pod następującymi adresami:  
- [https://localhost:5289/swagger](https://localhost:5289/swagger)  
- [http://localhost:7062/swagger](http://localhost:7062/swagger)

Można tam pozyskać informacje o dostępnych endpointach.

Aby przetestować w Swaggerze endpointy wymagające autoryzacji należy pozyskać swój **access token** (np. logując się) a następnie kliknąć w **Authorize** i wprowadzić token do pola formularza.

![image](https://github.com/user-attachments/assets/30cc69bb-511f-48c7-b8b7-0fb446cf35ac)

Token w tym polu należy podać w formacie:
```bash
Bearer <twój_token>
```

Analogicznie, przy wysyłaniu zapytań HTTP np. z Postmana lub cURL, należy dodać nagłówek `Authorization`, gdzie:
- **typ uwierzytelnienia (scheme)** to `Bearer`
- **wartość** to sam token

Przykład
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```


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

### 3. **Uruchomienie kontenera za pomocą `docker-compose`:**  
1. Przejdź do katalogu, w którym znajdują się pliki `ActiLink.sln` i `docker-compose.yml`.  
2. Ustaw hasło do bazy danych jako zmienną środowiskową Dockera:  
   - **Windows:**  
     ```bash
     .\set-password.bat Twoje_haslo123
     ```  
   - **Linux:**  
     ```bash
     ./set-password.sh Twoje_haslo123
     ```  
   **Uwaga!** Aby baza danych połączyła się poprawnie, hasło musi mieć przynajmniej 8 znaków i zawierać przynajmniej 3 z poniższych kategorii:  
   - małe litery  
   - wielkie litery  
   - cyfry  
   - znaki specjalne
   
   Hasło można zmieniać tym samym skryptem, podając nowe hasło jako argument, np.:  
   ```bash
   .\set-password.bat Nowe_haslo123
   ```  
   Jeśli skrypt nie zadziała poprawnie, można ręcznie edytować (jeśli trzeba to też utworzyć) plik `.env`  
   ```bash
   notepad .env
   ```  
   i dodać:  
   ```env
   MSSQL_SA_PASSWORD=Twoje_haslo123
   ```
3. **Wygeneruj silny, losowy klucz JWT (256-bitowy):**

   - **Windows (CMD/PowerShell):**
     ```bash
     .set-jwt-key.bat
     ```

   - **Linux/macOS (bash):**
     ```bash
     .set-jwt-key.sh
     ```

   Skrypt:
   - automatycznie utworzy plik `.env` (jeśli nie istnieje),
   - wygeneruje losowo klucz i doda lub zaktualizuje wartość `JWT_SECRET_KEY`,
   - nie usuwa innych zmiennych środowiskowych w `.env`.


4. **Alternatywnie:** Jeśli skrypty nie zadziałają, możesz ręcznie edytować lub utworzyć plik `.env`:
   ```bash
   notepad .env
   ```
   i dodać następujące wpisy
   ```bash
   MSSQL_SA_PASSWORD=Twoje_haslo123
   JWT_SECRET_KEY=losowo_wygenerowany_klucz
   ```
### 4. **Obsługa Dockera**  
#### **Budowanie i uruchomienie kontenera:**  
Zbudować i uruchomić można w konsoli:
```bash
docker-compose up -d
```
lub w Visual Studio (należy pamiętać o wyłączeniu poprzedniego kontenera):

![image](https://github.com/user-attachments/assets/7a4a67d6-94c1-4a3d-89fc-5a28112bb50e)

Jeśli istnieją niezastosowane migracje, po uruchomieniu kontenera z bazą danych, uruchom je poleceniem:
```bash
docker-compose -f docker-compose.migrate.yml run --build actilink-migrator
```

#### **Zatrzymanie kontenera bez usuwania:**  
```bash
docker-compose stop
```  

#### **Ponowne uruchomienie istniejącego kontenera:**  
```bash
docker-compose start
```  

#### **Usunięcie istniejącego kontenera:**  
```bash
docker-compose down
```  

#### **Usunięcie obrazów Dockera:**  
```bash
docker rmi actilink:latest
docker rmi mcr.microsoft.com/mssql/server:2022-latest
```
lub w gui w zakładce Images
#### **Usunięcie zawartości bazy danych**
Dane w bazie powinny przetrwać usunięcie kontenera. oraz ponowną kompilację. Aby usunąć zawartość bazy danych należy zamknąć kontener z dodatkową flagą:
```bash
docker-compose down -v
```
lub w Docker GUI w usunąć `actilink_db_data` z zakładki Volumes.
### 5. **Dostęp do bazy danych w SQL Server Management Studio**  
Aby zalogować się poprawnie, należy podać parametry tak jak na zdjęciu. login: `sa`, hasło: to które zostało ustawione wcześniej i pamiętać o zaznaczeniu **Trust server certificate**.  
   
   ![image](https://github.com/user-attachments/assets/772b4346-8159-47b0-a5b7-a8edf3d09f37)


## ❓ Rozwiązywanie problemów  
Jeśli napotkasz problemy z uruchomieniem backendu, upewnij się, że jesteś w folderze z plikiem .sln oraz że Docker jest włączony przed uruchomieniem kontenera.
