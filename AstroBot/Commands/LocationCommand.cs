using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// Command that can get the lat / lng coordiantes for any place on earth
    /// </summary>
    public class LocationCommand : AwesomeChatBot.Commands.Command, IRegexCommand
    {
        /// <summary>
        /// The name of the command
        /// </summary>
        public override string Name => "GeoLocation";

        /// <summary>
        /// The regex, when matched executes the command
        /// </summary>
        /// <returns></returns>
        public List<string> Regex => new List<string>(){ @"Where is (?'SearchLocation'.*\w)(\?)?" };

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="recievedMessage"></param>
        /// <param name="regexMatch"></param>
        /// <returns></returns>
        public Task<bool> ExecuteRegexCommand(RecievedMessage recievedMessage, Match regexMatch)
        {
            return Task<bool>.Factory.StartNew(() => {
                var location    = regexMatch.Groups["SearchLocation"].Value;
                var geoLocation = GeoLocation.GeoLocation.FindLocation(location);

                if (geoLocation == null) {
                    recievedMessage.Channel.SendMessageAsync(new SendMessage($"I don't know any place on earth with the name {location}")).Wait();
                    return true;
                }

                recievedMessage.Channel.SendMessageAsync(new SendMessage($"I found the following location for \"{location}\":\r\n```\r\nName:\t{geoLocation.Name}\r\nLat:\t{geoLocation.Lat}\r\nLng:\t{geoLocation.Lng}```"));

                return true;
            });
        }
    }
}