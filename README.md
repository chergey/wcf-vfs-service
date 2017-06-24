# Краткое описание
- Client - консольный клиент, через ChannelFactory<T>
- Service - WCF-сервис с реализованной ВФС
- WinService - windows-служба
- СonsoleHost - консольный хост
- Tests - тесты для ВФС (не для сервиса)

#Использование
- Работа в ФВС проиcходит относительно корневой директории Root (=c:), т.е. Root.DeleteFile("directory\subdirectory\file.txt"). При таком подходе, правда, теряется полный путь в исключениях (когда делаем рекурсивный обход директории)
- Можно также найти нужную директорию использую метод TraverseSubdirs и вызывать DeleteFile (dir=TraverseSubdirs(...), dir.DeleteFile("file.txt")


# Замеченные баги
- Некорретно работает отладка из-за ссылки на CNeptune (ссылка в инспектируемом проекте на него необходима для работы NConcern) - не работает Step Into, переменные члены класса не отображаются в Autos и Locals
