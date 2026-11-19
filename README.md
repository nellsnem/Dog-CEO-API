# Dog-CEO-API

Консольний застосунок на C#, який отримує випадкові фото собак за обраною породою через публічний REST API, зберігає дані у реляційну базу даних PostgreSQL (з униканням дублікатів) та виводить збережені записи.

## Опис API

Для роботи використано безкоштовний публічний **[Dog CEO API](https://dog.ceo/dog-api/)**.

* **Запит:** `GET https://dog.ceo/api/breed/{breed}/images/random/3`
* **Що робить:** замість `{breed}` підставляється назва породи англійською (наприклад, `pug`, `husky`, `akita`). У відповідь сервер повертає JSON зі списком із 3 випадкових посилань на фото.
* **Обробка помилок:** якщо немає інтернету, введено неіснуючу породу або прийшла порожня відповідь — програма не падає, а виводить зрозуміле повідомлення в консоль.

## Кроки запуску

1. **Створити базу даних у PostgreSQL** (наприклад, ApiLabDB) та виконати цей SQL-скрипт у pgAdmin, щоб створити дві пов'язані таблиці:
   ```sql
   CREATE TABLE IF NOT EXISTS Breeds (
       Id SERIAL PRIMARY KEY,
       Name VARCHAR(100) UNIQUE NOT NULL
   );

   CREATE TABLE IF NOT EXISTS Photos (
       Id SERIAL PRIMARY KEY,
       ImageUrl TEXT UNIQUE NOT NULL,
       BreedId INT NOT NULL REFERENCES Breeds(Id) ON DELETE CASCADE
   );
## Скриншоти роботи
<img width="846" height="512" alt="image" src="https://github.com/user-attachments/assets/869e38bb-0f17-447f-aab0-8ec1ae15477c" />
<img width="671" height="262" alt="image" src="https://github.com/user-attachments/assets/c0a3cf82-6027-43f9-bbc3-55b2649c5795" />
<img width="825" height="575" alt="image" src="https://github.com/user-attachments/assets/37c94cff-f2fe-40ce-8c01-ecc73c7470c2" />
