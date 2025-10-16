
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace LeavePayrollSystem.Services.Services
{
   

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendLeaveRequestNotificationAsync(string toEmail, string employeeName, DateTime startDate, DateTime endDate, string leaveType)
        {
            var subject = "New Leave Request Submitted";
            var body = $@"
                <html>
                <body>
                    <h2>Leave Request Notification</h2>
                    <p>A new leave request has been submitted and requires your attention.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <h3>Request Details:</h3>
                        <p><strong>Employee:</strong> {employeeName}</p>
                        <p><strong>Leave Type:</strong> {leaveType}</p>
                        <p><strong>Period:</strong> {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy}</p>
                        <p><strong>Duration:</strong> {(endDate - startDate).Days + 1} days</p>
                    </div>
                    
                    <p>Please log in to the HR System to review and take action on this request.</p>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: #e9ecef; border-radius: 5px;'>
                        <small>This is an automated notification from the Leave & Payroll Management System.</small>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendLeaveStatusUpdateAsync(string toEmail, string employeeName, string status, string managerComments)
        {
            var subject = $"Leave Request {status}";
            var statusColor = status == "Approved" ? "#28a745" : "#dc3545";

            var body = $@"
                <html>
                <body>
                    <h2>Leave Request Status Update</h2>
                    <p>Your leave request has been <strong style='color: {statusColor};'>{status}</strong>.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <h3>Request Details:</h3>
                        <p><strong>Employee:</strong> {employeeName}</p>
                        <p><strong>Status:</strong> <span style='color: {statusColor}; font-weight: bold;'>{status}</span></p>
                        <p><strong>Manager Comments:</strong> {managerComments ?? "No comments provided"}</p>
                    </div>
                    
                    <p>You can view the details in your HR System dashboard.</p>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: #e9ecef; border-radius: 5px;'>
                        <small>This is an automated notification from the Leave & Payroll Management System.</small>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPayrollGeneratedNotificationAsync(string toEmail, string employeeName, DateTime payPeriod, decimal netPay)
        {
            var subject = $"Payslip Available - {payPeriod:MMMM yyyy}";
            var body = $@"
                <html>
                <body>
                    <h2>Payslip Available</h2>
                    <p>Your payslip for <strong>{payPeriod:MMMM yyyy}</strong> is now available.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <h3>Payment Summary:</h3>
                        <p><strong>Employee:</strong> {employeeName}</p>
                        <p><strong>Pay Period:</strong> {payPeriod:MMMM yyyy}</p>
                        <p><strong>Net Pay:</strong> <span style='color: #28a745; font-weight: bold;'>${netPay:N2}</span></p>
                    </div>
                    
                    <p>You can view and download your detailed payslip from the HR System.</p>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: #e9ecef; border-radius: 5px;'>
                        <small>This is an automated notification from the Leave & Payroll Management System.</small>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // For development, we'll simulate email sending
                // In production, configure SMTP settings in appsettings.json

                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@company.com";

                // Only send real emails if configuration is provided
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    using var client = new SmtpClient(smtpServer, port)
                    {
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
                else
                {
                    // Log email instead of sending (for development)
                    Console.WriteLine($"=== EMAIL NOTIFICATION ===");
                    Console.WriteLine($"To: {toEmail}");
                    Console.WriteLine($"Subject: {subject}");
                    Console.WriteLine($"Body: {body}");
                    Console.WriteLine($"===========================");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't break the application
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }
}