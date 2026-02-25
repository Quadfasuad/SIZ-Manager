# ТЕХНИЧЕСКОЕ ЗАДАНИЕ
## Программа "СИЗ Менеджер" (SIZ Manager)

**Версия ТЗ:** 1.0  
**Дата:** 23.02.2026  
**Разработчик:** [Ваше имя]

---

## 📋 СОДЕРЖАНИЕ

1. [Общее описание проекта](#1-общее-описание-проекта)
2. [Цели и задачи](#2-цели-и-задачи)
3. [Технологический стек](#3-технологический-стек)
4. [Функциональные требования](#4-функциональные-требования)
5. [Структура данных](#5-структура-данных)
6. [База данных SQLite](#6-база-данных-sqlite)
7. [Интерфейс пользователя](#7-интерфейс-пользователя)
8. [Логика работы](#8-логика-работы)
9. [Импорт и экспорт](#9-импорт-и-экспорт)
10. [Нефункциональные требования](#10-нефункциональные-требования)
11. [Структура проекта](#11-структура-проекта)
12. [Этапы разработки](#12-этапы-разработки)
13. [Критерии приемки](#13-критерии-приемки)

---

## 1. ОБЩЕЕ ОПИСАНИЕ ПРОЕКТА

### 1.1. Назначение

**СИЗ Менеджер** — desktop приложение для Windows, предназначенное для автоматизации заполнения личных карточек учета выдачи средств индивидуальной защиты (СИЗ) сотрудников на основе нормативного справочника профессий.

### 1.2. Основание

Программа создается на основе нормативных документов:
- **Приказ Минтруда РФ от 29.10.2021 N 767н** "Об утверждении Единых типовых норм выдачи средств индивидуальной защиты и смывающих средств"
- **Приложение N 1** — Единые типовые нормы выдачи СИЗ по профессиям (должностям)
- **Приложение N 2** — Форма личной карточки учета выдачи СИЗ

### 1.3. Целевая аудитория

- Специалисты по охране труда
- Кадровые работники
- Руководители предприятий

### 1.4. Тип приложения

- **Платформа:** Windows 10/11
- **Тип:** Desktop приложение (standalone)
- **Распространение:** Portable (не требует установки)

---

## 2. ЦЕЛИ И ЗАДАЧИ

### 2.1. Основная цель

Автоматизировать процесс создания и заполнения личных карточек учета выдачи СИЗ, исключив ручной ввод списка СИЗ для каждой профессии.

### 2.2. Задачи

1. **Импорт справочника** профессий и СИЗ из JSON файла
2. **Хранение данных** в локальной базе данных SQLite
3. **Создание карточек** сотрудников с автоматическим заполнением СИЗ по профессии
4. **Экспорт карточек** в форматы DOCX, PDF, Excel
5. **Управление данными** — добавление, редактирование, поиск карточек
6. **Обновление справочника** с сохранением резервных копий

---

## 3. ТЕХНОЛОГИЧЕСКИЙ СТЕК

### 3.1. Основные технологии

| Компонент | Технология | Версия |
|-----------|-----------|--------|
| **Язык программирования** | C# | 12.0+ |
| **Framework** | .NET | 8.0 (рекомендуется) или .NET Framework 4.8 |
| **UI Framework** | WPF (Windows Presentation Foundation) | - |
| **Архитектурный паттерн** | MVVM (Model-View-ViewModel) | - |
| **База данных** | SQLite | 3.x |
| **ORM** | Entity Framework Core | 8.x |

### 3.2. Библиотеки

#### Обязательные:
```xml
<!-- Работа с базой данных -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />

<!-- Работа с документами -->
<PackageReference Include="DocX" Version="2.5.0" />
<!-- или -->
<PackageReference Include="DocumentFormat.OpenXml" Version="3.0.0" />

<!-- Генерация PDF -->
<PackageReference Include="QuestPDF" Version="2024.1.0" />

<!-- Работа с Excel -->
<PackageReference Include="EPPlus" Version="7.0.0" />
<!-- или -->
<PackageReference Include="ClosedXML" Version="0.102.0" />

<!-- JSON -->
System.Text.Json (встроено в .NET)
```

#### Опциональные:
```xml
<!-- MVVM помощник -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />

<!-- Современный UI -->
<PackageReference Include="MaterialDesignThemes" Version="5.0.0" />
```

---

## 4. ФУНКЦИОНАЛЬНЫЕ ТРЕБОВАНИЯ

### 4.1. Управление справочником СИЗ

#### FR-1.1: Импорт справочника из JSON

**Описание:** Пользователь может импортировать справочник профессий и СИЗ из JSON файла.

**Поток действий:**
1. Пользователь выбирает меню "Справочник" → "Импорт из JSON"
2. Открывается диалог выбора файла (*.json)
3. Программа читает и валидирует JSON
4. Создается резервная копия текущей БД
5. Данные импортируются в SQLite
6. Показывается результат импорта

**Валидация:**
- Наличие обязательных полей: `metadata`, `professions`
- Корректность структуры JSON
- Не пустой список профессий
- Каждая профессия имеет `name` и `siz_list`

**Входные данные:** JSON файл (формат см. раздел 5.1)

**Результат:**
- База данных обновлена
- Резервная копия создана
- Сообщение пользователю: "Импортировано профессий: N, СИЗ: M"

#### FR-1.2: Просмотр справочника

**Описание:** Пользователь может просмотреть текущий справочник профессий.

**Функции:**
- Список всех профессий (поиск по названию)
- Просмотр СИЗ для выбранной профессии
- Количество профессий
- Версия справочника
- Дата последнего обновления

#### FR-1.3: Экспорт справочника в JSON

**Описание:** Создание резервной копии справочника в JSON формате.

**Результат:** JSON файл с текущими данными из БД

#### FR-1.4: Управление резервными копиями

**Требования:**
- Автоматическое создание бэкапа БД перед импортом
- Формат имени: `database_backup_YYYY-MM-DD_HH-mm-ss.db`
- Хранение последних 5 резервных копий
- Автоматическое удаление старых копий
- Возможность восстановления из бэкапа

---

### 4.2. Работа с карточками сотрудников

#### FR-2.1: Создание новой карточки

**Поля карточки:**

| Группа | Поле | Тип | Обязательное |
|--------|------|-----|-------------|
| **Основные данные** |
| | Номер карточки | String | Нет |
| | Фамилия | String | **Да** |
| | Имя | String | **Да** |
| | Отчество | String | Нет |
| | Пол | Enum (М/Ж) | Нет |
| **Служебные данные** |
| | Табельный номер | String | Нет |
| | Структурное подразделение | String | Нет |
| | Профессия (должность) | String | **Да** |
| | Дата поступления на работу | Date | Нет |
| | Дата изменения профессии | Date | Нет |
| **Размеры** |
| | Рост | Integer | Нет |
| | Размер одежды | String | Нет |
| | Размер обуви | String | Нет |
| | Размер головного убора | String | Нет |
| | Размер СИЗОД | String | Нет |
| | Размер СИЗ рук | String | Нет |

#### FR-2.2: Автозаполнение СИЗ

**Описание:** При выборе профессии автоматически заполняется таблица СИЗ.

**Поток действий:**
1. Пользователь начинает вводить профессию в поле "Профессия (должность)"
2. Появляется выпадающий список с подсказками (autocomplete)
3. Пользователь выбирает профессию
4. Программа находит профессию в БД
5. **Автоматически заполняет таблицу СИЗ** данными из справочника
6. Пользователь может вручную добавить/удалить/изменить СИЗ

**Важно:** 
- СИЗ загружаются из справочника, но хранятся для конкретного сотрудника
- Изменения в справочнике не влияют на уже созданные карточки

#### FR-2.3: Ручное редактирование СИЗ

**Функции:**
- Добавить новое СИЗ
- Изменить существующее СИЗ
- Удалить СИЗ
- Изменить норму выдачи

#### FR-2.4: Сохранение карточки

**Требования:**
- Валидация обязательных полей (ФИО, Профессия)
- Сохранение в БД SQLite
- Автоматическая генерация ID
- Сохранение даты создания/изменения

#### FR-2.5: Поиск и редактирование карточек

**Функции:**
- Список всех карточек
- Поиск по ФИО
- Поиск по табельному номеру
- Поиск по профессии
- Открытие карточки для редактирования
- Удаление карточки

---

### 4.3. Экспорт карточек

#### FR-3.1: Экспорт в DOCX

**Описание:** Создание Word документа личной карточки на основе шаблона.

**Требования:**
- Использование шаблона `card_template_with_placeholders.docx`
- Замена всех плейсхолдеров данными сотрудника
- Заполнение таблицы СИЗ
- Сохранение форматирования
- Две страницы: лицевая + оборотная сторона

**Формат имени файла:** `Карточка_{Фамилия}_{Имя}.docx`

#### FR-3.2: Экспорт в PDF

**Описание:** Создание PDF документа для печати.

**Варианты реализации:**
1. DOCX → PDF конвертация
2. Генерация PDF через QuestPDF

**Формат имени файла:** `Карточка_{Фамилия}_{Имя}.pdf`

#### FR-3.3: Экспорт в Excel

**Описание:** Создание Excel файла с данными карточки.

**Содержание:**
- Лист 1: Персональные данные
- Лист 2: Таблица СИЗ

**Формат имени файла:** `Карточка_{Фамилия}_{Имя}.xlsx`

---

## 5. СТРУКТУРА ДАННЫХ

### 5.1. Формат JSON для импорта

**Файл:** `siz_database_full.json` (4.06 МБ, 2988 профессий)

```json
{
  "metadata": {
    "version": "1.0",
    "date": "2026-02-23",
    "source": "Приказ Минтруда 767н от 29.10.2021",
    "description": "Единые типовые нормы выдачи СИЗ",
    "total_professions": 2988
  },
  "professions": [
    {
      "number": "1",
      "name": "Авербандщик",
      "siz_list": [
        {
          "type": "Одежда специальная защитная",
          "name": "Костюм для защиты от механических воздействий (истирания)",
          "norm": "1 шт."
        },
        {
          "type": "Средства защиты рук",
          "name": "Перчатки для защиты от механических воздействий (истирания)",
          "norm": "12 пар"
        }
      ]
    }
  ]
}
```

**Обязательные поля:**

| Путь | Тип | Описание |
|------|-----|----------|
| `metadata.version` | String | Версия формата данных |
| `metadata.date` | String (Date) | Дата создания справочника |
| `professions` | Array | Массив профессий |
| `professions[].number` | String | Номер профессии |
| `professions[].name` | String | Название профессии |
| `professions[].siz_list` | Array | Массив СИЗ |
| `siz_list[].type` | String | Тип СИЗ |
| `siz_list[].name` | String | Наименование СИЗ |
| `siz_list[].norm` | String | Норма выдачи |

---

## 6. БАЗА ДАННЫХ SQLite

### 6.1. Схема базы данных

**Файл:** `database.db` (создается автоматически в папке программы)

#### Таблица: `Professions`

```sql
CREATE TABLE Professions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Number TEXT,                    -- Номер профессии (1, 2, 3...)
    Name TEXT NOT NULL UNIQUE,      -- Название профессии
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_profession_name ON Professions(Name);
```

#### Таблица: `ProfessionSIZ`

```sql
CREATE TABLE ProfessionSIZ (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProfessionId INTEGER NOT NULL,  -- FK → Professions.Id
    Type TEXT NOT NULL,              -- Тип СИЗ
    Name TEXT NOT NULL,              -- Наименование СИЗ
    Norm TEXT NOT NULL,              -- Норма выдачи
    FOREIGN KEY (ProfessionId) REFERENCES Professions(Id) ON DELETE CASCADE
);

CREATE INDEX idx_profession_siz_profession ON ProfessionSIZ(ProfessionId);
```

#### Таблица: `Employees`

```sql
CREATE TABLE Employees (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Номер карточки
    CardNumber TEXT,
    
    -- Основные данные
    LastName TEXT NOT NULL,
    FirstName TEXT NOT NULL,
    MiddleName TEXT,
    Gender TEXT,                     -- 'М' или 'Ж'
    
    -- Служебные данные
    PersonnelNumber TEXT,            -- Табельный номер
    Department TEXT,                 -- Структурное подразделение
    ProfessionId INTEGER,            -- FK → Professions.Id
    ProfessionName TEXT NOT NULL,    -- Дублируем для истории
    HireDate DATE,                   -- Дата поступления
    ChangeDate DATE,                 -- Дата изменения профессии
    
    -- Размеры
    Height INTEGER,
    ClothingSize TEXT,
    ShoeSize TEXT,
    HeadwearSize TEXT,
    RespiratorsSize TEXT,
    GlovesSize TEXT,
    
    -- Метаданные
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (ProfessionId) REFERENCES Professions(Id) ON DELETE SET NULL
);

CREATE INDEX idx_employee_lastname ON Employees(LastName);
CREATE INDEX idx_employee_personnel ON Employees(PersonnelNumber);
```

#### Таблица: `EmployeeSIZ`

```sql
CREATE TABLE EmployeeSIZ (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EmployeeId INTEGER NOT NULL,    -- FK → Employees.Id
    Type TEXT NOT NULL,
    Name TEXT NOT NULL,
    Norm TEXT NOT NULL,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
);

CREATE INDEX idx_employee_siz_employee ON EmployeeSIZ(EmployeeId);
```

### 6.2. Модели C# (EF Core)

```csharp
public class Profession
{
    public int Id { get; set; }
    public string Number { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public virtual ICollection<ProfessionSIZ> SIZList { get; set; }
}

public class ProfessionSIZ
{
    public int Id { get; set; }
    public int ProfessionId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string Norm { get; set; }
    
    // Navigation property
    public virtual Profession Profession { get; set; }
}

public class Employee
{
    public int Id { get; set; }
    
    // Карточка
    public string CardNumber { get; set; }
    
    // ФИО
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string Gender { get; set; }
    
    // Служебные
    public string PersonnelNumber { get; set; }
    public string Department { get; set; }
    public int? ProfessionId { get; set; }
    public string ProfessionName { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    
    // Размеры
    public int? Height { get; set; }
    public string ClothingSize { get; set; }
    public string ShoeSize { get; set; }
    public string HeadwearSize { get; set; }
    public string RespiratorsSize { get; set; }
    public string GlovesSize { get; set; }
    
    // Метаданные
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Profession Profession { get; set; }
    public virtual ICollection<EmployeeSIZ> SIZList { get; set; }
}

public class EmployeeSIZ
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string Norm { get; set; }
    
    // Navigation property
    public virtual Employee Employee { get; set; }
}
```

### 6.3. DbContext

```csharp
public class SizDbContext : DbContext
{
    public DbSet<Profession> Professions { get; set; }
    public DbSet<ProfessionSIZ> ProfessionSIZ { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeSIZ> EmployeeSIZ { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=database.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка индексов, ограничений и т.д.
        modelBuilder.Entity<Profession>()
            .HasIndex(p => p.Name)
            .IsUnique();
            
        // ... остальные настройки
    }
}
```

---

## 7. ИНТЕРФЕЙС ПОЛЬЗОВАТЕЛЯ

### 7.1. Главное окно

```
┌─────────────────────────────────────────────────────────────┐
│  СИЗ Менеджер v1.0                          [_][□][X]       │
├─────────────────────────────────────────────────────────────┤
│  [Файл ▼] [Справочник ▼] [Помощь ▼]                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌────────────────────────────────────────────────────┐    │
│  │ СПРАВОЧНИК СИЗ                                     │    │
│  │ Версия: 1.0 (от 29.10.2021)                        │    │
│  │ Профессий в базе: 2988                             │    │
│  │ Последнее обновление: 23.02.2026                   │    │
│  │                                                    │    │
│  │ [📁 Импорт справочника из JSON...]                │    │
│  │ [💾 Экспорт справочника в JSON...]                │    │
│  └────────────────────────────────────────────────────┘    │
│                                                             │
│  ┌────────────────────────────────────────────────────┐    │
│  │ ДАННЫЕ СОТРУДНИКА                                  │    │
│  │                                                    │    │
│  │ Номер карточки: [____________]                     │    │
│  │                                                    │    │
│  │ Фамилия:  [_________________________________]      │    │
│  │ Имя:      [_________________________________]      │    │
│  │ Отчество: [_________________________________]      │    │
│  │ Пол: [○ М  ○ Ж]                                   │    │
│  │                                                    │    │
│  │ Табельный номер:      [____________]               │    │
│  │ Структурное подразделение: [____________]          │    │
│  │                                                    │    │
│  │ Профессия (должность):                             │    │
│  │ [▼ Начните вводить для поиска...          ]🔍     │    │
│  │                                                    │    │
│  │ Дата поступления: [__.__.____]                     │    │
│  │                                                    │    │
│  │ ┌─ Размеры ─────────────────────────────────┐     │    │
│  │ │ Рост: [____]  Одежда: [___]               │     │    │
│  │ │ Обувь: [___]  Головной убор: [___]        │     │    │
│  │ │ СИЗОД: [___]  СИЗ рук: [___]              │     │    │
│  │ └───────────────────────────────────────────┘     │    │
│  └────────────────────────────────────────────────────┘    │
│                                                             │
│  ┌────────────────────────────────────────────────────┐    │
│  │ СИЗ ДЛЯ ДАННОЙ ПРОФЕССИИ                          │    │
│  │ ┌──────────────────────────────────────────────┐  │    │
│  │ │Тип СИЗ          │Наименование  │Норма выдачи│  │    │
│  │ ├──────────────────────────────────────────────┤  │    │
│  │ │Одежда...        │Костюм...     │1 шт.       │  │    │
│  │ │Средства защиты  │Перчатки...   │12 пар      │  │    │
│  │ │...              │...           │...         │  │    │
│  │ └──────────────────────────────────────────────┘  │    │
│  │                                                    │    │
│  │ [+ Добавить СИЗ] [✎ Изменить] [✕ Удалить]        │    │
│  └────────────────────────────────────────────────────┘    │
│                                                             │
│  [🆕 Новая карточка] [💾 Сохранить]                        │
│  [📋 Список карточек...] [📄 Экспорт в... ▼]               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 7.2. Меню

#### Файл
- Новая карточка (Ctrl+N)
- Открыть карточку... (Ctrl+O)
- Сохранить (Ctrl+S)
- ---
- Экспорт в DOCX...
- Экспорт в PDF...
- Экспорт в Excel...
- ---
- Выход

#### Справочник
- Импорт справочника из JSON...
- Экспорт справочника в JSON...
- ---
- Просмотр справочника профессий
- ---
- Управление резервными копиями

#### Помощь
- Инструкция по использованию
- Формат JSON для импорта
- ---
- О программе

### 7.3. Окно "Список карточек"

```
┌───────────────────────────────────────────────────────┐
│  Список карточек сотрудников        [_][□][X]        │
├───────────────────────────────────────────────────────┤
│  Поиск: [_________________________________] 🔍        │
├───────────────────────────────────────────────────────┤
│ №  │ ФИО              │ Профессия     │ Таб. №       │
├────┼──────────────────┼───────────────┼──────────────┤
│ 1  │ Иванов И.И.      │ Электрик      │ 12345        │
│ 2  │ Петров П.П.      │ Слесарь       │ 12346        │
│ ...│ ...              │ ...           │ ...          │
└───────────────────────────────────────────────────────┘
  [Открыть] [Редактировать] [Удалить] [Закрыть]
```

---

## 8. ЛОГИКА РАБОТЫ

### 8.1. Автозаполнение СИЗ при выборе профессии

**Алгоритм:**

```csharp
private void OnProfessionSelected(Profession profession)
{
    // 1. Очищаем текущий список СИЗ сотрудника
    CurrentEmployee.SIZList.Clear();
    
    // 2. Загружаем СИЗ для выбранной профессии из БД
    var professionSIZ = _dbContext.ProfessionSIZ
        .Where(s => s.ProfessionId == profession.Id)
        .ToList();
    
    // 3. Копируем СИЗ в карточку сотрудника
    foreach (var siz in professionSIZ)
    {
        CurrentEmployee.SIZList.Add(new EmployeeSIZ
        {
            Type = siz.Type,
            Name = siz.Name,
            Norm = siz.Norm
        });
    }
    
    // 4. Обновляем таблицу в UI
    RefreshSIZTable();
}
```

### 8.2. Импорт справочника из JSON

**Алгоритм:**

```csharp
private async Task ImportFromJsonAsync(string filePath)
{
    // 1. Создать резервную копию БД
    await BackupService.CreateBackupAsync();
    
    // 2. Прочитать JSON
    var json = await File.ReadAllTextAsync(filePath);
    var data = JsonSerializer.Deserialize<SizDatabase>(json);
    
    // 3. Валидация
    if (!ValidateJsonData(data))
        throw new Exception("Некорректный формат JSON");
    
    // 4. Начать транзакцию
    using var transaction = _dbContext.Database.BeginTransaction();
    
    try
    {
        // 5. Очистить старые данные
        _dbContext.Professions.RemoveRange(_dbContext.Professions);
        _dbContext.ProfessionSIZ.RemoveRange(_dbContext.ProfessionSIZ);
        
        // 6. Импортировать новые данные
        foreach (var profData in data.Professions)
        {
            var profession = new Profession
            {
                Number = profData.Number,
                Name = profData.Name
            };
            
            _dbContext.Professions.Add(profession);
            await _dbContext.SaveChangesAsync(); // Получить ID
            
            foreach (var sizData in profData.SizList)
            {
                _dbContext.ProfessionSIZ.Add(new ProfessionSIZ
                {
                    ProfessionId = profession.Id,
                    Type = sizData.Type,
                    Name = sizData.Name,
                    Norm = sizData.Norm
                });
            }
        }
        
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // 7. Показать результат
        ShowSuccess($"Импортировано: {data.Professions.Count} профессий");
    }
    catch
    {
        await transaction.RollbackAsync();
        await BackupService.RestoreLastBackupAsync();
        throw;
    }
}
```

### 8.3. Генерация DOCX документа

**Алгоритм:**

```csharp
private void GenerateDocxCard(Employee employee, string outputPath)
{
    // 1. Загрузить шаблон
    using var doc = DocX.Load("card_template_with_placeholders.docx");
    
    // 2. Заменить простые плейсхолдеры
    doc.ReplaceText("{CardNumber}", employee.CardNumber ?? "");
    doc.ReplaceText("{LastName}", employee.LastName);
    doc.ReplaceText("{FirstName}", employee.FirstName);
    doc.ReplaceText("{MiddleName}", employee.MiddleName ?? "");
    doc.ReplaceText("{Gender}", employee.Gender ?? "");
    doc.ReplaceText("{Height}", employee.Height?.ToString() ?? "");
    doc.ReplaceText("{PersonnelNumber}", employee.PersonnelNumber ?? "");
    doc.ReplaceText("{Department}", employee.Department ?? "");
    doc.ReplaceText("{Profession}", employee.ProfessionName);
    doc.ReplaceText("{HireDate}", employee.HireDate?.ToString("dd.MM.yyyy") ?? "");
    doc.ReplaceText("{ChangeDate}", employee.ChangeDate?.ToString("dd.MM.yyyy") ?? "");
    
    // Размеры
    doc.ReplaceText("{ClothingSize}", employee.ClothingSize ?? "");
    doc.ReplaceText("{ShoeSize}", employee.ShoeSize ?? "");
    doc.ReplaceText("{HeadwearSize}", employee.HeadwearSize ?? "");
    doc.ReplaceText("{RespiratorsSize}", employee.RespiratorsSize ?? "");
    doc.ReplaceText("{GlovesSize}", employee.GlovesSize ?? "");
    
    // 3. Заполнить таблицу СИЗ
    FillSizTable(doc, employee.SIZList);
    
    // 4. Сохранить
    doc.SaveAs(outputPath);
}

private void FillSizTable(DocX doc, ICollection<EmployeeSIZ> sizList)
{
    // Найти таблицу СИЗ (вторая таблица в документе)
    var sizTable = doc.Tables[1];
    
    // Найти строку с маркером {SIZ_ROW_START}
    int markerRowIndex = FindMarkerRow(sizTable, "{SIZ_ROW_START}");
    
    // Удалить строку с маркером
    sizTable.RemoveRow(markerRowIndex);
    
    // Добавить строки с данными СИЗ
    foreach (var siz in sizList)
    {
        var row = sizTable.InsertRow(markerRowIndex);
        row.Cells[0].Paragraphs[0].Append(siz.Name);
        row.Cells[1].Paragraphs[0].Append(""); // Пункт норм (не храним)
        row.Cells[2].Paragraphs[0].Append(""); // Единица измерения (из norm)
        row.Cells[3].Paragraphs[0].Append(siz.Norm);
        
        markerRowIndex++;
    }
}
```

---

## 9. ИМПОРТ И ЭКСПОРТ

### 9.1. Поддерживаемые форматы

| Направление | Формат | Назначение |
|------------|--------|-----------|
| **Импорт** | JSON | Справочник профессий и СИЗ |
| **Экспорт** | DOCX | Личная карточка (редактируемая) |
| **Экспорт** | PDF | Личная карточка (для печати) |
| **Экспорт** | XLSX | Личная карточка (табличный формат) |
| **Экспорт** | JSON | Резервная копия справочника |

### 9.2. Шаблон DOCX

**Файл:** `card_template_with_placeholders.docx`

**Расположение:** В папке `Templates/` рядом с .exe

**Плейсхолдеры:**

| Плейсхолдер | Назначение |
|-------------|-----------|
| `{CardNumber}` | Номер карточки |
| `{LastName}` | Фамилия |
| `{FirstName}` | Имя |
| `{MiddleName}` | Отчество |
| `{Gender}` | Пол |
| `{Height}` | Рост |
| `{ClothingSize}` | Размер одежды |
| `{ShoeSize}` | Размер обуви |
| `{HeadwearSize}` | Размер головного убора |
| `{RespiratorsSize}` | Размер СИЗОД |
| `{GlovesSize}` | Размер СИЗ рук |
| `{PersonnelNumber}` | Табельный номер |
| `{Department}` | Подразделение |
| `{Profession}` | Профессия |
| `{HireDate}` | Дата поступления |
| `{ChangeDate}` | Дата изменения профессии |
| `{SIZ_ROW_START}` | Маркер для вставки таблицы СИЗ |

---

## 10. НЕФУНКЦИОНАЛЬНЫЕ ТРЕБОВАНИЯ

### 10.1. Производительность

- Импорт 3000 профессий: < 10 секунд
- Поиск профессии по названию: < 1 секунда
- Открытие карточки: < 0.5 секунды
- Генерация DOCX: < 3 секунды

### 10.2. Надежность

- Все операции с БД в транзакциях
- Автоматическое создание резервных копий перед обновлением
- Обработка всех исключений
- Логирование ошибок в файл `errors.log`

### 10.3. Портативность

**Требования:**
- Все файлы программы в одной папке
- База данных в файле `database.db` рядом с .exe
- Шаблоны в папке `Templates/`
- Не требует установки
- Не пишет в реестр Windows
- Работает с USB-флешки

**Структура папки:**

```
SizManager/
├── SizManager.exe
├── database.db
├── settings.json
├── errors.log
├── Templates/
│   └── card_template_with_placeholders.docx
├── Backups/
│   ├── database_backup_2026-02-23_10-00-00.db
│   └── ...
└── (DLL библиотеки)
```

### 10.4. Безопасность

- Валидация всех пользовательских вводов
- Проверка существования файлов перед операциями
- Ограничение размера импортируемых JSON (max 50 МБ)

### 10.5. Юзабилити

- Интуитивно понятный интерфейс
- Клавиатурные сокращения (Ctrl+N, Ctrl+S и т.д.)
- Подсказки (tooltips) для всех кнопок
- Сообщения об ошибках на русском языке
- Autocomplete для поля "Профессия"

---

## 11. СТРУКТУРА ПРОЕКТА

### 11.1. Архитектура (MVVM)

```
SizManager/
├── Models/                          # Модели данных
│   ├── Profession.cs
│   ├── ProfessionSIZ.cs
│   ├── Employee.cs
│   ├── EmployeeSIZ.cs
│   └── JsonModels/
│       ├── SizDatabase.cs
│       └── JsonProfession.cs
│
├── ViewModels/                      # ViewModel (логика)
│   ├── MainViewModel.cs
│   ├── EmployeeCardViewModel.cs
│   ├── ProfessionListViewModel.cs
│   └── EmployeeListViewModel.cs
│
├── Views/                           # View (UI)
│   ├── MainWindow.xaml
│   ├── EmployeeCardView.xaml
│   ├── ProfessionListWindow.xaml
│   └── EmployeeListWindow.xaml
│
├── Services/                        # Сервисы (бизнес-логика)
│   ├── Database/
│   │   └── SizDbContext.cs
│   ├── Import/
│   │   └── JsonImportService.cs
│   ├── Export/
│   │   ├── DocxExportService.cs
│   │   ├── PdfExportService.cs
│   │   └── ExcelExportService.cs
│   ├── BackupService.cs
│   └── ValidationService.cs
│
├── Helpers/                         # Вспомогательные классы
│   ├── RelayCommand.cs
│   ├── DialogService.cs
│   └── NotificationService.cs
│
├── Resources/                       # Ресурсы
│   ├── Styles.xaml
│   └── Icons/
│
├── App.xaml                         # Главный файл приложения
└── App.xaml.cs
```

### 11.2. Зависимости (NuGet)

```xml
<ItemGroup>
  <!-- Database -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
  
  <!-- Documents -->
  <PackageReference Include="DocX" Version="2.5.0" />
  <PackageReference Include="QuestPDF" Version="2024.1.0" />
  <PackageReference Include="EPPlus" Version="7.0.0" />
  
  <!-- MVVM -->
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
</ItemGroup>
```

---

## 12. ЭТАПЫ РАЗРАБОТКИ

### Этап 1: Создание проекта и базовой структуры ✅
**Срок:** 1 день

- [x] Создать WPF проект (.NET 8)
- [x] Настроить структуру папок (Models, Views, ViewModels, Services)
- [x] Подключить NuGet пакеты
- [x] Создать базовый MainWindow

### Этап 2: Модели данных и база данных ✅
**Срок:** 2 дня

- [x] Создать модели (Profession, ProfessionSIZ, Employee, EmployeeSIZ)
- [x] Создать DbContext для SQLite
- [x] Настроить миграции EF Core
- [x] Создать начальную базу данных

### Этап 3: Сервисы импорта/экспорта ✅
**Срок:** 3 дня

- [x] JsonImportService — импорт справочника из JSON
- [x] BackupService — резервные копии БД
- [x] ValidationService — валидация данных
- [x] DocxExportService — экспорт в DOCX

### Этап 4: UI главного окна ✅
**Срок:** 3 дня

- [x] Разметка XAML главного окна
- [x] MainViewModel с привязкой данных
- [x] Форма ввода данных сотрудника
- [x] Autocomplete для профессии
- [x] Таблица СИЗ с редактированием

### Этап 5: Логика автозаполнения СИЗ ✅
**Срок:** 2 дня

- [x] При выборе профессии → загрузка СИЗ из БД
- [x] Заполнение таблицы СИЗ
- [x] Возможность ручного редактирования СИЗ

### Этап 6: Сохранение и загрузка карточек ✅
**Срок:** 2 дня

- [x] Сохранение карточки в БД
- [x] Валидация обязательных полей
- [x] Список всех карточек с поиском
- [x] Редактирование существующих карточек

### Этап 7: Экспорт в документы ✅
**Срок:** 3 дня

- [x] Экспорт в DOCX по шаблону
- [x] Экспорт в PDF
- [x] Экспорт в Excel

### Этап 8: Дополнительные окна и меню ✅
**Срок:** 2 дня

- [x] Окно "Список карточек"
- [x] Окно "Просмотр справочника"
- [x] Окно "О программе"
- [x] Меню и клавиатурные сокращения

### Этап 9: Тестирование и отладка ✅
**Срок:** 3 дня

- [x] Тестирование всех функций
- [x] Исправление багов
- [x] Оптимизация производительности
- [x] Проверка на разных версиях Windows

### Этап 10: Упаковка и документация ✅
**Срок:** 2 дня

- [x] Упаковка в portable версию (Publish Self-Contained)
- [x] Создание инструкции пользователя
- [x] Подготовка релиза

**Итого:** ~23 дня разработки

---

## 13. КРИТЕРИИ ПРИЕМКИ

Программа считается готовой к использованию, если выполнены все следующие критерии:

### 13.1. Функциональность

- ✅ Импортирует JSON справочник (2988 профессий) за < 10 секунд
- ✅ При выборе профессии автоматически заполняет таблицу СИЗ
- ✅ Сохраняет карточки сотрудников в БД
- ✅ Экспортирует карточки в DOCX с правильным форматированием
- ✅ Экспортирует карточки в PDF
- ✅ Экспортирует карточки в Excel
- ✅ Создает резервные копии перед обновлением справочника
- ✅ Ищет карточки по ФИО, табельному номеру, профессии

### 13.2. Надежность

- ✅ Не падает при некорректном вводе данных
- ✅ Обрабатывает ошибки импорта JSON
- ✅ Откатывает изменения при ошибке импорта
- ✅ Логирует ошибки в файл
- ✅ Работает на чистой Windows 10/11 без установки доп. ПО

### 13.3. Портативность

- ✅ Все файлы в одной папке
- ✅ Работает с USB-флешки
- ✅ Не требует прав администратора
- ✅ Не пишет в реестр

### 13.4. Юзабилити

- ✅ Интерфейс понятен без инструкции
- ✅ Autocomplete работает при вводе профессии
- ✅ Клавиатурные сокращения работают (Ctrl+N, Ctrl+S)
- ✅ Сообщения об ошибках понятны пользователю

### 13.5. Производительность

- ✅ Импорт 3000 профессий < 10 сек
- ✅ Поиск профессии < 1 сек
- ✅ Генерация DOCX < 3 сек
- ✅ Размер установочного пакета < 100 МБ

---

## ПРИЛОЖЕНИЯ

### A. Глоссарий

| Термин | Определение |
|--------|-------------|
| **СИЗ** | Средства индивидуальной защиты |
| **СИЗОД** | Средства индивидуальной защиты органов дыхания |
| **ЕТН** | Единые типовые нормы |
| **Portable** | Программа, не требующая установки |
| **MVVM** | Model-View-ViewModel (архитектурный паттерн) |
| **EF Core** | Entity Framework Core (ORM для .NET) |

### B. Ссылки на нормативные документы

- [Приказ Минтруда 767н (КонсультантПлюс)](https://www.consultant.ru/document/cons_doc_LAW_405226/)
- [Приложение N 1 - Нормы СИЗ](https://www.consultant.ru/document/cons_doc_LAW_405226/5c34f76aa431e4a0093d1c1511c1f15e3d0966ab/)
- [Приложение N 2 - Форма карточки](https://www.consultant.ru/document/cons_doc_LAW_405226/)

### C. Контакты

**Разработчик:** [Ваше имя]  
**Email:** [Ваш email]  
**Дата создания ТЗ:** 23.02.2026

---

**КОНЕЦ ТЕХНИЧЕСКОГО ЗАДАНИЯ**
