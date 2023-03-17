using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.Data;

namespace databaseProgram
{
    class Program
    {
        static SqlConnection connection;
        static SqlCommand cmd;
        static SqlDataReader reader;
        static SqlDataAdapter adapter = new SqlDataAdapter();
        int currentUser;
        static void Main(string[] args)
        {
            connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=F:\c#\databaseProgram\databaseProgram\Database1.mdf;Integrated Security=True");
            List<Pilot> listOfPilots = new List<Pilot>();
            List<Flight> listOfFlights = new List<Flight>();
            adminPanel();
            
        }
        static void login()
        {
            Console.WriteLine("Enter Username");
            string usernameInput = Console.ReadLine().ToLower();
            Console.WriteLine("Enter Password");
            string passwordInput = Console.ReadLine();
            if (verifyDetails(usernameInput, passwordInput))
            {

            }
        }
        static bool verifyDetails(string usernameEntered, string passwordEntered)
        {
            string message = ("Username or Password incorrect");
            try
            {
                connection.Open();
                cmd = new SqlCommand("Select * from Users where Username=@Username", connection);
                cmd.Parameters.AddWithValue("@Username", usernameEntered);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var result = PasswordHash.Verify(passwordEntered, reader["Password"].ToString());
                    if (result == true)
                    {
                        message = "1";
                    }
                }
                reader.Close();
                cmd.Dispose();
                connection.Close();
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }
            if (message == "1")
            {
                return true;
            }
            else
            {
                Console.WriteLine(message, "Info");
                return false;
            }
        }
        static void adminPanel()
        {
            List<string> adminPasswords = new List<string>();
            foreach (string line in System.IO.File.ReadLines(@"F:\c#\databaseProgram\databaseProgram\adminPassword.txt"))
            {
                adminPasswords.Add(line);
            }

            int tries = 0;
            bool valid = false;
            while (!valid)
            {
                Console.WriteLine("Enter password");
                string userInput = Console.ReadLine();

           
                foreach (string password in adminPasswords)
                {
                    if (PasswordHash.Verify(userInput, password))
                    {
                        valid = true;
                        appendUser();
                        break;
                    }
                }
                if (!valid)
                {
                    tries++;
                    if (tries == 3)
                    {
                        Environment.Exit(0);
                    }
                    else if (tries < 3)
                    {
                        Console.Clear();
                        Console.WriteLine("Incorrect password");
                    }   
                }
            }
        }
        static void appendUser()
        {
            List<string> allUsernames = fieldToList("Username", "Users");
            bool _continue = true;
            bool _dupeUsername = true;
            string usernameToAdd = "";
            while (_continue)
            {
                while (_dupeUsername)
                {
                    Console.WriteLine("Enter username to add");
                    usernameToAdd = Console.ReadLine().ToLower();
                    if (allUsernames.Contains(usernameToAdd))
                    {
                        Console.WriteLine("Username is taken, try another.");
                    }
                    else if (!allUsernames.Contains(usernameToAdd))
                    {
                        _dupeUsername = false;
                    }
                    else
                    {
                        Console.WriteLine("Error.");
                    }
                }

                
                Console.WriteLine("Enter password for user");
                string passwordToAdd = PasswordHash.Hash(Console.ReadLine());
                Console.WriteLine("Enter users Pilot ID");
                int userIdToAdd = Convert.ToInt32(Console.ReadLine());

                var sql = "INSERT INTO Users(Username, Password, PilotNo, Activated) VALUES(@usernameToAdd, @passwordToAdd, @userIdToAdd, @Activated)";
                try
                {

                    using (connection)
                    {
                        connection.Open();
                        SqlCommand insCmd = new SqlCommand(sql, connection);
                        insCmd.Parameters.AddWithValue("@usernameToAdd", usernameToAdd);
                        insCmd.Parameters.AddWithValue("@passwordToAdd", passwordToAdd);
                        insCmd.Parameters.AddWithValue("@userIdToAdd", userIdToAdd);
                        insCmd.Parameters.AddWithValue("@Activated", 0);
                        insCmd.ExecuteNonQuery();
                        Console.WriteLine("Added user " + userIdToAdd + " with Pilot ID " + userIdToAdd);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Console.WriteLine("Add another? Y/N");
                string userInput = Console.ReadLine().ToLower();
                if (userInput == "n")
                {
                    _continue = false;
                }
            }

        }
        static List<string> fieldToList(string field, string table)
        {

            List<string> usersList = new List<string>();
            using (cmd = connection.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = @"SELECT " + field + " FROM " + table;
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        usersList.Add(reader[field].ToString());
                    }
                }
            }
            return usersList;
        }
    }

    class Pilot
    {
        private int pilotNumber;
        private string name;

        public int get_pilotNumber()
        {
            return pilotNumber;
        }
        public string get_pilotName()
        {
            return name;
        }
    }
    class Flight
    {
        Pilot _pilot = new Pilot();
        private string flightNumber;
        private int pilotNumber;
        private string destination;
        public void assignPilot()
        {
            pilotNumber = _pilot.get_pilotNumber();
        }

    }
    class PasswordHash
    {
        private const int saltSize = 16;
        private const int hashSize = 20;
        public static string Hash(string password, int iterations)
        {
            //create salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[saltSize]);

            //create hash
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            var hash = pbkdf2.GetBytes(hashSize);

            // Combine salt and hash
            var hashBytes = new byte[saltSize + hashSize];
            Array.Copy(salt, 0, hashBytes, 0, saltSize);
            Array.Copy(hash, 0, hashBytes, saltSize, hashSize);

            // Convert to base64
            var base64Hash = Convert.ToBase64String(hashBytes);

            // Format hash with extra information
            return string.Format("$THOMASL0RD${0}${1}", iterations, base64Hash);
        }
        /// Creates a hash from a password with 10000 iterations
        public static string Hash(string password)
        {
            return Hash(password, 10000);
        }

        /// Checks if hash is supported.
        public static bool IsHashSupported(string hashString)
        {
            return hashString.Contains("$THOMASL0RD$");
        }


        /// Verifies a password
        public static bool Verify(string password, string hashedPassword)
        {
            // Check hash
            if (!IsHashSupported(hashedPassword))
            {
                throw new NotSupportedException("The hashtype is not supported");
            }

            // Extract iteration and Base64 string
            var splittedHashString = hashedPassword.Replace("$THOMASL0RD$", "").Split('$');
            var iterations = int.Parse(splittedHashString[0]);
            var base64Hash = splittedHashString[1];

            // Get hash bytes
            var hashBytes = Convert.FromBase64String(base64Hash);

            // Get salt
            var salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            // Create hash with given salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(hashSize);

            // Get result
            for (var i = 0; i < hashSize; i++)
            {
                if (hashBytes[i + saltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
