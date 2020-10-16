using System.Collections.Generic;
using System.Linq;

namespace FinanceBot
{
    /// <summary>
    /// Class to store conversation data. We need a dictionary structure to pass the conversation state to dialogs.
    /// </summary>
    public class UserData : Dictionary<string, object>
    {
        //private const string AmountPeopleKey = "AmountPeople";
        //private const string FullNameKey = "FullName";
        //private const string TimeKey = "Time";
        //private const string ConfirmedKey = "Confirmed";
        //private const string ConversationLanguageKey = "ConversationLanguage";
        private const string UserIdKey = "UserID";
        private const string UserNameKey = "UserName";
        private const string UserTypeKey = "UserType";
        //private const string ClaimNumberKey = "ClaimNumber";
        //private const string SimDataKey = "SimData";
        //private const string ReimbursementDataKey = "ReimbursementData";

        public UserData()
        {
            //this[AmountPeopleKey] = null;
            //this[FullNameKey] = null;
            //this[TimeKey] = null;
            //this[ConfirmedKey] = null;
            //this[ConversationLanguageKey] = null;
            this[UserIdKey] = null;
            this[UserNameKey] = null;
            this[UserTypeKey] = null;
            //this[ClaimNumberKey] = null;
            //this[SimDataKey] = null;
            //this[ReimbursementDataKey] = null;
        }

        public UserData(IDictionary<string, object> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x => this.Add(x.Key, x.Value));
            }
        }

        //public SIMS SimData
        //{
        //    get { return (SIMS)this[SimDataKey]; }
        //    set { this[SimDataKey] = value; }
        //}

        //public Reimbursement ReimbursementData
        //{
        //    get { return (Reimbursement)this[ReimbursementDataKey]; }
        //    set { this[ReimbursementDataKey] = value; }
        //}
        public string UserID
        {
            get { return (string)this[UserIdKey]; }
            set { this[UserIdKey] = value; }
        }

        public string UserName
        {
            get { return (string)this[UserNameKey]; }
            set { this[UserNameKey] = value; }
        }

        public string UserType
        {
            get { return (string)this[UserTypeKey]; }
            set { this[UserTypeKey] = value; }
        }

        //public string ClaimNumber
        //{
        //    get { return (string)this[ClaimNumberKey]; }
        //    set { this[ClaimNumberKey] = value; }
        //}

        //public string AmountPeople
        //{
        //    get { return (string)this[AmountPeopleKey]; }
        //    set { this[AmountPeopleKey] = value; }
        //}

        //public string Time
        //{
        //    get { return (string)this[TimeKey]; }
        //    set { this[TimeKey] = value; }
        //}

        //public string FullName
        //{
        //    get { return (string)this[FullNameKey]; }
        //    set { this[FullNameKey] = value; }
        //}

        //public string FirstName
        //{
        //    get { return ((string)this[FullNameKey])?.Split(" ")[0]; }
        //}

        //public string Confirmed
        //{
        //    get { return (string)this[ConfirmedKey]; }
        //    set { this[ConfirmedKey] = value; }
        //}

        //public string ConversationLanguage
        //{
        //    get { return (string)this[ConversationLanguageKey]; }
        //    set { this[ConversationLanguageKey] = value; }
        //}
    }

}
