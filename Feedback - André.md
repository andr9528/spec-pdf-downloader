# Feedback

## Comments Vs. Methods

You have a number of comments, which could easily have been method names. One such is like the below.

- `Database Context setup` -> `SetupDatabaseContext(WebApplicationBuilder builder)`

Seen in `pdf-downloader.Api\Program.cs`.

## Consistency

Some places i have you use `Domain.Enums.Role.USER.ToString()` and in other places `nameof(Role.ADMIN)`.

Both archieve the same thing, with the latter giving a constant.

My Resharper is even suggesting to change your usage of the first one to how you do in the second one.

## Try / Catch

If you only re-throw an exception caught by the `catch`, then the try/catch is redundant, and just add 7 lines of code that has no effect.

Catching the exception, to e.g log it, then re throwing is fine.

Seen in `pdf-downloader.Infrastructure\Services\PdfsDownloader.cs`.

## Long Methods

Your `DoDownloads(...)` has a rather large lambda expression inside it. It actually took me a moment to notice that it was a lambda. That lambda could have been extracted to a `private async Task DoSingleDownload(PdfDownload notDownloaded)`. That would have made it quicker to spot it being a lambda.

The Try/Catches are also very close to be duplicates.
Could make a method like below to eliminate the duplication.
`private Task<bool> AttemptDownload(string url, string targetPath, StringBuilder errorBuilder, bool appendNewLine = false)`

Seen in `pdf-downloader.Infrastructure\Services\PdfsDownloader.cs`

## Immutable Id

Your enties that are tracked by Entity Framework Core - i.e `PdfDownload` and `User` - can have their `.Id` changed during the execution.

If the Id is accidentally changed after loading from the database, and then `.SaveChanges(...)` is called, then as far as i know, Entity Framework Core will thrown an exception.

## Linq

### `Where` Usage

The predicate given to a `.Where(...)` can also directly be given to a `.FirstOrDefault(...)`. It has the same outcome, but slightly cuts down on code.

Also removes some greenlines in the code, when seen with Resharper Installed :P

Seen inside `pdf-downloader.Infrastructure\Persistence\DownloadRepository.cs`.

### Count vs Any

You have check for if the count of users if 0, which is could easily be a negated call to `.Any()`.

Seen inside `pdf-downloader.Infrastructure\Persistence\UserRepository.cs`.

## Possible Improvement to Pattern Usage

### Single responsibility principle

There is some methods / classes i can point to having multiple responsibilities.

#### `DownloadAsync` Method

Your method inside `pdf-downloader.Infrastructure\Services\PdfsDownloader.cs` has the responsibility to export the downloaded file, untop of downloading it.

#### `DownloadRepository` Class

Your class also has a method to retrieving a file from the drive.

You already have a `FileService`, with a similar method, so why does the `DownloadRepository` class have it too?

Looking at usage, the `IDownloadRepository.GetFile(...)` has no usage. Likely because you created an implementation on `FileService`, and didn't remove the old one. 

#### `AppDbContext` Class

Your `DbContext` is configuring how multiple different entities are stored in the database.
It isn't really an issue with how few entities are being configured in your case. 

However be mindfull if you reach a handful or more entities, cause then i'd suggest extracting each entities configuration to a seperate file and making use of Entity Framework Cores `IEntityTypeConfiguration<TEntity>` and `modelBuilder.ApplyConfiguration(...)`.

## Praice

### Patterns

Throughout the code i have seen areas where you make use of:

- Fail Fast
    - Seen in `pdf-downloader.Infrastructure\Services\PdfsDownloader.cs`
- AAA - Arange, Act, Assert
    - Seen In `pdf-downloader.Test\AuthenticationTests.cs`
- Dependency inversion principle
    - Seen in `pdf-downloader.Infrastructure\Services\FileService.cs`

### Interesting Approches

#### Dependency Injection

Having your DI registered through an extension method on `IServiceCollection` was unexpected, but actually sorta genius.

Seen in `pdf-downloader.Infrastructure\DependencyInjection.cs`