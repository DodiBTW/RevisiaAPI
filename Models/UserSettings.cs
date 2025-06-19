namespace RevisiaAPI.Models
{
    public class UserSettings
    {
        public int id = 0;
        public int userId;
        public double rememberMultiplier = 1.8;
        public double forgotMultiplier = 0.5;
        public int maxInterval = 365;
        public int dailyGoal = 10;

        public UserSettings() { }
    }
}
