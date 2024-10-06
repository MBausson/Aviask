namespace AviaskApi.Models;

public record PasswordResetModel(string Token, string Password, string RePassword);