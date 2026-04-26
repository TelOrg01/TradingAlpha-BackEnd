// ══════════════════════════════════════════════════════════════
// Resolve ambiguity between our Contracts DTOs and
// Microsoft.AspNetCore.Identity.Data types of the same name.
// This file applies to ALL .cs files in the Api project.
// ══════════════════════════════════════════════════════════════
global using LoginRequest = TradingAlpha.Contracts.Auth.LoginRequest;
global using RegisterRequest = TradingAlpha.Contracts.Auth.RegisterRequest;
global using AuthResponse = TradingAlpha.Contracts.Auth.AuthResponse;
global using UserProfileResponse = TradingAlpha.Contracts.Auth.UserProfileResponse;