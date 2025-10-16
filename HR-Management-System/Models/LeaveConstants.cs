namespace HR_Management_System.Models
{
    public static class LeaveConstants
    {
        public static class Types
        {
            public const string Annual = "Annual";
            public const string Sick = "Sick";
            public const string Unpaid = "Unpaid";
            public const string Emergency = "Emergency";
            public const string Maternity = "Maternity";
            public const string Paternity = "Paternity";
        }

        public static class Statuses
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
        }
    }
}
