# Задание 3, модули 9-12. Library Management API

REST API для библиотеки на Go: книги, читатели, выдача и возврат книг, статистика. Используется только стандартная библиотека (`net/http`, `encoding/json`, `sync`), без фреймворков и базы данных. Все данные хранятся в памяти в `map` и `slice`.

## Запуск

Нужен Go 1.22 или новее (шаблоны маршрутов вида `GET /api/books/{id}`).

```bash
go run ./cmd/server
```

Сервер запускается на http://localhost:8080. При старте добавляются 4 книги и 2 читателя, чтобы сразу было с чем работать.

Проверки:

```bash
go build ./...
go vet ./...
go test ./...
```

## Структура проекта

```
module_3/
├── cmd/server/main.go           запуск сервера на :8080 и начальные данные
├── internal/
│   ├── models/                  структуры Book, Reader, Loan, входные DTO и статистика
│   ├── apperr/apperr.go         тип ошибки с HTTP-статусом и кодом (BOOK_NOT_FOUND и т.д.)
│   ├── storage/memory.go        хранилище в памяти: map + sync.RWMutex + счётчики id
│   ├── services/                бизнес-логика и правила выдачи
│   ├── handlers/                HTTP-обработчики, разбор JSON и path-параметров
│   ├── response/response.go     единый формат ответа
│   ├── middleware/logging.go    логирование запросов и перехват паники (500)
│   └── router/router.go         маршруты http.ServeMux, 404 и 405
├── go.mod
└── README.md
```

Запрос проходит так: `middleware` → `router` (ServeMux) → `handlers` (разбор запроса) → `services` (проверки и правила) → `storage` (map под мьютексом). Обработчики не знают про хранилище, а сервисы не знают про HTTP: ошибки возвращаются как `*apperr.Error`, и `response.Error` превращает их в нужный статус.

## Endpoints

| Метод | Endpoint | Описание | Успех |
|---|---|---|---|
| GET | /api/books | Все книги | 200 |
| GET | /api/books/{id} | Книга по id | 200 |
| POST | /api/books | Добавить книгу | 201 |
| PUT | /api/books/{id} | Обновить книгу | 200 |
| DELETE | /api/books/{id} | Удалить книгу | 200 |
| GET | /api/books/search?title=&author=&genre=&year= | Фильтрация книг | 200 |
| GET | /api/readers?name= | Все читатели, поиск по имени | 200 |
| GET | /api/readers/{id} | Читатель по id | 200 |
| POST | /api/readers | Регистрация читателя | 201 |
| PUT | /api/readers/{id} | Обновить читателя | 200 |
| DELETE | /api/readers/{id} | Удалить читателя | 200 |
| GET | /api/readers/{id}/loans | История выдач читателя | 200 |
| GET | /api/loans | Все выдачи | 200 |
| GET | /api/loans/{id} | Выдача по id | 200 |
| POST | /api/loans | Выдать книгу | 201 |
| PUT | /api/loans/{id}/return | Вернуть книгу | 200 |
| GET | /api/loans/active | Активные выдачи | 200 |
| GET | /api/statistics | Статистика библиотеки | 200 |

Реализованы все endpoints из задания; `GET /api/readers` дополнительно ищет читателей по имени через `?name=`. Неизвестный маршрут возвращает 404 `ROUTE_NOT_FOUND`, неподдерживаемый метод — 405 `METHOD_NOT_ALLOWED` с заголовком `Allow`.

## Формат ответа

Успех:

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": { "id": 1, "title": "Чистый код" },
  "errorCode": null,
  "errorMessage": null
}
```

Ошибка:

```json
{
  "statusCode": 409,
  "isSuccess": false,
  "result": null,
  "errorCode": "BOOK_NOT_AVAILABLE",
  "errorMessage": "Нет доступных экземпляров книги"
}
```

Коды ошибок: `VALIDATION_ERROR`, `INVALID_JSON`, `EMPTY_BODY`, `INVALID_ID`, `INVALID_QUERY` (400); `BOOK_NOT_FOUND`, `READER_NOT_FOUND`, `LOAN_NOT_FOUND`, `ROUTE_NOT_FOUND` (404); `METHOD_NOT_ALLOWED` (405); `EMAIL_ALREADY_EXISTS`, `BOOK_NOT_AVAILABLE`, `READER_INACTIVE`, `LOAN_LIMIT_EXCEEDED`, `LOAN_ALREADY_RETURNED`, `BOOK_HAS_ACTIVE_LOANS`, `READER_HAS_ACTIVE_LOANS`, `COPIES_IN_USE` (409); `INTERNAL_ERROR` (500).

## Примеры запросов

Книга:

```json
POST /api/books
{ "title": "Изучаем Go", "author": "Jon Bodner", "year": 2021, "genre": "IT", "totalCopies": 2 }
```

Читатель (`isActive` необязателен, по умолчанию `true`):

```json
POST /api/readers
{ "firstName": "Ратмир", "lastName": "Сарбасов", "email": "ratmir@example.com", "age": 20 }
```

Выдача (`days` необязателен, по умолчанию 14, допустимо 1–60):

```json
POST /api/loans
{ "bookId": 4, "readerId": 3, "days": 14 }
```

## Бизнес-правила

- Нельзя выдать несуществующую книгу (404) или книгу несуществующему читателю (404).
- Нельзя выдать книгу неактивному читателю (409 `READER_INACTIVE`).
- Нельзя выдать книгу, если `AvailableCopies == 0` (409 `BOOK_NOT_AVAILABLE`).
- При выдаче `AvailableCopies` уменьшается на 1, при возврате увеличивается на 1.
- Нельзя вернуть уже возвращённую книгу (409 `LOAN_ALREADY_RETURNED`).
- У читателя не больше 5 активных выдач, константа `MaxActiveLoans` (409 `LOAN_LIMIT_EXCEEDED`).
- Нельзя удалить книгу или читателя с активными выдачами (409).
- Email читателя уникален без учёта регистра (409 `EMAIL_ALREADY_EXISTS`).
- При обновлении книги нельзя сделать `totalCopies` меньше числа выданных экземпляров (409 `COPIES_IN_USE`).

## Где используются темы модулей

**Модуль 1. Типы, операторы, циклы, структуры, константы**
- `int`, `string`, `bool`, `float64`: поля моделей; `float64` для процента доступности в `availabilityPercent` (`services/loan_service.go`).
- `if`/`else` и `switch`: валидация в `validateBook` и `validateReader`, проверки правил в `Borrow`.
- `for`: поиск и фильтрация в `sortedBooks`, `emailTaken`, `countActiveLoans`, подсчёты в `Statistics`.
- `struct`: `Book`, `Reader`, `Loan`, `Statistics` в `internal/models`.
- `const`: `MaxActiveLoans = 5`, `DefaultLoanDays`, `MaxLoanDays`, `TopBooksCount`.

**Модуль 2. Массивы, срезы, карты, функции, указатели**

| Механизм | Где |
|---|---|
| `map[int]Book`, `map[int]Reader`, `map[int]Loan` | `storage.Memory` — хранение по id |
| `[]Book`, `[]Loan`, `[]Reader` | результаты списков и фильтрации: `sortedBooks`, `filterLoans`, `ReaderService.GetAll` |
| `[12]int` | `loansByMonth` — число выдач по месяцам; `[12]string` для названий месяцев |
| `map[int]int` | подсчёт выдач каждой книги в `topBooks` |
| Указатели | `applyBookInput(*Book, ...)`, `applyReaderInput(*Reader, ...)`, `takeCopy(*Book)`, `returnCopy(*Book)`, `markReturned(*Loan, ...)`; `ReturnedAt *time.Time`; `IsActive *bool` во входных данных |
| Несколько возвращаемых значений | `GetByID(id) (*models.Book, error)`, `Borrow(...) (*models.Loan, error)`, `pathID(r) (int, error)` |
| `error` | свой тип `apperr.Error` реализует `error`, разбирается через `errors.As` |

Собственные функции (кроме HTTP-обработчиков), больше 10: `validateBook`, `applyBookInput`, `matchesFilter`, `sortedBooks`, `containsFold`, `validateReader`, `isValidEmail`, `emailTaken`, `applyReaderInput`, `takeCopy`, `returnCopy`, `markReturned`, `countActiveLoans`, `isOverdue`, `availabilityPercent`, `loansByMonth`, `topBooks`, `filterLoans`, `pathID`, `decodeJSON`.

**Модуль 3. HTTP-сервер**
- `net/http`, порт 8080, `http.Server` с таймаутом чтения заголовков (`cmd/server/main.go`).
- JSON через `encoding/json`: `decodeJSON` запрещает неизвестные поля, ограничивает тело 1 МБ и отличает пустое тело от битого JSON.
- Логирование каждого запроса в `middleware.Logging`; `middleware.Recover` превращает панику в ответ 500.

**Модуль 4. Маршрутизация**
- `http.ServeMux` с шаблонами Go 1.22: разные методы на одном ресурсе (`GET`/`PUT`/`DELETE /api/books/{id}`).
- Path-параметры: `r.PathValue("id")` в `pathID`.
- Query-параметры: `/api/books/search?author=Martin&genre=IT`, `/api/readers?name=`.
- Маршруты в `router`, обработчики в `handlers`.
- Middleware оборачивает весь `mux`: `Recover(Logging(mux))`.
- Для каждого пути зарегистрирован шаблон без метода, который отвечает 405 в едином формате, и `/` для 404.

**Конкурентный доступ.** Все `map` лежат в `storage.Memory` вместе с `sync.RWMutex`. Чтение (`GetAll`, `GetByID`, `Statistics`) берёт `RLock`, изменение (`Create`, `Update`, `Delete`, `Borrow`, `Return`) — `Lock`. Проверка правил и изменение данных при выдаче выполняются под одной блокировкой, поэтому два одновременных запроса не выдадут последний экземпляр дважды.

## Тесты

`go test ./...` запускает 8 тестов:

- `internal/services/services_test.go`: выдача и возврат меняют `AvailableCopies`, повторный возврат и выдача без экземпляров дают 409; лимит 5 активных выдач; повторный email и удаление книги или читателя с активной выдачей; расчёт статистики (активные, просроченные, процент, месяцы, топ).
- `internal/router/router_test.go` (через `net/http/httptest`): создание и получение книги; битый JSON и ошибка валидации (400); неизвестный маршрут (404) и неподдерживаемый метод (405); выдача несуществующему читателю (404).

```
ok  	library-management-api/internal/router
ok  	library-management-api/internal/services
```

Пример лога сервера:

```
2026/11/11 14:32:53 Library Management API запущен на http://localhost:8080
2026/11/11 14:32:53 GET /api/books -> 200 (0s)
2026/11/11 14:32:53 GET /api/books/2 -> 200 (0s)
2026/11/11 14:32:53 POST /api/loans -> 400 (0s)
```

## Результаты 15 сценариев

Сценарии выполнены через curl по порядку на только что запущенном сервере (4 книги и 2 читателя из начальных данных). Перед сценарием 13 выполнена ещё одна выдача (книга 1 читателю 3), чтобы в истории было две записи.

### 1. Получение списка книг

Ожидается: **200**, получено: **200** ✅

```bash
curl -X GET http://localhost:8080/api/books
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": [
    {
      "id": 1,
      "title": "Чистый код",
      "author": "Robert Martin",
      "year": 2008,
      "genre": "IT",
      "totalCopies": 3,
      "availableCopies": 3
    },
    {
      "id": 2,
      "title": "Чистая архитектура",
      "author": "Robert Martin",
      "year": 2017,
      "genre": "IT",
      "totalCopies": 2,
      "availableCopies": 2
    },
    {
      "id": 3,
      "title": "Мастер и Маргарита",
      "author": "Михаил Булгаков",
      "year": 1967,
      "genre": "Роман",
      "totalCopies": 4,
      "availableCopies": 4
    },
    {
      "id": 4,
      "title": "Совершенный код",
      "author": "Steve McConnell",
      "year": 2004,
      "genre": "IT",
      "totalCopies": 1,
      "availableCopies": 1
    }
  ],
  "errorCode": null,
  "errorMessage": null
}
```

### 2. Создание книги

Ожидается: **201**, получено: **201** ✅

```bash
curl -X POST http://localhost:8080/api/books -H "Content-Type: application/json" -d '{"title":"Изучаем Go","author":"Jon Bodner","year":2021,"genre":"IT","totalCopies":2}'
```

```json
{
  "statusCode": 201,
  "isSuccess": true,
  "result": {
    "id": 5,
    "title": "������� Go",
    "author": "Jon Bodner",
    "year": 2021,
    "genre": "IT",
    "totalCopies": 2,
    "availableCopies": 2
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 3. Получение книги по ID

Ожидается: **200**, получено: **200** ✅

```bash
curl -X GET http://localhost:8080/api/books/5
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": {
    "id": 5,
    "title": "������� Go",
    "author": "Jon Bodner",
    "year": 2021,
    "genre": "IT",
    "totalCopies": 2,
    "availableCopies": 2
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 4. Обновление книги

Ожидается: **200**, получено: **200** ✅

```bash
curl -X PUT http://localhost:8080/api/books/5 -H "Content-Type: application/json" -d '{"title":"Изучаем Go, 2-е издание","author":"Jon Bodner","year":2024,"genre":"IT","totalCopies":3}'
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": {
    "id": 5,
    "title": "������� Go, 2-� �������",
    "author": "Jon Bodner",
    "year": 2024,
    "genre": "IT",
    "totalCopies": 3,
    "availableCopies": 3
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 5. Поиск книг по автору

Ожидается: **200**, получено: **200** ✅

```bash
curl -X GET http://localhost:8080/api/books/search?author=Martin&genre=IT
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": [
    {
      "id": 1,
      "title": "Чистый код",
      "author": "Robert Martin",
      "year": 2008,
      "genre": "IT",
      "totalCopies": 3,
      "availableCopies": 3
    },
    {
      "id": 2,
      "title": "Чистая архитектура",
      "author": "Robert Martin",
      "year": 2017,
      "genre": "IT",
      "totalCopies": 2,
      "availableCopies": 2
    }
  ],
  "errorCode": null,
  "errorMessage": null
}
```

### 6. Создание читателя

Ожидается: **201**, получено: **201** ✅

```bash
curl -X POST http://localhost:8080/api/readers -H "Content-Type: application/json" -d '{"firstName":"Ратмир","lastName":"Сарбасов","email":"ratmir@example.com","age":20}'
```

```json
{
  "statusCode": 201,
  "isSuccess": true,
  "result": {
    "id": 3,
    "firstName": "������",
    "lastName": "��������",
    "email": "ratmir@example.com",
    "age": 20,
    "isActive": true
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 7. Повторная регистрация Email

Ожидается: **409**, получено: **409** ✅

```bash
curl -X POST http://localhost:8080/api/readers -H "Content-Type: application/json" -d '{"firstName":"Другой","lastName":"Читатель","email":"ratmir@example.com","age":25}'
```

```json
{
  "statusCode": 409,
  "isSuccess": false,
  "result": null,
  "errorCode": "EMAIL_ALREADY_EXISTS",
  "errorMessage": "Читатель с таким email уже зарегистрирован"
}
```

### 8. Выдача доступной книги

Ожидается: **201**, получено: **201** ✅

```bash
curl -X POST http://localhost:8080/api/loans -H "Content-Type: application/json" -d '{"bookId":4,"readerId":3,"days":14}'
```

```json
{
  "statusCode": 201,
  "isSuccess": true,
  "result": {
    "id": 1,
    "bookId": 4,
    "readerId": 3,
    "borrowedAt": "2026-11-11T14:32:32.8665436+05:00",
    "dueDate": "2026-11-25T14:32:32.8665436+05:00",
    "returnedAt": null,
    "isReturned": false
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 9. Выдача книги без доступных экземпляров

Ожидается: **409**, получено: **409** ✅

```bash
curl -X POST http://localhost:8080/api/loans -H "Content-Type: application/json" -d '{"bookId":4,"readerId":1}'
```

```json
{
  "statusCode": 409,
  "isSuccess": false,
  "result": null,
  "errorCode": "BOOK_NOT_AVAILABLE",
  "errorMessage": "Нет доступных экземпляров книги"
}
```

### 10. Выдача книги несуществующему читателю

Ожидается: **404**, получено: **404** ✅

```bash
curl -X POST http://localhost:8080/api/loans -H "Content-Type: application/json" -d '{"bookId":1,"readerId":999}'
```

```json
{
  "statusCode": 404,
  "isSuccess": false,
  "result": null,
  "errorCode": "READER_NOT_FOUND",
  "errorMessage": "Читатель не найден"
}
```

### 11. Возврат книги

Ожидается: **200**, получено: **200** ✅

```bash
curl -X PUT http://localhost:8080/api/loans/1/return
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": {
    "id": 1,
    "bookId": 4,
    "readerId": 3,
    "borrowedAt": "2026-11-11T14:32:32.8665436+05:00",
    "dueDate": "2026-11-25T14:32:32.8665436+05:00",
    "returnedAt": "2026-11-11T14:32:33.3244686+05:00",
    "isReturned": true
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 12. Повторный возврат

Ожидается: **409**, получено: **409** ✅

```bash
curl -X PUT http://localhost:8080/api/loans/1/return
```

```json
{
  "statusCode": 409,
  "isSuccess": false,
  "result": null,
  "errorCode": "LOAN_ALREADY_RETURNED",
  "errorMessage": "Книга уже возвращена"
}
```

### 13. Получение истории выдач читателя

Ожидается: **200**, получено: **200** ✅

```bash
curl -X GET http://localhost:8080/api/readers/3/loans
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": [
    {
      "id": 1,
      "bookId": 4,
      "readerId": 3,
      "borrowedAt": "2026-11-11T14:32:32.8665436+05:00",
      "dueDate": "2026-11-25T14:32:32.8665436+05:00",
      "returnedAt": "2026-11-11T14:32:33.3244686+05:00",
      "isReturned": true
    },
    {
      "id": 2,
      "bookId": 1,
      "readerId": 3,
      "borrowedAt": "2026-11-11T14:32:33.6223356+05:00",
      "dueDate": "2026-11-25T14:32:33.6223356+05:00",
      "returnedAt": null,
      "isReturned": false
    }
  ],
  "errorCode": null,
  "errorMessage": null
}
```

### 14. Получение статистики

Ожидается: **200**, получено: **200** ✅

```bash
curl -X GET http://localhost:8080/api/statistics
```

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "result": {
    "totalBooks": 5,
    "totalCopies": 13,
    "availableCopies": 12,
    "availabilityPercent": 92.3,
    "totalReaders": 3,
    "activeLoans": 1,
    "overdueLoans": 0,
    "topBooks": [
      {
        "bookId": 1,
        "title": "Чистый код",
        "loans": 1
      },
      {
        "bookId": 4,
        "title": "Совершенный код",
        "loans": 1
      }
    ],
    "loansByMonth": [
      {
        "month": "Январь",
        "loans": 0
      },
      {
        "month": "Февраль",
        "loans": 0
      },
      {
        "month": "Март",
        "loans": 0
      },
      {
        "month": "Апрель",
        "loans": 0
      },
      {
        "month": "Май",
        "loans": 0
      },
      {
        "month": "Июнь",
        "loans": 0
      },
      {
        "month": "Июль",
        "loans": 0
      },
      {
        "month": "Август",
        "loans": 0
      },
      {
        "month": "Сентябрь",
        "loans": 0
      },
      {
        "month": "Октябрь",
        "loans": 0
      },
      {
        "month": "Ноябрь",
        "loans": 2
      },
      {
        "month": "Декабрь",
        "loans": 0
      }
    ]
  },
  "errorCode": null,
  "errorMessage": null
}
```

### 15. Некорректный JSON или обязательные поля

Ожидается: **400**, получено: **400** ✅

```bash
curl -X POST http://localhost:8080/api/books -H "Content-Type: application/json" -d '{"title":"Без автора","year":2020'
```

```json
{
  "statusCode": 400,
  "isSuccess": false,
  "result": null,
  "errorCode": "INVALID_JSON",
  "errorMessage": "Некорректный JSON: unexpected EOF"
}
```

