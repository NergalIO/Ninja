# Ninja

2D stealth-экшен на Unity с упором на незаметное прохождение, читаемую архитектуру и воспроизводимую систему врагов.

## Обзор
- Игрок управляет ниндзя и скрывается от охраны, комбинируя тень, шум и окружение.
- Противники используют поле зрения и слышимость, чтобы патрулировать, преследовать и обследовать локацию.
- Сценарии запуска и UI-слои построены модульно, поэтому проект легко расширяется контентом и новыми механиками.

## Ключевые особенности
**Искусственный интеллект**
- Три основных состояния: Patrol → Chase → Search, переключение управляется событиями FOV и шумами.
- Поддержка NavMesh для обхода препятствий и гибкая настройка точек патруля.

**Иммерсивные системы**
- Поле зрения с динамическим радиусом и углом обзора.
- Система шума, реагирующая на действия игрока и окружение.
- Расширяемая система звука (музыка, SFX, групповые уровни громкости).

**Игровой фреймворк**
- Менеджер игры с паузой, сценарием загрузки и бэкендом настроек.
- UI-слои для меню, паузы, загрузки и внутриигровых подсказок.
- Поддержка Unity Input System и Cinemachine для современного управления и камеры.

## Технологический стек
- **Unity 2023.2+**, C# 10.
- **URP 2D**, **Cinemachine 3.1.5**, **NavMesh Components**.
- **Unity Input System 1.14.2**.
- Пакеты 2D (Animation, Tilemap, Aseprite), Timeline, Visual Scripting.

## Структура проекта
```
Assets/
├── Scripts/
│   ├── Core/              # Базовые утилиты, синглтоны
│   ├── Gameplay/
│   │   ├── Enemy/         # EnemyController, FieldOfView, патрули
│   │   ├── Levels/        # Логика уровней и триггеры
│   │   └── Player/        # Движение, камера, шум
│   ├── Input/             # Конфигурация Input System
│   ├── Systems/
│   │   ├── Loader/        # Асинхронная загрузка сцен
│   │   ├── Settings/      # Пользовательские настройки
│   │   └── Sound/         # Музыка и эффекты
│   └── UI/                # Меню, HUD, окна паузы
├── Scenes/
│   ├── Menu.unity
│   └── Test.unity
└── Resources/
    └── Sounds/
```

## Как запустить
### Требования
- Unity 2023.2 или новее.
- Windows/macOS/Linux, IDE: Rider или Visual Studio 2022.

### Быстрый старт
1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/NergalIO/Ninja.git
   cd Ninja
   ```
2. Добавьте папку проекта в Unity Hub и дождитесь импорта пакетов.
3. Откройте `Assets/Scenes/Menu.unity` или `Assets/Scenes/Test.unity`.
4. Нажмите Play — сцены готовы к запуску без дополнительной настройки.

## Управление по умолчанию
- `WASD` / стрелки — перемещение.
- Мышь — камера и взаимодействие.
- `ESC` — пауза и меню.

## Настройка и расширение
**Враги (`EnemyController`)**
- `Patrol Points`, `Patrol/Chase Speed`, `View Radius/Angle`, `Lose Target Time`, `Time To Forget Target`.

**Аудио (`Assets/Resources/Sounds/`)**
- `Music/` для фоновых треков, `SFX/` для эффектов.
- Крупные файлы (>50 МБ) храните через Git LFS.

**Настройки игрока и UI**
- Все параметры вынесены в ScriptableObject/Inspector, что упрощает эксперименты с балансом и UX.

## Заметки по разработке
- Архитектура: `Singleton` для менеджеров, `State` для AI, `Observer` для UI событий.
- Нейминг неймспейсов соответствует структуре `Ninja.*`.
- Поля, доступные в Inspector, помечаются `[SerializeField]`.

## Известные ограничения
- Большие аудиофайлы исключены `.gitignore`.
- Папка `Library/` генерируется Unity и не входит в репозиторий.

## Лицензия
Проект закрыт, все права защищены.

## Авторы и ссылки
- **NergalIO** — разработчик.
- Полезные материалы: [Unity Docs](https://docs.unity3d.com/), [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/index.html), [Cinemachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/index.html), [URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.2/manual/index.html).

---

**Версия:** 1.0.0  
**Последнее обновление:** 2024