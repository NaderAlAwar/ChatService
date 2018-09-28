namespace ChatService.DataContracts
{
    public class UserInfoDto
    {
        public UserInfoDto(string username, string firstName, string lastName)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
        }

        public string Username { get; }
        public string FirstName { get; }
        public string LastName { get; }
    }
}
