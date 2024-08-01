# Object Storage Service Dll

这是一个为c# asp.net后端开发的，运行在本地文件系统上的类对象存储文件系统。提供那些不便写在表中内容的文件存取。 支持单例模式、多线程等

This is a dll service for c# asp.net backend with local file system. It will create a likely object storage file system on local file system. Also, it supports Singleton Pattern, multi threads, etc,.

Dies ist ein Klassendateispeichersystem für die Backend-Entwicklung mit C# ASP.NET, das auf dem lokalen Dateisystem ausgeführt wird. Es bietet Zugriff auf Dateien für Inhalte, die sich nicht gut in Tabellen schreiben lassen. Es unterstützt das Singleton-Muster, Mehrthreading und andere Funktionen.

### Documents Supported language

- Chinese Simplify
- English
- German

## 使用方法  

### 1 添加服务

- 将release的dll文件挪到项目某个目录下

- 在Visual Studio中，选中项目的`依赖项`，右击选择添加引用，在`浏览`中选择刚刚放入的dll文件，成功添加程序集引用
- 在`Program.cs`中添加以下代码注册

```c#
 public static IHostBuilder CreateHostBuilder(string[] args) =>
     Host.CreateDefaultBuilder(args)
         .ConfigureWebHostDefaults(webBuilder =>
         {
         	 webBuilder.ConfigureServices((context, services) =>
		    {
		        // OSS 存储服务
                 services.AddSingleton(ObjectStorageContext.Instance);
             }
         }));
```

具体代码可能因为配置方式不同有细节差别

### 2 具体使用

- 在具体需要使用的`Repo`层进行依赖注入，以下为示例

```c#
using Object_Storage;
namespace YourProject.Repo
{
    public class ExampleRepo
    {
        private readonly ObjectStorageContext _OSS;
        public ExampleRepo(ObjectStorageContext oss)
        {
            _OSS = osc;
        }
    }
}
```

- 使用三个接口完成业务，请看接口文档

## Usage

### 1 Add Your Service

- Put the dll in release into a certain folder of your project.

- In Visual Studio, right-click the `dependencies` and add an quote of the dll.
- In `Program.cs`, use these codes to add a singleton instance.

```c#
 public static IHostBuilder CreateHostBuilder(string[] args) =>
     Host.CreateDefaultBuilder(args)
         .ConfigureWebHostDefaults(webBuilder =>
         {
         	 webBuilder.ConfigureServices((context, services) =>
		    {
		        // OSS Service
                 services.AddSingleton(ObjectStorageContext.Instance);
             }
         }));
```

Be aware that there may be small difference between various settings.

### 2 Use In Your  Code

- Inject the dependency in a specific `Repo Layer`, an example:

```c#
using Object_Storage;
namespace YourProject.Repo
{
    public class ExampleRepo
    {
        private readonly ObjectStorageContext _OSS;
        public ExampleRepo(ObjectStorageContext oss)
        {
            _OSS = osc;
        }
    }
}
```

- Use the three interface to complete your business. See The Interface Document

  

## Gebrauchsanweisung

### 1 Dienst hinzufügen

- Die DLL-Datei der Release-Version in ein bestimmtes Verzeichnis des Projekts verschieben.

- Im Visual Studio das Projekt auswählen, auf `Abhängigkeiten` klicken, mit der rechten Maustaste `Verweis hinzufügen` auswählen und im `Durchsuchen`-Tab die gerade eingefügte DLL-Datei auswählen.
- Fügen Sie im `Program.cs`-Datei den folgenden Code zur Registrierung hinzu.

```c#
 public static IHostBuilder CreateHostBuilder(string[] args) =>
     Host.CreateDefaultBuilder(args)
         .ConfigureWebHostDefaults(webBuilder =>
         {
         	 webBuilder.ConfigureServices((context, services) =>
		    {
		        // OSS Dienst
                 services.AddSingleton(ObjectStorageContext.Instance);
             }
         }));
```

Der spezifische Code kann aufgrund unterschiedlicher Konfigurationsmethoden Details unterscheiden.

### 2 spezifische Nutzung

- Führen Sie die Abhängigkeitsinjektion in der spezifischen `Repo`-Schicht durch, in der sie benötigt wird.

```c#
using Object_Storage;
namespace YourProject.Repo
{
    public class ExampleRepo
    {
        private readonly ObjectStorageContext _OSS;
        public ExampleRepo(ObjectStorageContext oss)
        {
            _OSS = osc;
        }
    }
}
```

- Verwenden Sie drei Schnittstellen, um die Geschäftslogik abzuschließen.



## 接口文档

#### public string store(byte[] bytes)

储存一段二进制内容（@param byte[] bytes），并返回一个唯一id码（@return string）

#### public bool delete(string fileName)

删除指定id码的文件（@param string fileName），并返回是否删除成功（@return bool）

#### public byte[] fetchFile(string fileName)

获取指定id码的文件内容（@param string fileName）， 并返回文件内容（@return byte[])

## API

#### public string store(byte[] bytes)

Store a certain binary content（@param byte[] bytes），and return an unique ID key（@return string）

#### public bool delete(string fileName)

Delete content with a certain ID key（@param string fileName），and return whether it is done successfully（@return bool）

#### public byte[] fetchFile(string fileName)

Fetch content with a certain ID key（@param string fileName）， and return the content（@return byte[])

## API-Dokumentation

#### public string store(byte[] bytes)

Speichern Sie einen Abschnitt binärer Daten (@param byte[] bytes) und geben Sie eine eindeutige ID zurück (@return string).

#### public bool delete(string fileName)

Entfernen Sie die Datei mit der angegebenen ID (@param string fileName) und geben Sie zurück, ob das Löschen erfolgreich war (@return bool).

#### public byte[] fetchFile(string fileName)

Rufen Sie den Inhalt der Datei mit der angegebenen ID ab (@param string fileName) und geben Sie den Dateinhalt zurück (@return byte[]).
