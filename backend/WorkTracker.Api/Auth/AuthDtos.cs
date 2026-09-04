namespace WorkTracker.Api.Auth;

public record SignupRequest(string Email, string Username, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);

public record AuthResponse(Guid Id, string Email, string Username, string AccessToken, string RefreshToken);
public record MeResponse(Guid Id, string Email, string Username);
