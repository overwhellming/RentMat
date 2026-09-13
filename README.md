# RentMat

> REST API для сервиса аренды и управления прокатом оборудования: каталог устройств, бронирование, баланс пользователей и история аренды.

## Возможности

- Аутентификация и авторизация (JWT)
- Каталог устройств: категории, статусы, доступность
- Бронирование оборудования с проверкой пересечений по времени
- Управление балансом пользователей и расчёт стоимости аренды
- История аренды и текущие активные брони
- Валидация входных данных (FluentValidation)
- Кэширование горячих запросов (FusionCache)
- Структурированное логирование (Serilog + Seq)

## Технологии

| Слой | Стек |
|---|---|
| Runtime | ASP.NET Core 10 (chiseled composite образ) |
| Архитектура | Clean Architecture, CQRS / MediatR |
| База данных | PostgreSQL |
| ORM | Entity Framework Core  |
| Валидация | FluentValidation |
| Кэширование | FusionCache |
| Логирование | Serilog + Seq |
| Инфраструктура | Docker / Docker Compose |

## Архитектура

Решение построено по принципам Clean Architecture с разделением на слои:

```
├── RentMat.Domain/          # Сущности, value objects, доменные события
├── RentMat.Application/     # Use cases (CQRS: commands/queries + handlers), DTO, валидаторы
├── RentMat.Infrastructure/  # EF Core, репозитории, внешние сервисы, кэш
|── RentMat.Api/             # Контроллеры/endpoints, middleware, DI, конфигурация
├── RentMat.UnitTests/ # Модульные тесты
└── RentMat.IntegrationTests/ # Интеграционные тесты
```

## Требования

| Инструмент | Версия | Назначение |
|---|---|---|
| .NET SDK | 10.0+ | Сборка и запуск бэкенда |
| Docker Desktop | 24+ | PostgreSQL, Seq, запуск через compose |
| EF Core CLI | 10.0+ | Применение миграций (`dotnet tool install --global dotnet-ef`) |

## Быстрый старт

### 1. Клонирование

```bash
git clone https://github.com/overwhellming/RentMat
cd RentMat
```

### 2. Переменные окружения

Создай `.env` в корне (шаблон — `.env.example`):

```env
POSTGRES_USER=admin
POSTGRES_PASSWORD=1234
POSTGRES_DB=rentmat_db
POSTGRES_PORT=5432
JWT_KEY=super_secret_key_at_least_32_chars_long
SEQ_PASSWORD=1234
```

Создай `appsettings.Development.json` в корне (шаблон — `appsettings.Example.json`):
```bash
cp RentMat.API/appsettings.Example.json RentMat.API/appsettings.Development.json
```

### 3. Запуск инфраструктуры

```bash
docker compose up -d
```

Поднимутся PostgreSQL и Seq.

### 4. Миграции

```bash
dotnet ef database update --project RentMat.Infrastructure --startup-project RentMat.API
```

## Конфигурация

Все настройки читаются из `appsettings.json` + переменных окружения (см. `.env` выше).

| Переменная | Обязательна | Описание |
|---|---|---|
| `POSTGRES_*` | да | Параметры БД для compose |
| `JWT_KEY` | да | Секрет подписи токенов (≥32 символов) |
| `SEQ_PASSWORD` | да | Пароль Seq |

## API

Полная спецификация — в Swagger UI: `http://localhost:5000/index.html`

### Authentication

| Метод | Путь | Описание | Auth |
|---|---|---|---|
| `POST` | `/api/auth/register` | Регистрация пользователя, возвращает JWT | — |
| `POST` | `/api/auth/login` | Аутентификация, возвращает JWT | — |
| `POST` | `/api/auth/refresh` | Обновление refresh-токена текущего пользователя | + |
| `POST` | `/api/auth/revoke` | Отзыв refresh-токена текущего пользователя | + |

### Bookings

| Метод | Путь | Описание | Auth |
|---|---|---|---|
| `GET` | `/api/bookings` | Список всех броней | + |
| `POST` | `/api/bookings` | Создание брони | + |
| `GET` | `/api/bookings/{id}` | Бронь по идентификатору | + |
| `GET` | `/api/bookings/me` | Брони текущего пользователя | + |
| `POST` | `/api/bookings/me/{id}/complete` | Завершение брони текущего пользователя | + |

### Devices

| Метод | Путь | Описание | Auth |
|---|---|---|---|
| `GET` | `/api/devices` | Список всех устройств | — |
| `GET` | `/api/devices/{id}` | Устройство по идентификатору | — |
| `POST` | `/api/devices/create` | Создание устройства | + |
| `PUT` | `/api/devices/{id}` | Обновление устройства | + |
| `PUT` | `/api/devices/{id}/retire` | Перевод устройства в статус `retired` | + |

### Users

| Метод | Путь | Описание | Auth |
|---|---|---|---|
| `GET` | `/api/users` | Список всех пользователей | + |
| `GET` | `/api/users/{id}` | Пользователь по идентификатору | + |
| `GET` | `/api/users/me` | Текущий пользователь | + |
| `GET` | `/api/users/me/balance` | Баланс текущего пользователя | + |
| `POST` | `/api/users/me/balance` | Пополнение баланса текущего пользователя | + |
| `GET` | `/api/users/me/deposits` | История пополнений текущего пользователя | + |

## Тестирование

```bash
dotnet test
```

- `RentMat.UnitTests` — доменная логика и handlers
- `RentMat.IntegrationTests` — API + БД (Testcontainers)

## Структура проекта

```
RentMat/
├── RentMat.Api/
├── RentMat.Application/
├── RentMat.Domain/
|── RentMat.Infrastructure/
├── RentMat.UnitTests/
|── RentMat.IntegrationTests/
├── docker-compose.yml
├── .env.example
└── RentMat.sln
```