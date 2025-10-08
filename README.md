# MVCWebInvite

## Overview

MVCWebInvite is a web application built using ASP.NET Core Razor Pages (.NET 8, C# 12.0). The project is designed to manage event invitations, guest lists, and RSVPs. It provides both public-facing and admin interfaces for handling guest responses, bookings, and menu selections.

## Main Features

- **RSVP Management:** Guests can submit RSVP forms to confirm attendance.
- **Guest List Administration:** Admins can view, add, edit, and remove guests.
- **Booking System:** Admins can create,update and delete bookings for events.
- **Menu Selection:** Admins can manage menu options for guests. Also create,add, edit and delete dishes.
- **Authentication:** Login functionality for admin area access.
- **Separation of Concerns:** Uses ViewModels and DTOs for clean data handling.

## Project Structure

- **Razor Pages & MVC Controllers:** Combines Razor Pages for views and MVC controllers for logic, especially in the admin area.
- **ViewModels & DTOs:** Strongly-typed models for data transfer and form handling.
- **Areas:** Admin functionality is separated into its own area for better organization.

## Strengths

- **Modern Stack:** Utilizes .NET 8 and C# 12.0 features.
- **Separation of Concerns:** Clear distinction between data models, view models, and controllers.
- **Extensible:** Modular structure makes it easy to add new features or extend existing ones.
- **User Experience:** Clean UI with shared layouts and partial views for consistency.

## Areas for Improvement

- **Validation & Error Handling:** Could benefit from more comprehensive validation and user feedback.
- **Unit Testing:** Limited or no automated tests; adding tests would improve reliability.

## Getting Started

1. **Clone the repository:**
2. **Open in Visual Studio 2022.**
3. **Restore NuGet packages and build the solution.**
4. **Run the project (F5) and navigate to the home page.**
