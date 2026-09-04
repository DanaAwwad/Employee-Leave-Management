using Employee___Leave_Management.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Employee___Leave_Management.Services
{
    public class LeaveService
    {
        private readonly string connectionString =
         "Server=localhost;Database=EmployeeDB;Trusted_Connection=True;TrustServerCertificate=True;";
        public async Task CreateLeaveRequest(int employeeId, int requestedDays)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();


            string sql = "SELECT COUNT(*) FROM Employees WHERE Id=@Id AND IsDeleted=0";
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", employeeId);

            int employeeCount = Convert.ToInt32(await command.ExecuteScalarAsync()); // ExecuteScalarAsync returns object

            if (employeeCount == 0)
            {
                Console.WriteLine("Employee not found");
                return;
            }

            string sql2 = "SELECT RemainingLeaveDays FROM Employees WHERE Id=@Id";
            using SqlCommand command2 = new SqlCommand(sql2, connection);

            command2.Parameters.AddWithValue("@Id", employeeId);

            object result = await command2.ExecuteScalarAsync();
            int remainingLeaveDays = Convert.ToInt32(result);

            if (remainingLeaveDays < requestedDays)
            {
                Console.WriteLine("Not enough leave days");
                return;
            }

            string sql3 = "INSERT INTO LeaveRequests (EmployeeId, RequestedDays, Status) VALUES (@EmployeeId, @RequestedDays, 0)";

            using SqlCommand command3 = new SqlCommand(sql3, connection);
            command3.Parameters.AddWithValue("@EmployeeId", employeeId);
            command3.Parameters.AddWithValue("@RequestedDays", requestedDays);
            await command3.ExecuteNonQueryAsync();

            Console.WriteLine("Leave request created successfully");
            return;
        }


        public async Task GetAllLeaveRequests()
        {
            var leaveRequests = new List<Leave_Requests>();

            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT LR.Id, E.Name, LR.RequestedDays, LR.Status FROM LeaveRequests LR JOIN Employees E ON LR.EmployeeId = E.Id";
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int requestId = (int)reader[0];
                string employeeName = (string)reader[1];
                int requestedDays = (int)reader[2];
                int status = (int)reader[3];

                Console.WriteLine("Request ID: {requestId}, Employee Name: {employeeName}, Requested Days: {requestedDays}, Status: {status}");
            }

        }


        public async Task AcceptLeave(int requestId)
        {
            int requestedDays;
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                string sql = "SELECT Status,RequestedDays FROM LeaveRequests WHERE Id=@RequestId";
                using SqlCommand command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@RequestId", requestId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Leave request not found");
                        return;
                    }
                    int status = (int)reader[0];
                    requestedDays = (int)reader[1];

                    if (status != 0)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Leave request is not pending");
                        return;
                    }
                }

                string sql2 = "UPDATE LeaaveRequests SET Status=1 WHERE Id=@RequestId";
                using SqlCommand command2 = new SqlCommand(sql2, connection, transaction);
                command2.Parameters.AddWithValue("@RequestId", requestId);
                await command2.ExecuteNonQueryAsync();

                string sql3 = "UPDATE Employees SET RemainingLeaveDays=RemainingLeaveDays-@RequestedDays WHERE Id=(SELECT EmployeeId FROM LeaveRequests WHERE Id=@RequestId)";
                using SqlCommand command3 = new SqlCommand(sql3, connection, transaction);
                command3.Parameters.AddWithValue("@RequestedDays", requestedDays);
                command3.Parameters.AddWithValue("@RequestId", requestId);
                await command3.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                Console.WriteLine("Leave request accepted and leave days deducted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error accepting leave request: {ex.Message}");
            }
        }

        public async Task RejectLeave(int requestId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT Status FROM LeaveRequests WHERE Id=@RequestId";
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RequestId", requestId);

            object result = await command.ExecuteScalarAsync();
            if (result == null)
            {
                Console.WriteLine("Leave request not found");
                return;
            }
            int status = (int)result;
            if (status != 0)
            {
                Console.WriteLine("Leave request is not pending");
                return;
            }

            string sql2 = "UPDATE LeaveRequests SET Status=2 WHERE Id=@RequestId";
            using SqlCommand command2 = new SqlCommand(sql2, connection);
            command2.Parameters.AddWithValue("@RequestId", requestId);

            await command2.ExecuteNonQueryAsync();
        }
    }
}

