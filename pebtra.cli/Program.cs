using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using pebtra.DAL;
using Pebtra.Core;

namespace Pebtra.Util;

class Program
{
    static int Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)            
            .Build();

        var services = new ServiceCollection();
        
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConfiguration(configuration.GetSection("Logging"));

            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;             
                options.TimestampFormat = "HH:mm:ss "; 
                options.IncludeScopes = true;         
            });

            logging.AddConsole(options =>
            {
                options.FormatterName = ConsoleFormatterNames.Simple;
            });
        });        

        services.AddDataAccess(configuration);
        
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<HttpClient>();
        
        services.AddScoped<StatementImportService>();
        services.AddScoped<CurrencyRateFetchService>();

        // Make the non-generic logger available (injected as ILogger logger) to avoid printing class names to the console.
        services.AddSingleton<ILogger>(sp =>
        {
            var factory = sp.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger(String.Empty);
        });        
        
        var serviceProvider = services.BuildServiceProvider();
    
        var importCommand = new Command("import", "Import data from file");
        var fileOption = new Option<string>("--file", "File to import") { IsRequired = true };
        var skipDuplicatesOption = new Option<bool>("--skip-duplicates", "Import statements containing already existing transactions, skip those transactions");
        importCommand.AddOption(fileOption);
        importCommand.AddOption(skipDuplicatesOption);
        importCommand.SetHandler((InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForOption(fileOption)!;
            var forceDuplicates = context.ParseResult.GetValueForOption(skipDuplicatesOption);
            using var scope = serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<StatementImportService>();
            ImportFile(importService, file, forceDuplicates);
        });

        var extractCommand = new Command("extract", "Extract and print plain text from a statement file");
        var fileOption2 = new Option<string>("--file", "Statement file") { IsRequired = true };
        extractCommand.AddOption(fileOption2);
        extractCommand.SetHandler((InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForOption(fileOption2)!;
            using var scope = serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<StatementImportService>();
            Extract(importService, file);
        });

        var fetchCommand = new Command("fetch", "Fetch data from source");
        fetchCommand.SetHandler((InvocationContext context) =>
        {
            using var scope = serviceProvider.CreateScope();
            var fetchService = scope.ServiceProvider.GetRequiredService<CurrencyRateFetchService>();
            FetchCurrencyRates(fetchService);
        });

        // var categorizeCommand = new Command("categorize", "Categorize existing data");
        // categorizeCommand.SetHandler(() => CategorizeData());

        var root = new RootCommand("Pebtra Utility Tool")
        {
            importCommand,
            extractCommand,
            fetchCommand,
            // categorizeCommand
        };

        var result = root.Invoke(args);
        
        // Dispose service provider
        serviceProvider.Dispose();
        
        return result;
    }

    static void ImportFile(StatementImportService importService, string filename, bool forceDuplicates = false)
    {
        importService.ImportAsync(filename, forceDuplicates).GetAwaiter().GetResult();
    }

    static void Extract(StatementImportService importService, string filename)
    {
        importService.ExtractText(filename);
    }

    static void FetchCurrencyRates(CurrencyRateFetchService fetchService)
    {
        fetchService.Fetch().GetAwaiter().GetResult();
    }

    static void CategorizeData()
    {
        // Implementation will be added later
    }
}

