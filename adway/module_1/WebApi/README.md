# University Course Management API

API для управления студентами, преподавателями, курсами и записями студентов на курсы.

## Технологии

* ASP.NET Core Web API
* .NET 10
* Entity Framework Core
* PostgreSQL
* AutoMapper
* Swagger
* Repository Pattern
* Dependency Injection

## Функциональность

* CRUD для студентов, преподавателей и курсов
* Запись студентов на курсы
* Изменение оценок
* Получение курсов студента
* Поиск курсов
* Валидация данных
* Обработка ошибок и логирование

## Архитектура

```text
Client
  ↓ HTTP
Controller
  ↓
Service
  ↓
Repository
  ↓
Entity Framework Core
  ↓
PostgreSQL
```

## Запуск

Настроить подключение к PostgreSQL в `appsettings.json`.

Выполнить:

```bash
dotnet ef database update
dotnet run
```

Swagger:

```text
https://localhost:XXXX/swagger
```


Тестовые данные находятся в папке `database`.
