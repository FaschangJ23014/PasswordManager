<div align="center">

# 🛡️ ShieldVault Password Manager

**A enterprise-grade, secure, and responsive full-stack password manager built with modern web technologies.**

[Live Demo](https://password-manager-sigma-lemon.vercel.app/) • [Backend API](https://passwordmanager-k5za.onrender.com)

[![Frontend Deployment](https://img.shields.io/badge/Frontend-Vercel-black?style=flat-square&logo=vercel)](https://password-manager-sigma-lemon.vercel.app/)
[![Backend Deployment](https://img.shields.io/badge/Backend-Render-46E3B7?style=flat-square&logo=render)](https://render.com)
[![Tech Stack](https://img.shields.io/badge/Stack-.NET%208%20|%20Svelte-blue?style=flat-square)](https://github.com/FaschangJ23014/PasswordManager)
[![Security](https://img.shields.io/badge/Security-AES--256%20|%20HIBP-success?style=flat-square)]()

</div>

## 🚀 Features
- **AES Encryption:** All passwords are encrypted at the backend before being stored in the database.
- **Master Password Protection:** Access to sensitive data is restricted and requires master password authentication.
- **Secure Architecture:** No sensitive data in the repository; uses environment variables for configuration.
- **Responsive UI:** Modern interface built with Svelte, fully responsive for desktop and mobile devices.

## 🛠️ Tech Stack
- **Backend:** C# / ASP.NET Core with Entity Framework Core.
- **Frontend:** Svelte.
- **Database:** PostgreSQL (via Supabase).
- **Deployment:** Hosted on Render(backend), Vercel(frontend) and Supabase(Database).

## 🔐 Security Approach
This project was developed with a strong focus on security best practices:
* **No Hardcoded Secrets:** All API keys and database credentials are managed via Environment Variables.
* **SQL Injection Protection:** Utilizes EF Core for parameterised queries to prevent SQL injection attacks.
* **Encryption:** AES-256 encryption ensures data remains unreadable even if the database is compromised.

## 💡 Motivation
This project was born out of personal necessity. After experiencing a data breach on a website where I had reused my credentials, I realized how critical it is to manage passwords properly. I built ShieldVault to take control of my digital security, stop password reuse, and learn how to implement robust encryption in a real-world application.

---
*Developed as a project to deepen knowledge in Web Security and Fullstack Development.*
