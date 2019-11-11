using System;
using System.Globalization;
using System.Linq;

namespace AstroBot.CronTasks
{
    public static class IntermediateRocketLaunchNotify
    {
        public static void Execute()
        {
            var intermediateLaunches = LaunchLibrary.LaunchLibraryClient.GetUpcomingLaunches(days: 1)
                .Where(launch => DateTime.ParseExact(
                    launch.Isostart,
                    "yyyyMMddTHHmmssZ",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal) > DateTime.UtcNow
                    && DateTime.ParseExact(
                        launch.Isostart,
                        "yyyyMMddTHHmmssZ",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal) < DateTime.UtcNow.AddHours(1));

            if (intermediateLaunches.Any())
            {
                foreach (var wrapper in Globals.BotFramework.ApiWrappers)
                {
                    foreach (var server in wrapper.GetAvailableServers())
                    {
                        if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("RocketLaunchesNewsEnabled", server))
                        {
                            var role = Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesIntermediateTagRole", server);
                            if (!string.IsNullOrEmpty(role))
                            {
                                var channel = server
                                    .ResolveChannelAsync(Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesNewsChannel", server))
                                    .GetAwaiter()
                                    .GetResult();

                                foreach (var launch in intermediateLaunches)
                                {
                                    var roles = server.GetAvailableUserRolesAsync().GetAwaiter().GetResult();
                                    var roleObj = roles.FirstOrDefault(serverRole => serverRole.Name == role);
                                    if (roleObj != null)
                                    {
                                        channel.SendMessageAsync($"{roleObj.GetMention()} Upcoming launch within the next hour!\r\n{wrapper.MessageFormatter.Bold(launch.Name)}\r\n{launch.VidUrLs.FirstOrDefault()}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Register()
        {
            Globals.CronDaemon.Add("*/5 * * * *", Execute);
        }
    }
}