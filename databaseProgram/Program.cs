﻿using System;
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
        static Stack navStack = new Stack();
        static List<string> commands = new List<string>();
        static int currentUser;
        static void Main(string[] args)
        {
            string[] textCommand = { "back" };
            foreach (string i in textCommand)
            {
                commands.Add(i);
            }
            connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=F:\c#\databaseProgram\databaseProgram\Database1.mdf;Integrated Security=True");
            login();
        }
        static string? takeInput()
        {
            Console.Write("> ");
            string userInput = Console.ReadLine();
            if (commands.Contains(userInput))
            {
                int i = commands.IndexOf(userInput);
                switch (i)
                {
                    case 0:
                        navStack.pop();
                        stackFunc(navStack.peek());
                        break;
                }
            }
            return userInput;
        }
        static string? takeUserPass()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }
        static int? getIndexOfArray(string[] arrayToSearch, string itemToFind)
        {
            int counter = 0;
            foreach (string i in arrayToSearch)
            {
                if (i == itemToFind)
                {
                    return counter;
                }
                counter++;
            }
            return null;
        }
        static void stackFunc(string popValue)
        {
            string[] functions = {"login", "adminPanel", "appendUser"};
            switch (getIndexOfArray(functions, popValue))
            {
                case 0:
                    login();
                    break;
                case 1:
                    adminPanel();
                    break;
                case 2:
                    appendUser();
                    break;
                default:
                    Console.WriteLine("Cannot go back.");
                    break;
            }
        }
        static void login()
        {
            bool retry = false;
            currentUser = 0;
            while (!retry)
            {
                Console.WriteLine("Enter Username");
                string usernameInput = takeUserPass().ToLower();
                if (usernameInput == "admin")
                {
                    adminPanel();
                }
                Console.WriteLine("Enter Password");
                string passwordInput = takeUserPass();
                if (verifyDetails(usernameInput, passwordInput))
                { 
                    try
                    {
                        if (cmd.Connection.State == ConnectionState.Open)
                        {
                            cmd.Connection.Close();
                        }
                        connection.Open();
                        cmd = new SqlCommand("Select USERID, Activated FROM Users Where Username = @Username", connection);
                        cmd.Parameters.AddWithValue("@Username", usernameInput);
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            Console.Clear();
                            currentUser = Convert.ToInt32(reader["USERID"]);
                            int _activated = Convert.ToInt32(reader["Activated"]);
                            if (_activated == 0)
                            {
                                Console.WriteLine("Your account hasn't been activated yet.");
                                changePassword();
                                connection.Close();
                            }
                            navStack.push("login");
                            userMenu();
                        }
                        reader.Close();
                        cmd.Dispose();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                    }
                    retry = true;
                    
                }
            }
        }
        static void userMenu()
        {
            Console.WriteLine("Menu \n1) See all flights.\n2) Account settings.");
            bool validOption = false;
            while (!validOption)
            {
                int userOption = Convert.ToInt32(takeInput());
                switch (userOption)
                {
                    case 1:
                        validOption = true;
                        seeFlights();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Re-Enter option:");
                        break;
                }
            }

        }
        static void seeFlights()
        {
            List<Flight> listOfFlights = new List<Flight>();
            Flight james = new Flight("010101", "Barbados");
            
            foreach (Flight i in listOfFlights)
            {
                Console.WriteLine("Flight number: ", i.getFlightNumer().ToString());
                Console.WriteLine("Destination: ", i.getDestination());
                Console.WriteLine("Pilot number: ", i.getPilotNumber().ToString());
            }
        }
        static void changePassword()
        {
            bool matchingPasswords = false;
            while (!matchingPasswords)
            {
                Console.WriteLine("Enter your new password");
                string newPassword = PasswordHash.Hash(takeUserPass());
                Console.WriteLine("Verify Password");
                if (PasswordHash.Verify(takeUserPass(), newPassword))
                {
                    matchingPasswords = true;
                    try
                    {
                        if (cmd.Connection.State == ConnectionState.Open)
                        {
                            cmd.Connection.Close();
                        }
                        connection.Open();
                        cmd = new SqlCommand("UPDATE Users SET Password = @newPassword, Activated = @changeActivated Where USERID = @UserID", connection);
                        cmd.Parameters.AddWithValue("@UserID", currentUser);
                        cmd.Parameters.AddWithValue("@newPassword", newPassword);
                        cmd.Parameters.AddWithValue("@changeActivated", 1);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Password updated.");
                        connection.Close();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Your passwords do not match");
                }
            }

        }
        static bool verifyDetails(string usernameEntered, string passwordEntered)
        {
            string message = ("Username or Password incorrect");
            int userID = 0;
            try
            {
                connection.Open();
                cmd = new SqlCommand("Select * from Users where Username=@Username", connection);
                cmd.Parameters.AddWithValue("@Username", usernameEntered);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    bool result = PasswordHash.Verify(passwordEntered, reader["Password"].ToString());
                    if (result == true)
                    {
                        userID = Convert.ToInt32(reader["USERID"]);
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
                currentUser = userID;
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
                string userInput = takeUserPass();

           
                foreach (string password in adminPasswords)
                {
                    if (PasswordHash.Verify(userInput, password))
                    {
                        valid = true;
                        navStack.push("adminPanel");
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
            allUsernames.Add("admin");
            bool _continue = true;
            bool _dupeUsername = true;
            string usernameToAdd = "";
            navStack.push("appendUser");
            while (_continue)
            {
                while (_dupeUsername)
                {
                    Console.WriteLine("Enter username to add");
                    usernameToAdd = takeUserPass().ToLower();
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
                string passwordToAdd = PasswordHash.Hash(takeUserPass());
                Console.WriteLine("Enter users Pilot ID");
                int userIdToAdd = Convert.ToInt32(takeUserPass());

                var sql = "INSERT INTO Users(Username, Password, PilotNo, Activated) VALUES(@usernameToAdd, @passwordToAdd, @userIdToAdd, @Activated)";
                try
                {

                    using (connection)
                    {
                        if (cmd.Connection.State == ConnectionState.Open)
                        {
                            cmd.Connection.Close();
                        }
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
                string userInput = takeInput().ToLower();
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
                if (cmd.Connection.State == ConnectionState.Open)
                {
                    cmd.Connection.Close();
                }
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
        private string flightNumber;
        private int pilotNumber;
        private string destination;
        public Flight(string _flightNumber, string _destination)
        {
            flightNumber = _flightNumber;
            destination = _destination;
        }
        public string getFlightNumer()
        {
            return flightNumber;
        }
        public int getPilotNumber()
        {
            return pilotNumber;
        }
        public string getDestination()
        {
            return destination;
        }
        public void assignPilot()
        {
            
        }
        public void getWeatherInfo()
        {

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
    class Stack
    {
        private int StackPointer = -1;
        private List<string> _stack = new List<string>();
        private bool stackEmpty()
        {
            if (StackPointer == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void push(string value)
        {
            try
            {
                _stack.Add(value);
                StackPointer++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public string? pop()
        {
            bool _stackEmpty = stackEmpty();
            if (_stackEmpty == false)
            {
                var popValue = _stack[StackPointer];
                StackPointer--;
                return popValue;
            }
            else
            {
                Console.WriteLine("You cannot go back any further.");
                return null;
            }
        }
        public string? peek()
        {
            bool _stackEmpty = stackEmpty();
            if (!_stackEmpty)
            {
                return _stack[StackPointer];
            }
            else
            {
                return null;
            }
        }

    }
}
