using Deathmatch.API.Matches.Registrations;

namespace Deathmatch.Core.Matches.Registrations
{
    public class MatchRegistrationAccessor : IMatchRegistrationAccessor
    {
        public MatchRegistrationAccessor(IMatchRegistration registration)
        {
            Registration = registration;
        }

        public IMatchRegistration Registration { get; }
    }
}
