﻿using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class SlashCommandHandler
{
    internal SeedHandler SeedHandler { get; set; }
    internal AboutHandler AboutHandler { get; set; }
    internal AsyncHandler AsyncHandler { get; set; }
    internal UpdateInfoHandler UpdateInfoHandler { get; set; }
    internal ScheduleAsyncsHandler ScheduleAsyncsHandler { get; set; }
    internal GenerateRaceSeedHandler GenerateRaceSeedHandler { get; set; }
    internal SetupRaceSettingsHandler SetupRaceSettingsHandler { get; set; }

    public SlashCommandHandler(SeedHandler seedHandler,
        AboutHandler aboutHandler,
        AsyncHandler asyncHandler,
        UpdateInfoHandler updateInfoHandler,
        ScheduleAsyncsHandler scheduleAsyncsHandler, 
        SetupRaceSettingsHandler setupRaceSettingsHandler,
        GenerateRaceSeedHandler generateRaceSeedHandler)
    {
        SeedHandler = seedHandler;
        AboutHandler = aboutHandler;
        AsyncHandler = asyncHandler;
        UpdateInfoHandler = updateInfoHandler;
        ScheduleAsyncsHandler = scheduleAsyncsHandler;
        GenerateRaceSeedHandler = generateRaceSeedHandler;
        SetupRaceSettingsHandler = setupRaceSettingsHandler;
    }
    
    public async Task HandleSlashCommands(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case Constants.GenerateSeed:
                await SeedHandler.HandleGenerateSeedCommand(command);
                break;
            case Constants.AboutZeffa:
                await AboutHandler.HandleAboutCommand(command);
                break;
            case Constants.GenerateAsync:
                await AsyncHandler.HandleGenerateAsyncCommand(command);
                break;
            case Constants.ShowUpdateInfo:
                await UpdateInfoHandler.HandleUpdateInfoCommand(command);
                break;
            case Constants.SetupRegularAsyncs:
                await ScheduleAsyncsHandler.HandleSetupScheduledAsyncsCommand(command);
                break;
            case Constants.RemoveAsyncConfig:
                await ScheduleAsyncsHandler.HandleRemoveConfigCommand(command);
                break;
            case Constants.SetupRaceSettings:
                await SetupRaceSettingsHandler.HandleSetupRaceSettingsCommand(command);
                break;
            case Constants.GenerateRaceSeed:
                await GenerateRaceSeedHandler.HandleGenerateRaceSeedCommand(command);
                break;
        }
    }
}