```
# Order System — тестовый проект на Go

Высоконагруженная система обработки заказов (6000 RPS) с HOT/FROZEN путями данных.

## 🏗 Архитектура
Generator (6k RPS) → RabbitMQ → TaskManager → gRPC Stream → TaskWorker
↓
Redis (HOT)
↙ ↘
Dashboard DB Syncer
(чтение) ↓
PostgreSQL (FROZEN)
↑
└── Dashboard fallback

text

### Компоненты

| Сервис | Описание | Порт |
|--------|----------|------|
| **order-gen** | Генератор заказов 6000 RPS | 8081 |
| **task-mgr** | Менеджер задач, группировка по category | 8082 |
| **task-wkr** | Воркер, пишет в Redis + лог в Kafka | 8083 |
| **dashboard** | Морда, читает HOT (Redis) → fallback FROZEN (PG) | 8084 |
| **db-syncer** | Фоновая синхронизация Redis → PostgreSQL | - |
| **auth-srv** | JWT авторизация (бонус) | 8085 |

### Технологии

- **Go** — основной язык
- **Redis** — HOT cache (TTL = 60 сек, скользящий)
- **PostgreSQL** — FROZEN storage
- **RabbitMQ** — буфер между генератором и обработчиками
- **Kafka** — централизованное логирование
- **gRPC** — коммуникация TaskManager ↔ TaskWorker

## 📚 API Документация (Swagger)

Каждый HTTP сервис имеет встроенную Swagger UI:

### Dashboard API (порт 8084)
# Открыть в браузере
open http://localhost:8084/swagger/index.html

# Основные эндпоинты:
GET  /api/orders/{userId}           # Получить горячие заказы
GET  /api/orders/{userId}/period    # Заказы за период (с фолбэком в БД)
GET  /api/orders/{userId}/history   # Вся история из БД
DELETE /api/cache/user/{userId}     # Очистить кэш пользователя
GET  /metrics                       # Prometheus метрик


Order Generator API (порт 8081)
bash
open http://localhost:8081/swagger/index.html

# Управление генератором:
POST /api/generator/start     # Запустить генерацию
POST /api/generator/stop      # Остановить генерацию
GET  /api/generator/stats     # Статистика (RPS, всего заказов)
PUT  /api/generator/rps       # Изменить RPS (по умолчанию 6000)

Auth Service API (порт 8085)
open http://localhost:8085/swagger/index.html

# Авторизация:
POST /api/auth/login          # Получить JWT токен
POST /api/auth/refresh        # Обновить токен
POST /api/auth/validate       # Проверить токен
GET  /api/auth/me             # Информация о пользователе
```