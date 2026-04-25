namespace CrmWorkTrack.WebApi.Auth.Authorization.Permissions;

public static class Permissions
{
    public static class Jobs
    {
        public const string Read = "perm:jobs.read";
        public const string Create = "perm:jobs.create";
        public const string Update = "perm:jobs.update";
        public const string Delete = "perm:jobs.delete";
        public const string Assign = "perm:jobs.assign";
        public const string UpdateStatus = "perm:jobs.updateStatus";
    }

    public static class Customers
    {
        public const string Read = "perm:customers.read";
        public const string Create = "perm:customers.create";
        public const string Update = "perm:customers.update";
        public const string Delete = "perm:customers.delete";
    }

    public static class CustomerContacts
    {
        public const string Read = "perm:customers.contacts.read";
        public const string Create = "perm:customers.contacts.create";
        public const string Update = "perm:customers.contacts.update";
        public const string Delete = "perm:customers.contacts.delete";
    }
}