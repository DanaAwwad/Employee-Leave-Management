using Employee___Leave_Management.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
namespace Employee___Leave_Management.Services
{
    public class EmployeeService
    {
        private List<Employees> Employees;

        private readonly string connectionString =
       "Server=localhost;Database=EmployeeDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task AddEmployee(String name, int remainingLeaveDays)
        {
            int rowsAffected;

            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "INSERT INTO Employees(Name,RemainingLeaveDays,IsDeleted) " +
                    "VALUES(@Name,@RemainingLeaveDays,0)";
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@RemainingLeaveDays", remainingLeaveDays);

            rowsAffected = await command.ExecuteNonQueryAsync();

            Console.WriteLine($"Rows affected: {rowsAffected}");

        }
        public async Task UpdateEmployee(int id, string name, int remainingLeaveDays)
        {
            int rowsAffected;

            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "UPDATE Employees SET Name=@Name, RemainingLeaveDays=@RemainingLeaveDays WHERE Id=@Id AND IsDeleted=0";
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@RemainingLeaveDays", remainingLeaveDays);

            rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                Console.WriteLine($"No employee found with Id: {id}");
            }
            else
            {
                Console.WriteLine($"Employee with Id: {id} updated successfully.");
            }
        }
        public async Task DeleteEmployee(int id)
        {
            int rowsAffected;

            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "UPDATE Employees SET IsDeleted=1 WHERE Id=@Id";
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);
            rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                Console.WriteLine($"No employee found with Id: {id}");
            }
            else
            {
                Console.WriteLine("Employee deleted successfully.");
            }

        }
        public async Task<List<Employees>> GetAllEmployees()
        {
            List<Employees> Employees = new List<Employees>();
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT Id, Name, RemainingLeaveDays FROM Employees WHERE IsDeleted=0";
            using SqlCommand command = new SqlCommand(sql, connection);

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            {
                while (await reader.ReadAsync())
                {
                    Employees student = new Employees
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        RemainingLeaveDays = (int)reader["RemainingLeaveDays"],
                        IsDeleted = false
                    };
                    Employees.Add(student);
                }

                return Employees;
            }
        }
        public async Task<List<Employees>> SearchEmployeeByName(string name)
        {
            List<Employees> Employees = new List<Employees>();

            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT Id, Name, RemainingLeaveDays FROM Employees WHERE IsDeleted=0 AND Name=@Name";
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", name);

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Employees employee = new Employees
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    RemainingLeaveDays = (int)reader["RemainingLeaveDays"],
                    IsDeleted = false
                };
                Employees.Add(employee);
            }
            return Employees;
        }
    }
}
