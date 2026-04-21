
namespace Common.UserActions
{
    public class ContactRequestParameters : ActionParametersBase
    {
        public ContactType ContactType;
        public string? EstateId;
        public EstateSearchParameters? searchParameters;
    }
}
