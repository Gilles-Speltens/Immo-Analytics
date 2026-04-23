
namespace Common.UserActions
{
    public class ContactRequestParameters : UserActionsBase
    {
        public ContactType ConType { get; set; }
        public string? EstateId { get; set; }
        public EstateSearchs? SearchParameters { get; set; }
    }
}
