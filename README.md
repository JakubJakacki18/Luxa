# Luxa

Projekt **Luxa** to portal społecznościowy stworzony w ramach przedmiotu **Programowanie aplikacji WWW w technologii .NET**. Jego głównym celem jest umożliwienie użytkownikom interakcji poprzez zdjęcia, z naciskiem na funkcjonalności społecznościowe. Projekt został zbudowany w oparciu o wzorzec projektowy **MVC** i wykorzystuje **Entity Framework** jako ORM.
 

## Główne funkcjonalności:
1. **Logowanie i rejestracja**:
   - Możliwość rejestracji za pomocą konta Google.
   - Podział użytkowników na role (np. admin, moderator, użytkownik).

2. **Zarządzanie zdjęciami**:
   - Dodawanie zdjęć.
   - Pobieranie zdjęć.
   - Polubienia zdjęć.
   - Podział zdjęć na kategorie.
   - Wyszukiwanie zdjęć według kategorii i tagów.
   - System komentarzy pod zdjęciami.

3. **Profil użytkownika**:
   - Możliwość ustawienia i zmiany avatara oraz tła profilu.
   - Prywatne konto z możliwością ograniczenia dostępu do zdjęć.
   - Edycja opisu profilu.

4. **System powiadomień**:
   - Powiadomienia dla użytkowników, np. o nowych funkcjonalnościach lub działaniach administracyjnych.

5. **Panel administracyjny**:
   - Zarządzanie użytkownikami i ich rolami.
   - Obsługa zgłoszeń użytkowników.

6. **Inne funkcjonalności**:
   - System reputacji konta.
   - Obsługa zgłoszeń błędów i problemów technicznych.

## Technologie i narzędzia:
- **Backend**:
  - ASP.NET Core z frameworkiem **Identity** do zarządzania użytkownikami.
  - **Entity Framework** do obsługi bazy danych.
  - **AutoMapper** do mapowania modeli.

- **Frontend**:
  - Widoki w technologii **Razor**.
  - **JavaScript** do obsługi dynamicznych elementów interfejsu użytkownika.
  - **CSS** oraz **Bootstrap** do stylizacji.

- **Baza danych**:
  - Relacyjna baza danych zarządzana przez **SQL Server**.

## Struktura projektu:
- **Controllers**: Odpowiada za przetwarzanie żądań użytkowników.
- **Services**: Logika biznesowa aplikacji.
- **Repository**: Pobieranie/wysyłanie danych do bazy danych
- **Data**:Zawiera Context, Enumy oraz statyczne zmienne
- **Interfaces**:Zawiera abstrakcję wykorzystywaną w serwisach oraz controllerach
- **Components**: Zawiera ViewComponenty
- **ViewModel**:Są to modele z atrybutami używane do wyświetlania na stronie lub wysyłania do kontrolera.
- **Views**: Zawiera widoki Razor, które generują interfejs użytkownika.
- **Models**: Definiuje struktury danych używane w aplikacji.
- **Data**: Zawiera konfigurację bazy danych i inicjalizację danych.
- **wwwroot**: Zawiera zasoby statyczne, takie jak pliki JavaScript, CSS i obrazy.

## Instrukcja:
### Połączenie kodu z bazą danych
1. Utwórz nową bazę danych o np. nazwie LuxaDb 
	(Widok->SQL Server Object Explorer -> (wybierz dostępny serwer np. SQL EXPRESS albo LocalDB (Istnieje bez instalacji czegokolwiek))-> Databases -> Add new database)
2. Rozwiń drzewo utworzonej bazy danych i wejdź w właściwości, wyszukaj connection string i skopiuj całość
3. Wklej w appsettings.json w cudzysłowach przy LuxaDb
4. Wejdź w konsole menadżera pakietów wpisz add-migration "dowolna nazwa" i update-database

### Logowanie za pomocą Google
Żeby logowanie Google działało należy pobrać pakiet "Microsoft.AspNetCore.Authentication.Google"
   	- https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.Google (dostęp: 13/06/2024)


