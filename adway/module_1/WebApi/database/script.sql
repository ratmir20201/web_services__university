INSERT INTO "Teachers"
    ("FirstName", "LastName", "Email", "Department")
VALUES
    ('Иван', 'Петров', 'ivan.petrov@example.com', 'Информатика'),
    ('Анна', 'Смирнова', 'anna.smirnova@example.com', 'Математика');

INSERT INTO "Students"
    ("FirstName", "LastName", "Email", "BirthDate", "CreatedAt")
VALUES
    ('Алексей', 'Иванов', 'alexey.ivanov@example.com', '2004-05-15', NOW()),
    ('Мария', 'Соколова', 'maria.sokolova@example.com', '2003-11-20', NOW());