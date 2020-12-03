using Deathmatch.API.Matches;
using OpenMod.API.Prioritization;
using System;
using System.Collections.Generic;

namespace Deathmatch.Core.Matches
{
    public class RegisteredMatchInfo
    {
        public string Id { get; set; }

        public bool Enabled { get; set; }

        public Priority Priority { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Aliases { get; set; }

        public RegisteredMatchInfo()
        {
            Id = "";
            Enabled = true;
            Priority = Priority.Normal;
            Title = "";
            Description = "";
            Aliases = new List<string>();
        }

        public RegisteredMatchInfo(IMatchRegistration registration)
        {
            Id = registration.Id;
            Enabled = registration.Enabled;
            Priority = registration.Priority;
            Title = registration.Title;
            Description = registration.Description;
            Aliases = new List<string>(registration.Aliases);
        }

        public void ApplyTo(IMatchRegistration registration)
        {
            if (registration.Id != Id)
                throw new Exception("Cannot apply info to non-matching registration");

            registration.Enabled = Enabled;
            registration.Priority = Priority;
            registration.Title = Title;
            registration.Description = Description;
            registration.Aliases = new List<string>(Aliases);
        }
    }
}
