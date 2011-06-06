using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FarleyFile.Views
{
    [Serializable]
    public sealed class UserDashboardView
    {
        public IList<Contact> Contacts = new List<Contact>();
    }
    
    public sealed class Contact
    {
        public string DisplayName;
        public string Identifier;
    }
}