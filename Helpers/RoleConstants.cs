namespace SocietyManagementAPI.Helpers
{
    public static class RoleConstants
    {
        public const int SuperAdmin = 1;
        public const int SocAdmin = 2;
        public const int Chairman = 3;
        public const int Secretary = 4;
        public const int Treasurer = 5;
        public const int CommitteeMember = 6;
        public const int Owner = 7;
        public const int Tenant = 8;
        public const int FamilyMember = 9;
        public const int FacilityManager = 10;
        public const int Security = 11;
        public const int Staff = 12;

        // Optional: get name from ID
        public static string GetRoleName(int roleId)
        {
            return roleId switch
            {
                SuperAdmin => "SUPER_ADMIN",
                SocAdmin => "SOC_ADMIN",
                Chairman => "CHAIRMAN",
                Secretary => "SECRETARY",
                Treasurer => "TREASURER",
                CommitteeMember => "COMMITTEE_MEMBER",
                Owner => "OWNER",
                Tenant => "TENANT",
                FamilyMember => "FAMILY_MEMBER",
                FacilityManager => "FACILITY_MANAGER",
                Security => "SECURITY",
                Staff => "STAFF",
                _ => "UNKNOWN_ROLE"
            };
        }
    }
}
