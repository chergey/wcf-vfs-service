# Краткое описание
- Client - консольный клиент, через ChannelFactory<T>
- Service - WCF-сервис с реализованной ВФС
- WinService - windows-служба
- СonsoleHost - консольный хост
- Tests - тесты для ВФС (не для сервиса)

#Использование
 - Работа в ФВС проиcходит относительно корневой директории Root (=c:), т.е. Root.DeleteFile("directory\subdirectory\file.txt")
- Можно также найти нужную директорию использую метод TraverseSubdirs
и вызывать DeleteFile("file.txt")

# Замеченные баги
- Некорретно работает отладка из-за ссылки на CNeptune (необходим для работы NConcern) - не работает Step Into, переменные члены класса не отображаются в Autos и Locals


