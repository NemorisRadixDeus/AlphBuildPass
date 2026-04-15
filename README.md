# Password Manager API — Прототип для диплома

Менеджер паролей с выбором криптографических протоколов и механизмом деривации ключей.

## Стек технологий

- **Backend:** ASP.NET Core 8.0 Web API (C#)
- **БД:** SQLite (по умолчанию) / PostgreSQL (опционально)
- **ORM:** Entity Framework Core 8
- **Аутентификация:** JWT Bearer Token
- **Шифрование:** AES-256-GCM, ChaCha20-Poly1305
- **KDF:** Argon2id, PBKDF2
- **Документация API:** Swagger UI

## Быстрый старт

### 1. Убедитесь, что установлен .NET 8 SDK

```bash
dotnet --version
```

Если не установлен: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Запустите проект

```bash
cd PasswordManager
dotnet restore
dotnet run
```

### 3. Откройте Swagger UI

Перейдите в браузере: http://localhost:5000

Swagger покажет все эндпоинты с документацией.

## API эндпоинты

### Авторизация (без токена)

| Метод | URL                    | Описание           |
|-------|------------------------|---------------------|
| POST  | /api/auth/register     | Регистрация         |
| POST  | /api/auth/login        | Вход (выдача JWT)   |

### Хранилище (требуется JWT)

| Метод  | URL                 | Описание              |
|--------|---------------------|-----------------------|
| GET    | /api/vault          | Список всех записей   |
| GET    | /api/vault/{id}     | Получить одну запись   |
| POST   | /api/vault          | Добавить запись        |
| PUT    | /api/vault/{id}     | Обновить запись        |
| DELETE | /api/vault/{id}     | Удалить запись         |

### Генератор паролей (без токена)

| Метод | URL                    | Описание                      |
|-------|------------------------|-------------------------------|
| POST  | /api/generator         | Генерация с параметрами       |
| GET   | /api/generator/quick   | Быстрая генерация (query)     |

## Переключение на PostgreSQL

1. Создайте базу данных:
   ```sql
   CREATE DATABASE password_manager;
   ```

2. В файле `Program.cs` закомментируйте блок SQLite и раскомментируйте PostgreSQL.

3. Проверьте строку подключения в `appsettings.json`.

4. Перезапустите приложение — таблицы создадутся автоматически.

## Тестирование в Postman

1. Импортируйте коллекцию `postman_collection.json` (Postman → Import).
2. Запустите приложение (`dotnet run`).
3. Выполните запросы по порядку: Register → Login → Vault операции.
4. Запустите Collection Runner для автотестов.

## Структура проекта

```
PasswordManager/
├── Controllers/
│   ├── AuthController.cs         # Регистрация и авторизация
│   ├── VaultController.cs        # CRUD хранилища паролей
│   └── GeneratorController.cs    # Генератор паролей
├── Services/
│   ├── CryptoService.cs          # AES-256-GCM / ChaCha20-Poly1305
│   ├── KeyDerivationService.cs   # Argon2id / PBKDF2
│   ├── PasswordGeneratorService.cs
│   └── JwtService.cs             # JWT токены
├── Models/
│   └── Entities.cs               # User, VaultEntry, AuditLog
├── DTOs/
│   └── Dtos.cs                   # Запросы и ответы API
├── Data/
│   └── AppDbContext.cs            # EF Core контекст
├── Program.cs                     # Точка входа + DI
├── appsettings.json               # Настройки
└── README.md
```
