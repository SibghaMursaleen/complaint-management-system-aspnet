# SIBA Complaint Management System 🏛️

A modern, high-performance web portal designed for Sukkur IBA University (SIBA) to streamline student complaints, administrative resolutions, and digital accountability.

## 🚀 Key Features

### 🎓 For Students
*   **Secure Portal**: Domain-restricted signup (@iba-suk.edu.pk).
*   **Complaint Dashboard**: Track the status of submitted tickets in real-time.
*   **Rich Submission**: Support for categories (Hostel, Transport, Academic), priority levels, and file attachments.
*   **Discussion Threads**: Live interaction with administrative staff on specific complaints.
*   **Personalization**: Custom profile pictures/logos and secure password management.

### 🛡️ For Administrators
*   **Staff Management**: Senior Admin can create and manage junior administrative staff accounts.
*   **Resolution Workflow**: Modern interface to manage, process, and resolve student issues.
*   **Immutable Archives**: Automated "Resolution Proof" snapshotting for every resolved ticket to ensure accountability.
*   **Advanced Analytics**: Overview of system-wide complaint distribution and staff performance.

## 🛠️ Technology Stack
*   **Framework**: ASP.NET Core MVC 9.0
*   **Database**: Microsoft SQL Server
*   **ORM**: Entity Framework Core
*   **Styling**: Tailwind CSS (Indigo/Slate Theme)
*   **Icons**: Font Awesome 6.4 + Heroicons
*   **Fonts**: Plus Jakarta Sans / Inter

## 📥 Installation & Setup

### Prerequisites
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [SQL Server / LocalDB](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### 1. Clone the Repository
```bash
git clone https://github.com/SibghaMursaleen/complaint-management-system-aspnet.git
cd complaint-management-system-aspnet
```

### 2. Configure Database
Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SIBAComplaints;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Apply Migrations
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```
The application will be available at `http://localhost:5167`.

## 🔑 Default Credentials (Seed Data)
| Role | Email | Password |
| :--- | :--- | :--- |
| **Senior Admin** | `senior@admin.siba.edu.pk` | `Admin@123` |
| **Staff Member** | `staff@admin.siba.edu.pk` | `Staff@123` |

## 📐 Project Structure
*   **/Controllers**: Application logic and routing.
*   **/Models**: Entity definitions and data structures.
*   **/Views**: Dynamic Razor UI templates styled with Tailwind.
*   **/Data**: DB Context and data seeding.
*   **/wwwroot**: Static assets (CSS, Uploaded Profiles, Attachments).

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
**Developed with ❤️ for Sukkur IBA University.**
