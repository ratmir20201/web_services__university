# API Endpoints

## Students

| Метод  | Endpoint                     | Описание                            |
| ------ | ---------------------------- | ----------------------------------- |
| GET    | `/api/Students`              | Получить список всех студентов      |
| GET    | `/api/Students/{id}`         | Получить студента по ID             |
| GET    | `/api/Students/{id}/courses` | Получить курсы конкретного студента |
| POST   | `/api/Students`              | Создать студента                    |
| PUT    | `/api/Students/{id}`         | Обновить студента                   |
| DELETE | `/api/Students/{id}`         | Удалить студента                    |

## Teachers

| Метод  | Endpoint             | Описание                       |
| ------ | -------------------- | ------------------------------ |
| GET    | `/api/Teachers`      | Получить список преподавателей |
| GET    | `/api/Teachers/{id}` | Получить преподавателя по ID   |
| POST   | `/api/Teachers`      | Создать преподавателя          |
| PUT    | `/api/Teachers/{id}` | Обновить преподавателя         |
| DELETE | `/api/Teachers/{id}` | Удалить преподавателя          |

## Courses

| Метод  | Endpoint                       | Описание               |
| ------ | ------------------------------ | ---------------------- |
| GET    | `/api/Courses`                 | Получить список курсов |
| GET    | `/api/Courses/{id}`            | Получить курс по ID    |
| GET    | `/api/Courses?search=database` | Поиск курсов           |
| POST   | `/api/Courses`                 | Создать курс           |
| PUT    | `/api/Courses/{id}`            | Обновить курс          |
| DELETE | `/api/Courses/{id}`            | Удалить курс           |

## Enrollments

| Метод  | Endpoint                | Описание                         |
| ------ | ----------------------- | -------------------------------- |
| GET    | `/api/Enrollments`      | Получить список записей на курсы |
| GET    | `/api/Enrollments/{id}` | Получить запись по ID            |
| POST   | `/api/Enrollments`      | Записать студента на курс        |
| PUT    | `/api/Enrollments/{id}` | Изменить оценку                  |
| DELETE | `/api/Enrollments/{id}` | Удалить запись                   |

