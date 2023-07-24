using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;";

using (SqlConnection sqlConnection = new SqlConnection(connectionString))
{
    sqlConnection.Open();
    SqlCommand command = new SqlCommand();
    command.CommandText = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HabitTracker') " +
        "CREATE DATABASE HabitTracker";
    command.Connection = sqlConnection;
    command.ExecuteNonQuery();
    Console.WriteLine("DB created or existed");
    command.CommandText = "USE HabitTracker IF NOT EXISTS (SELECT * FROM sys.tables  WHERE name = 'Habit') " +
        "CREATE TABLE Habit(Id INTEGER PRIMARY KEY IDENTITY(1,1) NOT NULL, Date TEXT NOT NULL, Hours INTEGER NOT NULL)";
    command.ExecuteNonQuery();
    Console.WriteLine("table created or existed");
}

GetUserInput();

void GetUserInput()
{
    bool closeApp = false;
    while (!closeApp)
    {
        Console.WriteLine("\n\nMAIN MENU");
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("\nType 0 to Close Application.");
        Console.WriteLine("Type 1 to View All Records.");
        Console.WriteLine("Type 2 to Insert Record.");
        Console.WriteLine("Type 3 to Delete Record.");
        Console.WriteLine("Type 4 to Update Record.");
        Console.WriteLine("------------------------------------------\n");
        string command = Console.ReadLine();

        switch (command)
        {
            case "0":
                Console.WriteLine("\nGoodbye!\n");
                closeApp = true;
                //Environment.Exit(0);
                break;
            case "1":
                GetAllRecords();
                break;
            case "2":
                Insert();
                break;
            case "3":
                Delete();
                break;
            case "4":
                Update();
                break;
            default:
                Console.WriteLine("\nInvalid Command. Please type a number from 0 to 4.\n");
                break;
        }
    }
}

void GetAllRecords()
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        var sqlCommand = new SqlCommand();
        sqlCommand.CommandText = "USE HabitTracker SELECT * FROM Habit";
        sqlCommand.Connection = connection;
        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
        List<CodingHabit> tableData = new();

        if (sqlDataReader.HasRows)
        {
            while (sqlDataReader.Read())
            {
                tableData.Add(new CodingHabit
                {
                    Id = sqlDataReader.GetInt32(0),
                    Date = DateTime.ParseExact(sqlDataReader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                    Hours = sqlDataReader.GetInt32(2),
                });
            }
        }
        else
        {
            Console.WriteLine("\nNo data found");
        }

        Console.WriteLine("_________________________________\n");
        foreach(var codingHabit in tableData)
        {
            Console.WriteLine($"{codingHabit.Id} - {codingHabit.Date.ToString("dd-MM-yyyy")} - Hours: {codingHabit.Hours}");
        }
    }
}

void Insert()
{
    string dateInput = GetDateInput();
    int hours = GetNumberInput("\nPlease insert integer value");

    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        var sqlCommand = new SqlCommand();
        sqlCommand.CommandText = $"USE HabitTracker INSERT INTO Habit VALUES ('{dateInput}', {hours})";
        sqlCommand.Connection = connection;
        sqlCommand.ExecuteNonQueryAsync();
        Console.WriteLine("\nValues were successfully inserted into Habit table");
    }
}

string GetDateInput()
{
    Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main manu.\n\n");
    string dateInput = Console.ReadLine();
    if (dateInput == "0") GetUserInput();
    while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
    {
        Console.WriteLine("\nInvalid date. (Format: dd-mm-yy). try again or type 0 to return to main manu");
        dateInput = Console.ReadLine();
        if (dateInput == "0") GetUserInput();
    }
    return dateInput;
}


int GetNumberInput(string message)
{
    Console.WriteLine(message);
    int num;
    string numInput = Console.ReadLine();
    if (numInput == "0") GetUserInput();
    while (!int.TryParse(numInput, out num) || num < 1)
    {
        Console.WriteLine("\nInput is not valid. Please enter positive integer.To exit to main menu enter 0");
        numInput = Console.ReadLine();
        if (numInput == "0") GetUserInput();
    }
    return num;
}

void Delete()
{
    GetAllRecords();
    int id = GetNumberInput("\nPlease specify id of entry you wish to delete. Enter 0 to exit to main menu");
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        var sqlCommand = new SqlCommand();
        sqlCommand.CommandText = $"USE HabitTracker DELETE FROM Habit WHERE ID = ({id})";
        sqlCommand.Connection = connection;
        int deletedEntries = sqlCommand.ExecuteNonQuery();
        if (deletedEntries == 0)
        {
            Console.WriteLine($"\nCouldn't find entry with id {id}");
            Delete();
        }
        else
        {
            Console.WriteLine("\nEntry was successfully deleted from Habit table");
        }
    }
}

void Update()
{
    GetAllRecords();
    int id = GetNumberInput("\nPlease specify id of entry you wish to update. Enter 0 to exit to main menu");
    
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        var sqlCommand = new SqlCommand();
        sqlCommand.CommandText = $"USE HabitTracker SELECT * FROM Habit WHERE ID = {id}";
        sqlCommand.Connection = connection;
        var checkEntry = sqlCommand.ExecuteScalar();
        if (checkEntry is null)
        {
            Console.WriteLine($"\nCouldn't find entry with id {id}");
            Update();
        }
        else
        {
            string dateInput = GetDateInput();
            int hours = GetNumberInput("\nPlease insert integer value");

            sqlCommand.CommandText = $"USE HabitTracker UPDATE Habit SET Date = '{dateInput}', Hours = {hours} " +
                $"WHERE ID = {id}";
            sqlCommand.ExecuteNonQuery();
            Console.WriteLine($"\nEntry with id {id} was successfully updated");
        }
    }
}