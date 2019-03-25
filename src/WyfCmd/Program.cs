// <copyright file="Program.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PowerArgs;
    using WhatsYourFace.Models;
    using WhatsYourFace.Utilities;
    using WhatsYourFace.Wyfcmd.Commands;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable; cannot because of ILogger<Program>
    public class Program
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        private static readonly string[] Environments = { "development", "production", "test1" };

        private static ILogger<Program> logger;
        private static IServiceProvider serviceProvider;
        private static IConfigurationRoot config;

        public static async Task Main(string[] args)
        {
            string env = null;
            if (FirstArgumentIsEnvironment(args))
            {
                env = args[0];
                args = args.Skip(1).ToArray();
            }

            config = ConfigurationUtilities.LoadAppSettingsJsonConfiguration(env);
            ConfigureDependencyInjection();
            logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            using (logger.BeginScope(
                "Executing command {command} in {environment} environment",
                args.FirstOrDefault() ?? "help",
                env ?? "N/A"))
            {
                Stopwatch timer = Stopwatch.StartNew();

                try
                {
                    // TODO (vladcananau): watch this for attribute inheritance support
                    // https://github.com/adamabdelhamed/PowerArgs/issues/142
                    // await RunWithPowerArgs(args);
                    RunWithCommandLineParser(args);
                    logger.LogInformation("Command successful after: {duration}ms", timer.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Command failed after: {duration}ms", timer.ElapsedMilliseconds);
                }
                finally
                {
                    Console.ReadLine();
                }
            }
        }

        private static bool FirstArgumentIsEnvironment(string[] args)
        {
            return args.Length > 0 && Environments.Contains(args[0], StringComparer.OrdinalIgnoreCase);
        }

        private static async Task RunWithPowerArgs(string[] args)
        {
            Args.RegisterFactory(typeof(CmdDispatcher), () => new CmdDispatcher(serviceProvider));
            await Args.InvokeActionAsync<CmdDispatcher>(args);
        }

        private static void RunWithCommandLineParser(IEnumerable<string> args)
        {
            var parser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = Console.Out;
            });

            CmdDispatcher dispatcher = new CmdDispatcher(serviceProvider);

            // TODO (vladcananau): watch this for async support:
            // https://github.com/commandlineparser/commandline/pull/390
            parser
                .ParseArguments<
                    MatchFaceToNamesCmd,
                    CreateFaceListCmd,
                    UploadImagesToFaceListCmd,
                    DownloadImagesCmd,
                    RemoveGenderMismatchesCmd>(args)
                .WithParsed<MatchFaceToNamesCmd>(commandArgs =>
                {
                    dispatcher.WhatsYourFace(commandArgs).GetAwaiter().GetResult();
                })
                .WithParsed<CreateFaceListCmd>(commandArgs =>
                {
                    dispatcher.CreateFaceList(commandArgs).GetAwaiter().GetResult();
                })
                .WithParsed<UploadImagesToFaceListCmd>(commandArgs =>
                {
                    dispatcher.UploadImages(commandArgs).GetAwaiter().GetResult();
                })
                .WithParsed<DownloadImagesCmd>(commandArgs =>
                {
                    dispatcher.DownloadImages(commandArgs).GetAwaiter().GetResult();
                })
                .WithParsed<RemoveGenderMismatchesCmd>(commandArgs =>
                {
                    dispatcher.RemoveMismatches(commandArgs).GetAwaiter().GetResult();
                });

            return;
        }

        private static void ConfigureDependencyInjection()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IConsole, CommandPrompt>();
            services.AddTransient<IFileSystem, FileSystem>();

            services.AddFaceIdLookup(config);
            services.AddFaceClient(config);
            services.AddFaceMatcher(config);
            services.AddFaceUploader();
            services.AddImageSearchClient(config);
            services.AddImageDownloader();
            services.AddImageSanitizer();

            services.AddTransient<MatchFaceToNamesCmd>();
            services.AddTransient<CreateFaceListCmd>();
            services.AddTransient<UploadImagesToFaceListCmd>();
            services.AddTransient<DownloadImagesCmd>();
            services.AddTransient<RemoveGenderMismatchesCmd>();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights();
                loggingBuilder.AddConsole(options => { options.IncludeScopes = true; });
            });

            serviceProvider = services.BuildServiceProvider();
        }
    }
}
