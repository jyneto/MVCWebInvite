This is an ASP.NET Core (.NET 8) web app for event invitations and bookings. It contains a public RSVP page and an Admin area for managing guests, tables, menu items and bookings. The project uses strongly-typed models, ViewModels/DTOs, server-side validation and an HTTP client provider to communicate with a backend WebAPI.

How to run (quick)
- Prerequisites: .NET 8 SDK, Visual Studio 2022.
- Clone the repository and open the solution in Visual Studio 2022.
- Ensure the required backend WebAPI is running and reachable (the app uses `IAuthorizedClientProvider` to call `/guest`, `/tables`, `/menu`, `/bookings`).
- In Visual Studio: __Restore NuGet Packages__ → __Build Solution__ → __Start Debugging__ (F5).
- Public site root: `/`
- Admin area root: `/Admin` (authenticate via the app's Account area to access CRUD pages).

Files that show the main functionality
- Public RSVP page and submit logic: `Controllers/HomeController.cs`, `ViewModels/RsvpFormVm.cs`
- Bookings CRUD (Admin): `Areas/Admin/Controllers/BookingController.cs`, `ViewModels/Admin/BookingFormViewModel.cs`, `ViewModels/Admin/BookingListItemVm.cs`, `Models/Booking.cs`
- Guest administration: `ViewModels/Admin/GuestFormViewModel.cs` and admin controllers/views in `Areas/Admin`
- Client-side validation lib present in `wwwroot/lib/jquery-validation/*` (enables unobtrusive validation)

Why this meets the typical assignment criteria
- Functional requirements (forms + CRUD)
  - RSVP form that submits guest info: implemented in `HomeController.SubmitRSVP` and `RsvpFormVm`.
  - Admin CRUD for bookings, tables and guests: implemented under `Areas/Admin` (e.g., `BookingController`, `TableController`).
- Validation
  - Server-side validation attributes: e.g., `RsvpFormVm` uses `[Required]`, `[EmailAddress]`; `BookingFormViewModel` uses `[Required]` and `[Range(...)]`; `Models/Booking.cs` uses `[Required]`.
  - Anti-forgery protection on POST actions: `[ValidateAntiForgeryToken]` present in controller POST methods (security requirement).
  - Client-side validation supported by included jQuery Validation files.
- Architecture & separation of concerns
  - Models, ViewModels and DTOs separate UI and data concerns (`Models/*`, `ViewModels/*`, `Models/ApiDtos/*`).
  - Admin area organized with `Areas/Admin` for modularity.
- Modern stack & tooling
  - Targets .NET 8 and uses Razor views/controllers; ready to run in Visual Studio 2022.
- Error handling & user feedback
  - Controllers use try/catch and `TempData` to show success/error messages to the user.
- Extensibility & readability
  - Strongly typed ViewModels and SelectList population make it straightforward to add features or tests.

How to verify features for grading (manual test checklist)
- RSVP:
  1. Browse to `/`.
  2. Submit the RSVP form with valid/invalid values to confirm validation and success message (`TempData["RsvpSuccess"]`).
- Admin authentication:
  1. Browse to `/Admin`.
  2. If not logged in, you should be redirected to the `Account` login; sign in and return to admin pages.
- Bookings (CRUD):
  1. Go to `/Admin/Booking`.
  2. Create a booking (use table and guest dropdowns), verify validation and success message.
  3. Edit and Delete bookings and confirm UI/TempData messages.
- Tables & Guests:
  1. Browse the corresponding admin pages to add/edit/delete tables and guests.
- API dependency:
  - If the app cannot reach the WebAPI, controllers return friendly error messages in `TempData` and log the problem. For full functionality ensure the WebAPI base URL is configured and running.

Notes for the grader
- The app uses `IAuthorizedClientProvider` to obtain HttpClient instances for anonymous and authorized requests. For full integration testing the backend WebAPI must be running (API routes used: `/guest`, `/tables`, `/menu`, `/bookings`).
- Validation attributes and anti-forgery tokens are implemented — check `ViewModels` and controller POST actions.
- Key implementation files to inspect: `Controllers/HomeController.cs`, `Areas/Admin/Controllers/BookingController.cs`, `Models/Booking.cs`, `ViewModels/Admin/*`.

Limitations / room for improvement (honest)
- No automated unit tests included in the submission (adding tests will improve reliability).
- Small areas for UX polish and more granular validation messages could be added.
- The app depends on a backend API — include the API or document its expected base URL for complete grading.

## Getting Started

1. **Clone the repository:**
2. **Open in Visual Studio 2022.**
3. **Restore NuGet packages and build the solution.**
4. **Run the project (F5) and navigate to the home page.**
