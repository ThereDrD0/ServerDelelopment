# ServerDevelopment

Решения контрольных работ сделаны на C# / ASP.NET Core. Структура сохранена: каждое задание лежит в отдельном проекте.

Для запуска нужен .NET SDK 10. Проверить версию:

```powershell
dotnet --version
```

Установить зависимости и собрать всё решение:

```powershell
dotnet restore
dotnet build
```

Python-файлы `requirements.txt` или `pyproject.toml` в оригинальном ТЗ нужны для FastAPI. В этой версии зависимости указаны в каждом `.csproj`, а `requirements.txt` оставлен как формальная отметка, что Python-зависимости не используются.

## Контрольная 2

Проекты: `Second.Task1` - `Second.Task7`.

Запуск любого задания:

```powershell
dotnet run --project Second.Task1
```

Основные проверки:

```powershell
curl -X POST http://localhost:5064/create_user -H "Content-Type: application/json" -d "{\"name\":\"Alice\",\"email\":\"alice@example.com\",\"age\":30,\"is_subscribed\":true}"
curl "http://localhost:5004/product/123"
curl "http://localhost:5004/products/search?keyword=phone&category=Electronics&limit=5"
curl -X POST http://localhost:5005/login -H "Content-Type: application/json" -d "{\"username\":\"user123\",\"password\":\"password123\"}" -c cookies.txt
curl http://localhost:5005/user -b cookies.txt
curl http://localhost:5008/headers -H "User-Agent: Mozilla/5.0" -H "Accept-Language: en-US,en;q=0.9"
curl http://localhost:5009/info -H "User-Agent: Mozilla/5.0" -H "Accept-Language: en-US,en;q=0.9"
```

Для заданий с cookie сначала выполните `/login`, затем используйте cookie при запросе к защищённому маршруту.

## Контрольная 3

Проекты: `Third.Task1` - `Third.Task8`.

Запуск:

```powershell
dotnet run --project Third.Task1
```

Основные проверки:

```powershell
curl -u user:password http://localhost:5010/login
curl -X POST http://localhost:5011/register -H "Content-Type: application/json" -d "{\"username\":\"user1\",\"password\":\"correctpass\"}"
curl -u user1:correctpass http://localhost:5011/login
curl -u docs:password http://localhost:5012/docs
curl -X POST http://localhost:5013/login -H "Content-Type: application/json" -d "{\"username\":\"john_doe\",\"password\":\"securepassword123\"}"
curl -X POST http://localhost:5014/register -H "Content-Type: application/json" -d "{\"username\":\"alice\",\"password\":\"qwerty123\"}"
curl -X POST http://localhost:5015/register -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"123\",\"role\":\"admin\"}"
curl -X POST http://localhost:5016/register -H "Content-Type: application/json" -d "{\"username\":\"test_user\",\"password\":\"12345\"}"
curl -X POST http://localhost:5017/todos -H "Content-Type: application/json" -d "{\"title\":\"Buy groceries\",\"description\":\"Milk, eggs, bread\"}"
```

Переменные окружения используются в `Third.Task3`:

```powershell
$env:MODE="DEV"
$env:DOCS_USER="docs"
$env:DOCS_PASSWORD="password"
dotnet run --project Third.Task3
```

В режиме `PROD` документация должна отдавать 404:

```powershell
$env:MODE="PROD"
dotnet run --project Third.Task3
```

## Контрольная 4

Проекты: `Fourth.Task1` - `Fourth.Task5`, тесты: `Fourth.Task4.Tests`, `Fourth.Task5.Tests`.

Запуск:

```powershell
dotnet run --project Fourth.Task1
```

Основные проверки:

```powershell
curl http://localhost:5018/products
curl http://localhost:5019/check
curl http://localhost:5019/resources/2
curl -X POST http://localhost:5020/users -H "Content-Type: application/json" -d "{\"username\":\"user\",\"age\":19,\"email\":\"user@example.com\",\"password\":\"password123\"}"
curl -X POST http://localhost:5021/users -H "Content-Type: application/json" -d "{\"username\":\"user\",\"age\":20}"
curl -X POST http://localhost:5022/users -H "Content-Type: application/json" -d "{\"username\":\"user\",\"age\":20}"
```

Миграции в `Fourth.Task1` применяются автоматически при старте проекта. База SQLite создаётся рядом с собранным приложением.

Запуск тестов:

```powershell
dotnet test Fourth.Task4.Tests\Fourth.Task4.Tests.csproj
dotnet test Fourth.Task5.Tests\Fourth.Task5.Tests.csproj
```

## Переменные окружения

Пример лежит в `.env.example`. Реальный `.env` не публикуется и уже добавлен в `.gitignore`.

## Быстрая проверка всего

```powershell
dotnet restore
dotnet build
dotnet test Fourth.Task4.Tests\Fourth.Task4.Tests.csproj
dotnet test Fourth.Task5.Tests\Fourth.Task5.Tests.csproj
```
